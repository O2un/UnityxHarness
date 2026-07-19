# 아이템 획득 & 능력치 강화 (ItemUpgradeSystem)

## Overview

Project B 플랫포머에서 룸을 클리어하면 문 앞에 보상 카드 2장이 스폰되고, 플레이어가 그중 하나를 골라 능력치 강화 또는 스킬 습득 효과를 얻는 룸 단위 성장 루프다. 현재 룸 진행은 `RoomModule`이 클리어를 판정해 `OnRoomCleared`를 쏘고 `RoomDoorView`가 문을 여는 데서 끝난다. 이 기능은 그 사이에 "선택 1회"라는 성장 스텝을 끼워 넣어, 반복되는 룸 클리어에 누적 성장 동기를 만든다.

## Goals

- 룸 클리어 시점(`IRoomProgression.OnRoomCleared`)에 문 앞 기준 Transform 오프셋 위치로 보상 카드 후보 2장을 스폰한다.
- 플레이어가 카드 후보에 접근해 상호작용하면 해당 카드를 획득 확정하고 나머지 후보를 회수한다.
- 공통 `InventoryManager`(`IInventoryWriter.Add`)를 통해 카드를 인벤토리에 담고, 그 결과(`AddResult`)로 선택 확정 여부를 판단한다.
- 획득 카드는 인벤토리 슬롯을 소비하며, 이 기능이 사용하는 상한은 **6칸**이다.
- 획득한 Passive 카드 목록으로부터 최종 스탯을 계산하는 순수 C# 집계 로직을 두고, 획득 즉시 반영한다.
- 카드 4종(패시브 스킬 습득 / 공격 강화 / 생존 강화 / 이동 강화)을 데이터(ScriptableObject)로 정의하고, 각각 대응 시스템에 반영한다.
- 카드 데이터·효과 계약·진행 로직·View·집계 Module을 모두 `Assets/20_ProejctB/` 아래에 둔다.

## Out of Scope

- 슬롯이 가득 찼을 때(획득 카드 6장 도달)의 **교체 UI**. 후보를 유지하고 안내만 표시한다.
- 곱연산(%) modifier. 이번에는 **합연산만** 지원한다.
- 카드 아트·아이콘 정식 에셋. 플레이스홀더 스프라이트로 진행한다.
- 카드 등급·희귀도·가중치 추첨 테이블. 이번에는 후보 풀에서 중복 없이 균등 2장을 뽑는다.
- 카드 리롤·스킵·재선택.
- 인벤토리 화면 UI(획득 목록 표시). 이번 범위는 획득과 효과 반영까지다.
- 획득 카드의 저장/세션 간 영속화.
- 자동화 테스트 검증 — 테스트 어셈블리(asmdef) 도입 전까지 검증하지 않는다.

## Technical Requirements

### 배치

- 신규 코드는 전부 `Assets/20_ProejctB/01_Scripts/` 아래에 둔다 (폴더명 오타 `ProejctB`는 현행 유지). 기능 단위 폴더 `Reward/`(또는 `ItemUpgrade/`)를 새로 만든다.
- 공통 `00_CommonFramework`의 `InventoryManager`·`IItemData`는 **수정하지 않는다.** Project B 구체 타입을 공통이 참조하면 안 된다.

### 카드 데이터

- 카드 SO는 공통 `O2un.Manager.IItemData`를 구현한다. `Category`는 `ItemCategory.Passive`를 반환한다 (`InventoryModule.AddItem`이 Passive를 일반 슬롯 `AddToFirstEmpty`로 넣으므로 스택 없이 1칸 1장으로 들어간다).
- `MaxStack`은 Passive 경로에서 사용되지 않으므로 1을 반환한다.
- **획득 상한은 6장**이다. 인벤토리 일반 슬롯이 12칸이더라도 이 기능의 진행 로직이 획득 카드 수 6을 상한으로 검사하고, 도달 시 `IInventoryWriter.Add`를 호출하지 않은 채 슬롯 부족으로 처리한다. 상한은 상수(`MAX_UPGRADE_SLOT = 6`)로 둔다.
- 수치 modifier·대상 스탯·패시브 스킬 종류는 `IItemData`가 아니라 Project B 전용 카드 효과 인터페이스에 둔다.

