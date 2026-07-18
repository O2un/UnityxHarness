# platform-enemy-fsm

> 분해 순서 3/4 (platform-enemy-ai 분해) · Depends On: 2 enemy-blackboard-2d-sensor · Provides: 동작하는 AI (Attack은 스텁)

## Overview
2D 횡스크롤 플랫폼 적의 상태 머신 본체. 후보 2가 채운 `EnemyBlackboard`를 읽기 전용으로 소비해, 순찰(Patrol) → 감지(Detect) → 추격(Chase) → 공격(Attack)으로 흐르는 상태 4종과 전이 조건 5종을 정의하고 데이터로 조립한다. 전이를 켜는 것은 전적으로 센싱이며, 지면·낭떠러지·벽 감지는 상태 내부 행동에 쓰인다. `AttackState`는 이번 범위에서 **스텁**이며 실제 히트박스 실행은 다음 단계다.

## Goals
- 상태 4종(`PatrolState`·`DetectState`·`ChaseState`·`AttackState`)을 `EnemyStateSO`로 저작 가능하게 정의한다
- 전이 조건 5종(`PlayerVisibleCondition`·`HeardSoundCondition`·`WithinAttackRangeCondition`·`LostTargetCondition`·`TookDamageCondition`)을 `EnemyTransitionConditionSO`로 정의한다
- `PlatformEnemyAIProfile : EnemyAIProfileSO`가 상태·조건을 데이터로 조립해 `PlatformEnemyAI : BaseEnemyAI`를 생성하게 한다
- 전이 그래프를 `PlatformEnemyAI.ctor`의 `AddTransition`으로 조립한다
- 상태·조건·`StateMachine`을 Unity API 비의존 순수 로직으로 유지해 Play 없이 테스트 가능하게 한다
- 다음 단계가 붙일 `AttackState` 훅(공격 시작/종료 판정 지점)을 열어 둔다

## Out of Scope
- 소리 채널 정의(발행 인터페이스·페이로드 타입) — **후보 1(sound-signal-channel)** 범위
- `Physics2D` 레이 센서, 센싱 주기 스케줄러, 피격 R3 구독, Blackboard 필드 추가 — **후보 2(enemy-blackboard-2d-sensor)** 범위. 여기서는 채워진 Blackboard를 **읽기만** 한다
- 애니메이션 이벤트 히트박스 On/Off, 실제 데미지 적용, 공격 후 쿨타임 SO, 애니 종료 기반 Attack→Chase 복귀 판정 — **후보 4(enemy-melee-attack-execution)** 범위. `AttackState`는 이동 정지 + 훅만 있는 스텁으로 둔다
- 넉백·경직 반응
- 원거리 공격 적
- 다수 적 동시 실행 성능 최적화
- 공통 코어(`00_CommonFramework`)의 FSM 베이스·`EnemyBlackboard`·SO 저작 계층 재작성

## Technical Requirements

### 배치
- 2D 상태·센싱 조건·프로파일 등 신규 코드는 `Assets/20_ProjectB` 아래에 둔다
- `00_CommonFramework`의 `O2un.AI` FSM 베이스(`IState`·`ITransitionCondition`·`Transition`·`StateMachine`·`BaseEnemyAI`), SO 저작 계층, `EnemyBlackboard`, `ActorManager`는 **참조만** 한다
- 공통 베이스에 Player·특정 씬 등 구체 참조가 역류하지 않게 한다

### 상태 4종 (행동만 수행, 자기 전이 결정 금지)
- `PatrolState`: 바라보는 방향으로 순찰 이동. `GroundAhead == false` 또는 `WallAhead == true`면 방향 반전
- `DetectState`: 그 자리에서 플레이어 쪽을 바라보며 경계
- `ChaseState`: 플레이어(또는 `LastKnownTargetPosition`) 방향으로 추격. 낭떠러지 앞에서는 정지하되, 정지 상태로 추격 포기 타임아웃이 경과하면 Patrol로 복귀한다 (별도 즉시 복귀 규칙을 두지 않는다)
- `AttackState`(스텁): 이동 정지 후 공격 실행 훅 호출. 실제 히트박스·쿨타임은 후보 4에서 채운다
- 각 상태는 프로젝트 A `SeekPlayerState` 패턴대로 `EnemyBlackboard` + `CharacterMover`를 주입받는다

