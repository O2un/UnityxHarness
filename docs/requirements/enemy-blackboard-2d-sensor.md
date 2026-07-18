# enemy-blackboard-2d-sensor

> 분해 순서 2/4 (platform-enemy-ai 분해) · Depends On: 1 sound-signal-channel · Provides: 채워진 `EnemyBlackboard` (읽기 전용 계약)

## Overview
2D 횡스크롤 적이 세상을 인지하는 층. View가 소유한 센서가 `Physics2D` 레이(시야·지면/낭떠러지·벽)와 R3 구독(소리·피격)으로 인지 결과를 만들고, `EnemyBlackboard`를 채운다. 상태 머신은 이 Blackboard를 **읽기 전용**으로 소비한다. 이번 범위는 인지 데이터 생산까지이며, 그 데이터를 쓰는 상태·전이는 다음 단계에서 만든다.

## Goals
- `EnemyBlackboard`를 확장해 순간 신호와 보존 값을 분리한다 (`LastKnownTargetPosition`, `TimeSincePerceived`)
- 시야·지면/낭떠러지·벽을 `Physics2D` 레이로 판정하는 2D 센서를 View 소유로 정의한다
- 후보 1의 공통 소리 채널을 구독해 청취 반경 판정으로 `HeardSoundThisTick`을 채운다
- 피격 R3 이벤트를 구독해 피격 신호를 Blackboard에 반영한다
- 레이 쿼리를 매 프레임이 아니라 **조절 가능한 센싱 주기** 간격으로만 실행하는 스케줄러를 둔다
- 센서 → AI **단방향** 계약을 세운다: 상태·조건은 Blackboard에 쓰지 않는다

## Out of Scope
- 소리 채널 자체의 정의(발행 인터페이스·페이로드 타입) — **후보 1(sound-signal-channel)** 범위. 여기서는 구독만 한다
- `PatrolState`·`DetectState`·`ChaseState`·`AttackState`, 전이 조건 5종, `EnemyStateSO`/`EnemyTransitionConditionSO` 저작, 전이 그래프, `PlatformEnemyAIProfile`·`PlatformEnemyAI` — **후보 3(platform-enemy-fsm)** 범위
- 애니메이션 이벤트 히트박스 On/Off, 공격 쿨타임 SO, Attack→Chase 복귀 판정 — **후보 4(enemy-melee-attack-execution)** 범위
- 넉백·경직 반응
- 원거리 공격 적
- 다수 적 동시 실행 성능 최적화
- 공통 코어(`00_CommonFramework`)의 FSM 베이스·SO 저작 계층 재작성

## Technical Requirements

### 배치
- 2D 센서·센싱 주기 스케줄러·프로파일용 센싱 데이터는 `Assets/20_ProjectB` 아래에 둔다
- `EnemyBlackboard` 확장은 **원본 배치 규칙을 따른다** (기존 `EnemyBlackboard`가 있는 위치 기준). 확장이 공통 코어 재작성이 되지 않게 하고, Player·특정 씬 등 구체 참조가 공통 베이스로 역류하지 않게 한다
- `00_CommonFramework`의 `O2un.AI` FSM 베이스·SO 저작 계층·`ActorManager`는 **참조만** 한다

### 센싱
- **시야**: 전방으로 `Physics2D` 레이. 바라보는 방향·시야 각도·시야 거리 안 + 벽 차폐 없음 → `IsPlayerVisible = true`
- **지면/낭떠러지**: 발밑 앞쪽 레이, 미접촉 시 `GroundAhead = false`
- **벽**: 전방 레이 → `WallAhead`
- **소리**: 후보 1의 공통 R3 채널을 구독. 청취 반경 안이면 차폐 무관하게 `HeardSoundThisTick = true`
- **피격**: R3 이벤트 구독. 구독은 `AddTo`로 수명 관리한다

### 센싱 주기
- 레이 쿼리는 매 프레임이 아니라 **센싱 주기 간격**으로만 실행한다
- 갱신 사이에는 Blackboard 직전 값을 유지한다
- `StateMachine.Tick`은 매 프레임 그대로 실행되며, 센싱 주기와 분리된다
- 센싱 주기 값을 바꿔도 센서 외부(상태·조건) 코드는 수정되지 않아야 한다

### Blackboard 데이터 구분
- 순간 신호: `IsPlayerVisible`, `HeardSoundThisTick` — 갱신 시점에만 유효
- 보존 값: `LastKnownTargetPosition`, `TimeSincePerceived` (시야·소리 감지 시 0 리셋, 아니면 증가)
- 소비 측 판정(Detect 확정 / Chase 유지 / Patrol 복귀)은 순간 신호가 아니라 `TimeSincePerceived` 임계값 비교로 하도록 값을 제공한다
- 센서 → AI **한 방향**. 상태·조건은 읽기 전용

### 역할 분리
- 센서는 **View**가 소유 (Physics2D 레이 + R3 구독)
- `NpcActor`가 View의 센서 결과를 읽어 `EnemyBlackboard`를 채운 뒤 `_ai.Tick(dt)` 호출, 이동 결과를 적용한다 (프로젝트 A 틱 모델과 동일)
- 순수 계산(`TimeSincePerceived` 누적·리셋, 청취 반경 거리 판정)은 Unity API 비의존 로직으로 분리해 Play 없이 테스트 가능하게 한다

### 조절 가능 데이터 (ScriptableObject)
이번 범위에서 필요한 값: 시야 거리, 시야 각도, 청취 반경, 어그로 범위, 센싱 주기. Context가 입력받아 Blackboard·센서에 전달한다.

## Acceptance Criteria
- [ ] `EnemyBlackboard`에 `LastKnownTargetPosition`·`TimeSincePerceived`가 추가된다
- [ ] 순간 신호(`IsPlayerVisible`·`HeardSoundThisTick`)와 보존 값이 구분되어 있다
- [ ] 시야·지면/낭떠러지·벽이 `Physics2D` 레이로 판정되어 Blackboard에 반영된다
- [ ] 센서가 후보 1의 공통 소리 채널을 구독하고, 청취 반경 안이면 차폐 무관하게 `HeardSoundThisTick`이 켜진다
- [ ] 피격 R3 이벤트를 구독해 Blackboard에 반영한다
- [ ] 레이 쿼리 호출 횟수가 프레임 레이트가 아니라 센싱 주기에 비례한다
- [ ] 센싱 주기를 바꿔도 센서 외부 코드는 수정되지 않는다
- [ ] 센서는 View가 소유하고, `NpcActor`가 결과를 읽어 Blackboard를 채운다
- [ ] Blackboard는 센서에서만 쓰이며 외부에는 읽기 전용 계약으로 노출된다
- [ ] `TimeSincePerceived` 누적·리셋과 청취 반경 판정이 Play 모드 없이 순수 테스트로 검증된다
- [ ] `00_CommonFramework`의 기존 FSM 베이스·SO 저작 계층이 수정되지 않는다

## Open Questions
- `EnemyBlackboard` 확장을 공통 코어에서 필드 추가로 할지, ProjectB 전용 파생/컴포지션으로 할지 (원본 배치 규칙 확인 필요)
- 읽기 전용 계약을 별도 인터페이스로 노출할지, 프로퍼티 setter 접근 제한으로 충분히 볼지
- 센싱 주기를 적마다 위상 분산(스태거)할지 (다수 적 성능은 이번 범위 밖이지만 구조 결정에는 영향)
- 피격 R3 이벤트의 기존 발행 지점이 무엇인지 (신규 정의 필요 여부)
