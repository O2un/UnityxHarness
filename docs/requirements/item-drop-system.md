# ItemDropSystem PRD

> 근거: `docs/spec/ItemDrop.md` · 경험치 루프의 앞단(PRD 1). 출력이 PRD 2(`ExperienceSystem`)의 입력이 된다.

## Overview

적이 죽은 자리에 경험치 아이템을 스폰하고, 플레이어가 닿으면 주움을 감지해 "경험치 N 획득"을 다음 단계로 넘긴다. 처치 보상 루프의 앞단으로, 경험치 누적·레벨 판정은 하지 않고 획득량(amount) 하나만 하류로 발행한다.

## Goals

- 적 처치 이벤트(위치·경험치량)를 구독해 `ItemActor`를 스폰하는 드롭 흐름을 만든다
- 아이템 View가 `OnTriggerEnter`로 플레이어 접촉을 감지하고 주움 이벤트를 발행하게 한다
- 주움 시 "경험치 N 획득"(amount) 입력 하나만 하류(PRD 2)로 넘긴다
- 아이템 스폰·회수를 기존 `PoolModule`(`Get`/`Release`) 재사용으로 처리한다
- 아이템 프리팹·풀 크기 등 스폰 파라미터를 데이터(`ScriptableObject`)로 구성한다

## Out of Scope

- 경험치 누적·레벨 판정 (PRD 2 `ExperienceModule` 소관)
- 아이템 종류 확장(회복·자석 등) — 이번엔 경험치 아이템 1종
- 자석 흡입·드롭 확률 테이블·아이템 UI 표시
- 주움 연출(사운드·이펙트) — 이번 범위 제외

## Technical Requirements

### 배치

- 모든 구성요소는 **`10_ProjectA`(프로젝트 A 전용)** 아래에 둔다. (`PoolModule` 등 기존 공통 인프라는 `00_CommonFramework`에서 재사용)

### 구성요소

| 구성요소 | 계층 | 책임 |
|---|---|---|
| `ItemActor` | Actor | 스폰된 아이템 1개의 수명·경험치량 보유, 주움 이벤트 발행 |
| `ItemView` | View (MonoBehaviour) | 아이템 표시 + 트리거 Collider, `OnTriggerEnter`로 플레이어 접촉 감지 |
| `ItemDropContext` | Context | 적 처치 이벤트 구독 → 스폰, 주움 이벤트 수신, 스폰 시 경험치량 주입 |
| `PoolModule` | Module (기존 재사용) | 아이템 인스턴스 `Get`/`Release` |

### 스폰 트리거 (이벤트 경유)

- **별도 Spawner 클래스를 두지 않는다.** 적 Actor가 스폰을 직접 호출하면 적이 아이템 시스템을 알게 되어 의존 방향이 깨진다.
- 적 Actor는 "죽음 + 위치 + 경험치량"만 이벤트로 발행한다. `ItemDropContext`가 이 이벤트를 구독해 스폰한다 — 단방향 유지.
- `ItemDropContext`가 이미 "스폰 트리거 수신 → `PoolModule.Get`"을 책임지므로, 그 위에 얇은 스포너 위임 계층을 더 두지 않는다.

### 스폰

- 입력: 적 처치 이벤트의 **위치 + 경험치량**으로 `ItemActor`를 스폰한다
- 경험치 양은 몬스터 데이터에서 와서 스폰 시 주입한다 (아이템이 값을 하드코딩하지 않음)
- 스폰·회수는 `PoolModule.Get`/`Release`로 처리하고 `Instantiate`/`Destroy` 직접 호출 금지

### 주움 감지 (아이템 트리거 기준)

- 주움 판정 주체는 **아이템 쪽 트리거**다. `ItemView`의 트리거 Collider가 `OnTriggerEnter`로 플레이어를 감지한다
- 감지 시 `ItemActor`가 주움 이벤트를 발행하고, 아이템은 `PoolModule.Release`로 회수된다
- 주움 이벤트 payload는 **경험치량(amount)만** 담는다 — 구체 Player·아이템 타입 참조 금지

### 계약 (하류 연결)

- 주움 결과는 "경험치 N 획득"(amount) 입력 하나로 PRD 2에 넘긴다
- `ExperienceModule` 내부를 직접 참조하지 않는다 (단방향)
- R3 구독은 `AddTo`로 lifecycle에 바인딩한다

### 데이터 (ScriptableObject)

- 아이템 프리팹·풀 크기 등 스폰 파라미터를 `ScriptableObject`로 구성한다
- 경험치 양은 여기 두지 않고 몬스터 데이터에서 스폰 시 주입한다

## Acceptance Criteria

- [ ] 적이 죽으면 그 위치에 경험치 아이템이 스폰된다
- [ ] 스폰이 적 처치 이벤트 구독으로 트리거되고, 적 Actor가 아이템 스폰/`PoolModule`을 직접 호출하지 않는다
- [ ] 스폰되는 경험치량이 몬스터 데이터에서 주입된다 (아이템 하드코딩 없음)
- [ ] 플레이어가 아이템에 닿으면 `ItemView`의 트리거 `OnTriggerEnter`로 주움이 감지된다
- [ ] 주움 시 "경험치 N 획득"(amount)이 하류로 발행되고 아이템은 `PoolModule.Release`로 회수된다
- [ ] 스폰·회수가 `PoolModule.Get`/`Release`로 처리되고 `Instantiate`/`Destroy` 직접 호출이 없다
- [ ] 주움 이벤트 payload에 구체 Player·아이템 타입 참조가 없다 (amount만)
- [ ] 경험치 누적·레벨 판정 로직이 이 시스템에 없다
- [ ] R3 구독이 모두 `AddTo`로 lifecycle에 바인딩된다

## Open Questions

- (해소) 배치 → `10_ProjectA`
- (해소) 주움 판정 주체 → 아이템 쪽 트리거
- (해소) 스폰 트리거 소유 → 별도 스포너 없이 적 처치 이벤트 구독(`ItemDropContext`)
- (해소) 주움 연출 → 이번 범위 제외
