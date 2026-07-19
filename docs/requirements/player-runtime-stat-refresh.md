# 플레이어 런타임 스탯 갱신 경로 (PlayerRuntimeStat)

> 분해 1/4 — `item-upgrade-system.md`의 "효과 반영" 축을 선행 분리한 PRD.
> 이 문서는 **강화 카드 없이** 스탯을 런타임에 바꿀 수 있는 배선만 만든다. 카드·인벤토리·보상 스폰은 후속 PRD가 얹는다.

## Overview

Project B 플랫포머의 플레이어 스탯(이동 속도 / 최대 체력 / 근접 공격력)은 현재 `PlayerMover`·`Player2DActor`·`PlayerDataStore`가 **생성 시점에 SO 값을 캡처**해 고정된다. 이 기능은 그 값들을 런타임에 갱신 가능한 단일 스탯 소스로 옮기고, 각 소비처가 변경을 즉시 반영하도록 배선한다. 이후 업그레이드 카드는 이 소스에 modifier를 더하기만 하면 되고, 다른 성장 수단(레벨업·장비)도 같은 경로를 재사용한다.

## Goals

- Project B 전용 순수 C# 스탯 상태 클래스를 두고, 최종 스탯(이동 속도·최대 체력·공격력 배수)을 R3 `ReadOnlyReactiveProperty`로 노출한다.
- base 값 + 합연산 modifier 합으로 최종 값을 계산한다. modifier 추가/전체 재설정 API를 제공한다.
- `PlayerMover`의 최대 이동 속도를 런타임에 갱신 가능하게 만든다.
- 근접 콤보 피해량을 **적중 시점에 해석**하도록 바꿔, 생성 시 굳은 `HitboxConfig.Damage`에 묶이지 않게 한다.
- 최대 체력을 증가시키면 **증가분만큼 현재 체력도 함께 오르는** 갱신 경로를 만든다.
- Play 모드에서 키 입력으로 스탯 변경을 발생시켜 세 경로가 모두 즉시 반영되는지 확인할 임시 개발 하네스를 둔다.

## Out of Scope

- 업그레이드 카드 SO·인벤토리 연동 — 분해 2/4.
- 보상 카드 스폰·선택 UI — 분해 3/4.
- 패시브 스킬(크리티컬·유도 미사일) — 분해 4/4.
- 곱연산(%) modifier. 합연산만 지원한다.
- 원거리 스킬(`RangedSkillData.Damage`) 피해량 강화. 이번 범위는 이동·체력·근접 공격력 3종이다.
- 적(NPC) 스탯의 런타임 갱신.
- 스탯 값의 저장/세션 간 영속화.
- 스탯 표시 UI(HUD 수치 갱신).
- 자동화 테스트 검증 — 테스트 어셈블리(asmdef) 도입 전까지 검증하지 않는다.

## Technical Requirements

### 배치

- 신규 코드는 `Assets/20_ProejctB/01_Scripts/Player/Stat/` 아래에 둔다 (폴더명 오타 `ProejctB`는 현행 유지).
- 스탯 상태 클래스는 **Unity API 비의존 순수 C#**으로 작성한다. `MonoBehaviour` 상속·`Transform`/`GameObject` 조작·씬 탐색을 하지 않는다.

### 스탯 소스

- `UpgradeStatType` enum(`AttackDamage`, `MaxHealth`, `MoveSpeed`)을 정의한다. 후속 PRD의 카드 데이터가 이 enum을 그대로 참조한다.
- 읽기 인터페이스와 쓰기 인터페이스를 분리한다. 소비처(Mover·Actor·Context)는 읽기 인터페이스만 의존한다.

```csharp
namespace O2un.ProjectB.Platformer
{
    public enum UpgradeStatType { AttackDamage, MaxHealth, MoveSpeed }

    public interface IPlayerStatReader
    {
        ReadOnlyReactiveProperty<float> MoveSpeed { get; }
        ReadOnlyReactiveProperty<int> MaxHealth { get; }
        ReadOnlyReactiveProperty<int> AttackBonus { get; }
    }

    public interface IPlayerStatWriter
    {
        void SetBase(UpgradeStatType stat, float baseValue);
        void AddModifier(UpgradeStatType stat, float value);
        void ClearModifiers();
    }
}
```

- 구현체(`PlayerStatModule`)는 stat별 base와 modifier 합을 들고, `base + Σmodifier`를 최종값으로 발행한다. 값이 실제로 바뀔 때만 발행한다.
- base 값은 씬 시작 시 기존 SO에서 주입한다: `MoveSpeed` ← `MovementData.MaxMoveSpeed`, `MaxHealth` ← `PlayerDataStore.MaxHP` 초기값. `AttackBonus`의 base는 0이며, 스테이지 피해량에 **가산**되는 보너스로 다룬다(스테이지별 상대 밸런스를 유지하기 위해 배수·치환이 아니라 가산).
- DI: `ProjectBSceneScope`에 `Lifetime.Singleton`으로 등록하고 `IPlayerStatReader`/`IPlayerStatWriter`로 노출한다. 룸을 넘어 유지되어야 하므로 `RoomSceneScope`가 아니다.

### 이동 속도

