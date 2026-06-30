# PRD: 인벤토리 시스템 (roguelike-inventory)

## Overview
로그라이크 런 도중 획득한 아이템을 보관·사용·정리하는 슬롯 기반 인벤토리 시스템. 플레이어는 고정된 칸 수 안에서 소비 아이템을 쌓고, 장비를 착용하며, 패시브 아이템 효과를 누적한다. 공간 퍼즐(그리드)이나 무게 계산 없이 "줍고 / 쓰고 / 버리는" 핵심 루프에 집중한다.

## Goals
- 고정 칸 수(기본 12칸)의 슬롯 기반 인벤토리를 제공한다
- 동일한 소비 아이템을 한 슬롯에 스택(최대 스택 수 제한)으로 보관한다
- 아이템을 줍기(추가) / 사용(소비) / 버리기(제거)할 수 있다
- 아이템을 소비형 / 장비형 / 패시브 3종으로 분류해 처리한다
- 인벤토리 상태 변경 시 UI가 이벤트로 자동 갱신된다
- 데이터 레이어(Module)와 UI를 분리한다

## Out of Scope
- 그리드(공간 차지) 방식 및 아이템 회전
- 무게 기반 제한
- 드래그 앤 드롭으로 슬롯 위치 재배치 (이번엔 자동 정렬만)
- 아이템 강화·합성·크래프팅
- 런 간 영구 저장(메타 프로그레션) — 이번엔 런 내 메모리만
- 멀티플레이어 / 네트워크 동기화

## Technical Requirements
- **레이어 구성** (프로젝트 컨벤션 준수)
  - `InventoryManager` — 순수 C# Manager. VContainer 싱글턴 등록·주입
  - `InventoryModule` — 순수 로직(추가/제거/스택 계산). Unity API 비의존, `new` 생성 가능
  - `IItemData` — 아이템 정의 인터페이스 (분류·최대 스택·아이콘 키 등)
- **데이터 모델**
  - 슬롯 수: 기본 12 (상수 `DEFAULT_SLOT_COUNT`)
  - 소비형 최대 스택: 상수 `MAX_STACK_SIZE` (예: 99)
  - 아이템 분류 enum: `Consumable`, `Equipment`, `Passive`
- **반응형 상태** — R3 `ReactiveProperty` / `Subject`로 슬롯 변경 통지. UI는 `Subscribe` + `AddTo`로 구독
- **아이콘·프리팹 로딩** — `AssetService`(Addressables) 경유. `Resources.Load` 금지
- **비동기** — 필요 시 UniTask `async/await`만 사용

```csharp
public enum ItemCategory { Consumable, Equipment, Passive }

public interface IItemData
{
    string Id { get; }
    ItemCategory Category { get; }
    int MaxStack { get; }   // Consumable만 > 1
    string IconKey { get; } // AssetService 조회 키
}
```

## Acceptance Criteria
- [ ] 빈 슬롯에 아이템을 주우면 해당 슬롯이 채워진다
- [ ] 동일 소비 아이템을 주우면 기존 스택에 합쳐지고, 최대 스택 초과분은 새 슬롯으로 넘어간다
- [ ] 모든 슬롯이 가득 차면 추가 줍기가 거부되고 결과가 호출자에 통지된다
- [ ] 소비형 아이템 사용 시 스택이 1 감소하고, 0이 되면 슬롯이 비워진다
- [ ] 아이템 버리기 시 슬롯에서 제거된다
- [ ] 슬롯 상태 변경 시 구독 중인 UI가 자동 갱신된다
- [ ] `InventoryModule`이 Unity API 없이 단위 테스트로 검증 가능하다

## Open Questions
- 장비형(Equipment) 아이템은 인벤토리 칸을 차지하나, 아니면 별도 장비 슬롯으로 빠지나?
- 패시브(Passive) 아이템은 슬롯을 차지하나, Isaac처럼 무제한 누적되나?
- 슬롯이 가득 찼을 때 동작 — 줍기 거부 / 바닥에 남김 / 기존 아이템과 교체?
- 아이템 사용 효과(회복·버프 등)는 이 시스템 범위인가, 별도 효과 시스템에 위임하나?
- 슬롯 수를 런 도중 확장 가능한가(업그레이드)?
