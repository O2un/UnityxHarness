# 업그레이드 카드 데이터 & 인벤토리 연동 (UpgradeCardData)

> 분해 2/4 — `item-upgrade-system.md`의 "카드 데이터 · 인벤토리 · 집계" 축.
> **선행 의존: `player-runtime-stat-refresh.md`(분해 1/4).** 이 PRD는 그 PRD가 만든 `IPlayerStatWriter`에 카드 modifier를 흘려보낸다. 카드를 씬에 스폰하고 고르는 UX는 후속(분해 3/4)이다.

## Overview

업그레이드 카드를 ScriptableObject 데이터로 정의하고, 공통 `InventoryManager`에 담아 보관하며, 보유 카드 목록으로부터 최종 스탯을 집계해 플레이어 스탯 소스에 반영하는 데이터 계층을 만든다. 현재 Project B는 `InventoryManager`를 스코프에 등록조차 하지 않고 있어(`ProjectBSceneScope.Configure`에 없음, `10_ProjectA/GameSceneScope`에만 있음), 등록부터 시작한다. 이 PRD가 끝나면 "카드 한 장을 인벤토리에 넣으면 스탯이 즉시 오른다"가 코드 경로로 성립하고, 남은 것은 그 진입점을 룸 보상으로 잇는 일이다.

## Goals

- 카드 SO(`UpgradeCardSO`)와 후보 풀 SO(`UpgradeCardPoolSO`)를 정의한다.
- 카드 SO가 공통 `O2un.Manager.IItemData`를 구현해 `InventoryManager`에 그대로 담기게 한다.
- `ProjectBSceneScope`에 `InventoryManager`를 등록해 Project B에서 인벤토리를 쓸 수 있게 한다.
- 보유 카드 목록에서 스탯 modifier를 합산하는 순수 C# 집계 Module을 두고, 결과를 `IPlayerStatWriter`에 반영한다.
- 획득 상한 6칸을 검사하고 결과를 해석하는 순수 C# 획득 로직을 두고, 결과를 R3 `Subject`로 알린다.
- 획득 즉시 스탯이 갱신되고, 같은 계열 카드 2장이 누적된다.

## Out of Scope