- `PlayerMover._maxMoveSpeed`의 `readonly`를 풀고 갱신 메서드(`SetMaxMoveSpeed(float)`)를 추가한다. 나머지 필드는 그대로 둔다.
- `Player2DActor`가 `IPlayerStatReader.MoveSpeed`를 구독해 `_mover.SetMaxMoveSpeed`를 호출한다. 구독은 `_disposables`에 `AddTo`한다.
- 가속/감속 로직은 건드리지 않는다. 목표 속도만 바뀌므로 갱신 즉시 다음 `FixedTick`부터 반영된다.

### 근접 공격력

- 현재 `Player2DActor.CreateStageHitboxes`가 `new HitboxConfig(comboData.GetStage(i + 1).Damage, ...)`로 피해량을 **굳혀서** 만들고, `OnHit` 핸들러가 `e.Target.ApplyDamage(e.Damage)`로 그 값을 그대로 쓴다.
- `HitboxConfig`는 `readonly struct`이므로 사후 변경할 수 없다. 따라서 **hitbox를 재생성하지 말고**, `OnHit` 핸들러에서 최종 피해량을 해석하도록 바꾼다. 스테이지 인덱스를 클로저로 캡처하고 `e.Damage` 대신 `스테이지 base Damage + AttackBonus.CurrentValue`를 적용한다.
- 최종 피해량은 최소 1로 하한을 둔다(음수 modifier가 들어와도 회복이 되지 않도록).

### 최대 체력

- `PlayerDataStore`는 현재 `_maxHP`를 바꿀 수단이 없다(`IPlayerDataWriter`에 `VaryHP`/`SetCurrentHP`만 존재). `IPlayerDataWriter`에 `VaryMaxHP(int delta)`를 추가하고 `PlayerDataStore`에 구현한다.
  - `VaryMaxHP`는 최대 체력을 `delta`만큼 올리고 **현재 체력도 같은 `delta`만큼 가산**한다(전체 회복 아님). 최대 체력 하한은 1이며, 감소 시 현재 체력을 새 최대치로 clamp한다.
  - 이 변경은 `00_CommonFramework` 수정이지만 Project B 구체 타입을 참조하지 않는 **범용 확장**이므로 공통에 두는 것이 맞다. 공통이 Project B를 참조하는 방향은 여전히 금지다.
- `Player2DContext`(또는 스탯 배선을 담당하는 지점)가 `IPlayerStatReader.MaxHealth` 변화량을 `VaryMaxHP`로 전달한다. 이전 값 대비 **차분**을 넘겨야 하며, 최종값을 그대로 넘기지 않는다.
- `PlayerHealthAdapter`는 `_reader.MaxHP.CurrentValue`를 매번 읽으므로 수정 없이 새 최대치를 따른다.

### 개발 하네스

- `Assets/20_ProejctB/01_Scripts/Player/Dev/`에 `RoomSpawnTestTrigger`와 동일한 패턴의 임시 `MonoBehaviour`를 둔다: `ISceneInitializable` 구현, `[Inject]` 생성자 주입, `Keyboard.current`로 키 입력 감지.
  - 키 3개에 각각 `AddModifier(MoveSpeed, +N)`, `AddModifier(MaxHealth, +N)`, `AddModifier(AttackDamage, +N)`를 매핑한다.
  - 후속 PRD(보상 카드)가 실제 진입점을 제공하면 이 하네스는 제거한다. 파일 상단에 그 취지를 한 줄로 남긴다.

## Acceptance Criteria

- [ ] Play 모드에서 이동 속도 증가 키를 누르면 **즉시** 플레이어 이동 속도가 눈에 띄게 빨라진다.
- [ ] 이동 속도 증가 키를 두 번 누르면 효과가 누적된다 (덮어쓰기가 아님).
- [ ] 최대 체력 증가 키를 누르면 최대 체력이 오르고, 현재 체력도 증가분만큼 함께 오른다 (전체 회복이 아니다).
- [ ] 피해를 입어 체력이 깎인 상태에서 최대 체력 증가 키를 눌러도 체력이 최대치로 회복되지 않는다.
- [ ] 공격력 증가 키를 누른 뒤 적을 공격하면 쓰러뜨리는 데 필요한 타격 수가 줄어든다.
- [ ] 공격력 증가는 콤보 진행 중에 눌러도 다음 타격부터 반영된다 (재시작 불필요).
- [ ] 아무 키도 누르지 않은 상태의 이동·체력·공격 감각이 기존 플레이와 동일하다.
- [ ] 룸을 전환해도 적용된 스탯 변경이 유지된다.
- [ ] Play 모드 진입부터 2개 룸 클리어까지 콘솔 에러가 없다.

## Open Questions

- `AttackBonus`를 가산으로 두면 콤보 후반 스테이지의 상대 우위가 줄어든다. 스테이지 비율을 유지하는 배수 방식이 더 맞는지 밸런스 확인 필요. (현 결정: 가산)
- 이동 속도 modifier의 1회 증가폭·최대 체력 증가폭의 기본 수치는 카드 데이터(분해 2/4)에서 확정한다. 이 PRD의 하네스는 임의 값으로 진행한다.
- `IPlayerDataWriter` 구현체는 현재 `PlayerDataStore` 하나뿐이므로 메서드 추가로 깨지는 다른 구현체는 없다. 다만 Project A(`10_ProjectA/GameSceneScope`)도 같은 store를 등록하므로, `VaryMaxHP` 추가가 Project A 밸런스에 영향이 없는지(현재 호출처 없음) 확인만 한다.