```csharp
namespace O2un.ProjectB.Platformer
{
    public enum UpgradeCardKind { StatModifier, PassiveSkill }
    public enum UpgradeStatType { AttackDamage, MaxHealth, MoveSpeed }
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

- 아이콘·카드 스프라이트는 플레이스홀더로 진행한다. `IItemData.IconKey`는 플레이스홀더 키를 반환하고, 카드 View는 단색/기본 스프라이트로 표시한다.

- 카드 SO(`UpgradeCardSO : ScriptableObject, IUpgradeCardData`)와 후보 풀 SO(`UpgradeCardPoolSO`)를 만든다. 풀 SO는 소비처가 추첨 로직 하나뿐이므로 전역 `RegisterInstance` 대신 `WithParameter`로 넘긴다.

### 진행 로직 (Manager/Module)

- 순수 C# 클래스가 다음을 담당한다: 후보 2장 추첨(중복 없음), `IInventoryWriter.Add` 호출 결과 해석, 선택 확정/거부 판정.
- 확정 규칙:
  - 획득 카드 수가 6 미만이고 `AddResult.Added`/`Stacked` → 선택 확정, 나머지 후보 회수, 선택 완료 신호 발행.
  - 획득 카드 수가 이미 6이거나 `AddResult.SlotsFull` → 후보 유지, 슬롯 부족 안내 신호 발행. 재시도 가능.
- 외부 알림은 C# event가 아니라 R3 `Subject`/`Observable`로 노출한다 (`OnCandidatesSpawned`, `OnCardSelected`, `OnSlotsFull`).
- Unity API(`Transform`/`GameObject`/씬 탐색)에 의존하지 않는다. 배치·프롬프트·시각 표현은 View가 맡는다.

### View

- 카드 View는 `RoomDoorView`와 동일한 패턴을 따른다: `Collider2D` 트리거로 플레이어 범위 판정(`LayerMask` 비교), `IInputReader.IsAttackPressed` 구독 후 핸들러 안에서 상태 검사, 프롬프트 루트 `SetActive`.
- 카드 스폰 위치는 룸 씬의 `RoomSceneScope`가 직렬화 필드로 제공하는 **기준 Transform 하나**(`_rewardSpawnPoint`)를 쓰고, 후보 2장은 그 지점 기준 좌우 오프셋으로 배치한다. 지점마다 오브젝트를 두지 않는다. `GameObject.Find` 등 씬 탐색 금지.
- 프리팹 인스턴스화는 `IPoolService`(`PoolManager`) 또는 Addressables 기반 `AssetService`를 사용한다. `Resources.Load` 금지.

### DI

- 공통 `InventoryManager`는 현재 Project B 스코프에 **등록되어 있지 않다** (`ProjectBSceneScope.Configure`에 없음, `10_ProjectA/GameSceneScope`에만 있음). `ProjectBSceneScope`에 싱글턴으로 등록하고 `IInventoryReader`/`IInventoryWriter`/`IDisposable`로 노출한다.
- 보상 진행 로직도 `ProjectBSceneScope`에 싱글턴 등록한다 (룸 씬을 넘어 획득이 누적돼야 하므로 `RoomSceneScope`가 아니다).
- 카드 View는 룸 씬에서 생성되므로 `RoomSceneScope`의 빌드 콜백에서 `resolver.InjectGameObject`로 주입한다.

### 효과 반영

모든 효과는 **획득 즉시** 반영된다. `PlayerMover`·`MeleeComboModule`·`PlayerHealthAdapter`가 생성자에서 SO 값을 캡처하고 있으므로, 각 Module에 런타임 값 갱신 경로를 신설한다 (집계 Module의 결과를 구독해 갱신).

- **패시브 스킬 습득**: 액티브 스킬이 아니라 **상시 적용되는 패시브 효과**다. 이번 범위의 2종:
  - `CriticalOnHit` — 공격 적중 시 일정 확률로 피해 배수 적용. `MeleeComboModule`의 히트 처리 경로에서 판정한다.
  - `HomingMissile` — 공격 적중 시 유도 미사일 추가 발사. 기존 `RangedSkillModule`의 발사 경로를 재사용하되, 입력이 아니라 적중 이벤트로 트리거된다.
  - 해금 상태는 집계 Module이 획득 카드에서 산출한 패시브 스킬 집합으로 조회한다. `Player2DActor`의 `rangedRefs` 생성 조건은 건드리지 않고, 발동 여부만 해금 집합으로 게이트한다.
- **공격 강화**: `MeleeComboData`의 스테이지 Damage로 `HitboxConfig`가 생성되는 지점(`Player2DActor.CreateStageHitboxes`)에 집계된 최종 공격력을 반영한다. 현재는 SO 값을 그대로 쓰므로 집계값을 거치도록 바꾼다.
- **생존 강화**: `Player2DContext.InitHealth()`가 만드는 `PlayerHealthAdapter`의 최대 체력에 반영한다. **최대 체력 증가분만큼 현재 체력도 같이 증가**한다 (전체 회복이 아니라 증가분 가산).
- **이동 강화**: `PlayerMover`의 최대 이동 속도를 런타임에 갱신한다.
- 집계 Module은 `base값 + 획득 Passive 카드 modifier 합`(합연산 전용)으로 최종 스탯을 계산하는 순수 C# 클래스다. `IInventoryReader.Slots`를 구독해 획득 목록을 읽고, 카드 없음 / 한 장 / 동일 계열 두 장 누적을 모두 처리한다. Unity API 비의존으로 분리한다.

## Acceptance Criteria

- [ ] 룸의 적을 모두 처치하면 문이 열리는 것과 함께 문 앞에 카드 2장이 나타난다.
- [ ] 카드는 서로 다른 종류이며, 룸마다 다시 추첨된다.
- [ ] 카드 범위 안에 들어가면 상호작용 프롬프트가 표시되고, 범위를 벗어나면 사라진다.
- [ ] 한 카드를 선택하면 그 카드만 사라지지 않고 **두 후보가 모두 사라진다** (선택된 것은 획득, 나머지는 회수).
- [ ] 선택 후 다시 상호작용해도 추가 획득이 일어나지 않는다 (룸당 1회).
- [ ] 카드 6장을 획득한 뒤 선택하면 후보가 사라지지 않고 슬롯 부족 안내가 표시된다.
- [ ] 이동 강화 카드를 획득하면 **획득 즉시** 이동 속도가 눈에 띄게 증가한다 (룸 전환을 기다리지 않는다).
- [ ] 생존 강화 카드를 획득하면 최대 체력이 증가하고, 현재 체력도 증가분만큼 함께 오른다.
- [ ] 공격 강화 카드를 획득하면 적을 쓰러뜨리는 데 필요한 타격 수가 줄어든다.
- [ ] 크리티컬 패시브 획득 전에는 피해량이 일정하고, 획득 후에는 일부 타격의 피해량이 커진다.
- [ ] 유도 미사일 패시브 획득 전에는 근접 적중 시 추가 투사체가 없고, 획득 후에는 발사된다.
- [ ] 같은 계열 강화 카드를 두 번 획득하면 효과가 누적된다 (덮어쓰기가 아님).
- [ ] 카드를 하나도 획득하지 않은 상태의 스탯이 기존 플레이와 동일하다.
- [ ] 룸을 전환해도 이전 룸에서 획득한 강화가 유지된다.
- [ ] Play 모드 진입부터 2개 룸 클리어까지 콘솔 에러가 없다.

## 결정 사항 (2026-07-19 확정)

1. **스킬 카드는 패시브 스킬이다** — 액티브가 아니라 공격 적중 시 크리티컬, 유도 미사일 추가 발사 같은 상시 효과. 공통 `SkillModule`/`SkillDefinitionSO`는 도입하지 않는다.
2. **획득 즉시 반영** — 룸 전환 대기 없이 각 Module에 런타임 갱신 경로를 신설한다.
3. **합연산만 구현** — 곱연산(%)은 이번 범위 밖.
4. **최대 체력 증가 시 현재 체력도 증가분만큼 함께 오른다.**
5. **스폰 위치는 기준 Transform 1개 + 오프셋** — 룸 씬마다 지점 2개를 배치하지 않는다.
6. **카드 비주얼은 임시 스프라이트**로 진행한다.
7. **인벤토리 슬롯을 소비하고, 상한은 6칸**이다 (12칸 전부 쓰지 않음).

## 남은 선행 작업

- `ProjectBSceneScope`에 `InventoryManager` 등록 (현재 Project A에만 있음).
- 룸 씬(RoomData_1/2)에 `_rewardSpawnPoint` Transform 추가 — 씬 변경이므로 별도 승인 필요.