- 카드 프리팹·View·상호작용·프롬프트 — 분해 3/4.
- 룸 클리어 연동·후보 2장 추첨의 씬 배치 — 분해 3/4. (추첨 로직 자체는 이 PRD에서 순수 C#으로 만들고, 호출은 후속이 한다.)
- 패시브 스킬 카드의 **실제 효과 발동**(크리티컬·유도 미사일) — 분해 4/4. 이 PRD는 패시브 종류를 데이터로 담고 해금 집합을 산출하는 데까지만 한다.
- 슬롯이 가득 찼을 때의 교체 UI. 안내 신호 발행까지만 한다.
- 곱연산(%) modifier. 합연산만 지원한다.
- 카드 등급·희귀도·가중치 추첨 테이블. 후보 풀에서 중복 없이 균등 2장을 뽑는다.
- 카드 아트·아이콘 정식 에셋. `IconKey`는 플레이스홀더 키를 반환한다.
- 인벤토리 화면 UI(획득 목록 표시).
- 획득 카드의 저장/세션 간 영속화.
- 자동화 테스트 검증 — 테스트 어셈블리(asmdef) 도입 전까지 검증하지 않는다.

## Technical Requirements

### 배치

- 신규 코드는 `Assets/20_ProejctB/01_Scripts/Reward/` 아래에 둔다 (폴더명 오타 `ProejctB`는 현행 유지).
- 공통 `00_CommonFramework`의 `InventoryManager`·`InventoryModule`·`IItemData`는 **수정하지 않는다.** 공통이 Project B 구체 타입을 참조하면 안 된다.

### 카드 데이터

- 카드는 공통 `O2un.Manager.IItemData`를 구현한다.
  - `Category`는 `ItemCategory.Passive`를 반환한다. `InventoryModule.AddItem`이 Passive를 일반 슬롯 `AddToFirstEmpty`로 넣으므로 스택 없이 1칸 1장으로 들어간다.
  - `MaxStack`은 Passive 경로에서 쓰이지 않으므로 1을 반환한다.
  - `Id`는 SO별 고유 문자열이어야 한다(중복 추첨 방지와 인벤토리 조회에 쓰인다).
- 수치 modifier·대상 스탯·패시브 종류는 `IItemData`가 아니라 Project B 전용 인터페이스에 둔다.
- `UpgradeStatType`은 분해 1/4에서 이미 정의된 것을 **재정의하지 않고 그대로 쓴다.**

```csharp
namespace O2un.ProjectB.Platformer
{
    public enum UpgradeCardKind { StatModifier, PassiveSkill }
    public enum PassiveSkillType { CriticalOnHit, HomingMissile }

    public interface IUpgradeCardData : O2un.Manager.IItemData
    {
        UpgradeCardKind Kind { get; }
        UpgradeStatType TargetStat { get; }
        float ModifierValue { get; }
        PassiveSkillType PassiveSkill { get; }
        string DisplayName { get; }
    }
}
```

- `UpgradeCardSO : ScriptableObject, IUpgradeCardData`에 `[CreateAssetMenu(menuName = "ProjectB/Platformer/UpgradeCard")]`를 붙인다. 기존 SO(`MovementData`, `MeleeComboData`)의 메뉴 경로 컨벤션을 따른다.
- `UpgradeCardPoolSO`는 `UpgradeCardSO` 리스트를 들고, 후보 풀 역할만 한다. 소비처가 추첨 로직 하나뿐이므로 전역 `RegisterInstance` 대신 등록 시 `WithParameter(...)`로 넘긴다(`ProjectBSceneScope`가 `StageDataSO`를 `RoomModule`에 넘기는 방식과 동일).
- 카드 4종을 에셋으로 만든다: 패시브 스킬 습득 / 공격 강화(`AttackDamage`) / 생존 강화(`MaxHealth`) / 이동 강화(`MoveSpeed`). 패시브는 `CriticalOnHit`·`HomingMissile` 2종으로 각각 하나씩 둔다. **에셋 생성은 씬·에셋 변경이므로 별도 승인 대상이다.**

### DI

- `ProjectBSceneScope`에 `InventoryManager`를 `Lifetime.Singleton`으로 등록하고 `IInventoryReader`·`IInventoryWriter`·`IDisposable`로 노출한다. `InventoryManager`는 `IInitializable`도 구현하므로 `RegisterEntryPoint` 대신 `Register(...).AsImplementedInterfaces()` 또는 명시적 `As<>` 나열 중 기존 스코프 스타일에 맞춘다.
- 집계 Module과 획득 로직도 `ProjectBSceneScope`에 싱글턴 등록한다. 획득이 룸을 넘어 누적돼야 하므로 `RoomSceneScope`가 아니다.

### 집계 Module

- `IInventoryReader.Slots`(R3 `ReadOnlyReactiveProperty<IReadOnlyList<InventorySlot>>`)를 구독해, 슬롯이 갱신될 때마다 보유 카드에서 최종 스탯을 다시 계산한다.
  - `Slots`는 스냅샷 배열을 발행하므로 매번 전체를 순회해 다시 합산해도 된다. 증분 계산을 만들지 않는다.
  - 슬롯의 `Item`이 `IUpgradeCardData`가 아닌 경우는 건너뛴다(다른 아이템이 같은 인벤토리를 쓸 수 있다).
- 계산 결과를 `IPlayerStatWriter`에 반영한다: `ClearModifiers()` 후 stat별 합계를 `AddModifier`로 한 번씩 넣는 방식으로 **재계산이 멱등**하게 한다. 델타 누적 방식은 쓰지 않는다.
- 획득한 `PassiveSkillType` 집합을 조회 가능한 형태로 노출한다(`bool IsUnlocked(PassiveSkillType)` 또는 읽기 전용 집합). 분해 4/4가 이 조회로 발동을 게이트한다.
- 카드 없음 / 한 장 / 동일 계열 두 장 누적을 모두 처리한다.
- **Unity API 비의존 순수 C#**으로 작성한다. `MonoBehaviour` 상속·`Transform`/`GameObject` 조작·씬 탐색을 하지 않는다. `Mathf`·`Random` 같은 정적 유틸과 SO를 값으로 받아 쓰는 것은 무방하다.

### 획득 로직

- 순수 C# 클래스가 담당한다: 후보 2장 추첨(중복 없음, 균등), `IInventoryWriter.Add` 호출과 결과 해석, 확정/거부 판정.
- 획득 상한은 **6장**이며 상수 `MAX_UPGRADE_SLOT = 6`으로 둔다. 인벤토리 일반 슬롯이 12칸이더라도 이 상한을 자체 검사한다.
- 확정 규칙:
  - 획득 카드 수 < 6 이고 `AddResult.Added`/`Stacked` → 선택 확정, 선택 완료 신호 발행.
  - 획득 카드 수가 이미 6 → `IInventoryWriter.Add`를 **호출하지 않고** 슬롯 부족으로 처리, 슬롯 부족 신호 발행. 재시도 가능.
  - `AddResult.SlotsFull` → 슬롯 부족 신호 발행. 재시도 가능.
- 외부 알림은 C# `event`가 아니라 R3 `Subject`/`Observable`로 노출한다: `OnCandidatesDrawn`, `OnCardAcquired`, `OnSlotsFull`.
- 추첨은 후보 풀에서 서로 다른 2장을 뽑는다. 풀 크기가 2 미만이면 뽑은 만큼만 반환하고 경고 로그를 남긴다.
- **Unity API 비의존 순수 C#**으로 작성한다. 배치·프롬프트·시각 표현은 후속 PRD의 View가 맡는다.

### 개발 하네스

- 분해 1/4이 남긴 임시 스탯 하네스를 이 PRD의 하네스로 대체한다: 키 입력으로 "후보 2장 추첨 → 첫 번째 카드 획득"을 실행해 데이터 경로를 Play 모드로 확인한다.
- 분해 3/4가 실제 보상 진입점을 제공하면 이 하네스는 제거한다. 파일 상단에 그 취지를 한 줄로 남긴다.

## Acceptance Criteria

- [ ] 개발 하네스 키로 이동 강화 카드를 획득하면 **즉시** 이동 속도가 눈에 띄게 증가한다.
- [ ] 생존 강화 카드를 획득하면 최대 체력이 증가하고, 현재 체력도 증가분만큼 함께 오른다.
- [ ] 공격 강화 카드를 획득하면 적을 쓰러뜨리는 데 필요한 타격 수가 줄어든다.
- [ ] 같은 계열 강화 카드를 두 번 획득하면 효과가 누적된다 (덮어쓰기가 아님).
- [ ] 카드를 하나도 획득하지 않은 상태의 스탯이 기존 플레이와 동일하다.
- [ ] 추첨된 후보 2장은 항상 서로 다른 카드다.
- [ ] 카드를 6장 획득한 뒤 추가 획득을 시도하면 슬롯 부족 로그가 남고 7번째 카드가 들어가지 않는다.
- [ ] 슬롯 부족 상태에서 시도해도 스탯이 변하지 않고 콘솔 에러가 발생하지 않는다.
- [ ] 룸을 전환해도 이전 룸에서 획득한 강화가 유지된다.
- [ ] 패시브 스킬 카드를 획득하면 해금 집합에 반영된 것이 로그로 확인된다 (효과 발동은 분해 4/4 범위).
- [ ] Play 모드 진입부터 2개 룸 클리어까지 콘솔 에러가 없다.

## Open Questions

- 카드별 modifier 수치(이동 속도 +?, 최대 체력 +?, 공격력 +?)의 초기값은 미정. 첫 구현은 체감 가능한 임의 값으로 두고 플레이 후 조정한다.
- 패시브 스킬 카드를 중복 획득했을 때의 처리 — 현재는 해금 집합이라 두 번째 장이 무효가 된다. 중복을 추첨에서 제외할지, 무효인 채 슬롯만 소비하게 둘지 결정 필요.
- `IconKey` 플레이스홀더 키의 실제 문자열 규약(빈 문자열 / 고정 키 / 카드 Id 재사용) 미정.