### 전이 조건 5종
- 모두 `EnemyTransitionConditionSO`로 저작 가능하다
- Blackboard를 **읽기만** 하고 쓰지 않는다
- Detect 확정 / Chase 유지 / Patrol 복귀 판정은 순간 신호(`IsPlayerVisible`·`HeardSoundThisTick`)가 아니라 `TimeSincePerceived`의 임계값 비교로 한다

### 전이 그래프
`PlatformEnemyAI.ctor`에서 `AddTransition`으로 조립:

| From | 조건 | To |
|---|---|---|
| Patrol | 시야 ∨ 소리 | Detect |
| Detect | 감지 확정 시간 내 감지 유지 | Chase |
| Detect | 감지 확정 실패 (경과 시간 초과) | Patrol |
| Chase | 공격 범위 안 | Attack |
| Attack | (스텁) 공격 종료 + 범위 밖 | Chase |
| Chase | 추격 포기 타임아웃 초과 (낭떠러지 정지 중 포함) | Patrol |
| Patrol / Detect / Attack | `TookDamageCondition` | Chase |

- 피격은 전역 전이가 **아니다**. 공통 `StateMachine`은 현재 상태 등록 전이만 평가하므로, 베이스를 수정하지 않고 각 상태에 개별 등록한다
- Attack → Chase 조건의 "공격 종료" 판정은 후보 4에서 애니메이션 종료 + 쿨타임으로 교체되며, 이번 범위에서는 교체 가능한 형태의 스텁으로 둔다

### 역할 분리
- 상태·조건·`StateMachine`은 Unity API 비의존 순수 클래스
- `NpcActor`가 Blackboard를 채운 뒤 `_ai.Tick(dt)` 호출, 이동 결과 적용 (후보 2에서 세운 틱 모델을 그대로 사용)

### 조절 가능 데이터 (ScriptableObject)
이번 범위에서 필요한 값: 공격 범위, 감지 확정 시간, 추격 포기 타임아웃, 순찰 속도, 추격 속도. Context가 입력받아 프로파일에 전달한다.

## Acceptance Criteria
- [ ] 상태 4종이 `EnemyStateSO`로 저작 가능하며, 각 상태는 전이를 결정하지 않고 행동만 수행한다
- [ ] 전이 조건 5종이 `EnemyTransitionConditionSO`로 저작 가능하다
- [ ] `PlatformEnemyAIProfile`이 상태·조건만으로 `PlatformEnemyAI`를 조립한다
- [ ] 상태·조건이 `EnemyBlackboard`에 쓰기를 하지 않는다
- [ ] Patrol이 `GroundAhead == false` 또는 `WallAhead == true`에서 방향을 반전한다
- [ ] 낭떠러지 앞에서 멈춘 Chase가 추격 포기 타임아웃 경과 시 Patrol로 복귀한다
- [ ] Detect 확정·Patrol 복귀 판정이 순간 신호가 아닌 `TimeSincePerceived` 임계값 비교로 이뤄진다
- [ ] Patrol·Detect·Attack 각각에서 피격 시 Chase로 전이한다 (전역 전이 없이 상태별 등록)
- [ ] Play 모드 없이 순수 테스트로 "타임아웃 경과 시 Patrol 복귀", "낭떠러지에서 이동 중단"을 검증할 수 있다
- [ ] `AttackState`가 후보 4의 히트박스·쿨타임을 붙일 수 있는 훅을 노출한다
- [ ] `00_CommonFramework`의 기존 FSM 베이스·SO 저작 계층이 수정되지 않는다

## Open Questions
- `AttackState` 스텁의 종료 판정을 임시로 무엇으로 둘지 (고정 시간 경과 vs 즉시 종료)
- 시야 조건과 소리 조건을 별도 조건 SO 2개로 두고 OR 전이를 2줄 등록할지, 합성 조건 SO를 둘지
- `PlatformEnemyAIProfile`이 조건 SO 인스턴스를 적별로 복제할지 공유할지 (상태를 갖는 조건이 있을 경우)
