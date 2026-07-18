# enemy-melee-attack-execution

> 분해 순서 4/4 (platform-enemy-ai 분해) · Depends On: 3 platform-enemy-fsm 의 `AttackState` 훅 · Provides: 완성된 적 AI

## Overview
후보 3에서 스텁으로 남겨둔 `AttackState`에 실제 근접 공격 실행을 연결한다. 기존 근접 콤보(`melee-combo`)와 **동일한 방식**으로 애니메이션 이벤트에 맞춰 히트박스를 On/Off 하고, 애니메이션 종료 + 후속 쿨타임으로 공격 종료를 판정해 Chase로 복귀시킨다. 이 단계가 끝나면 적 AI가 순찰부터 실제 타격까지 완결된다.

## Goals
- `AttackState`가 이동을 정지하고 공격 애니메이션을 재생하게 한다
- 애니메이션 이벤트로 히트박스를 On/Off 한다 (기존 `melee-combo`와 동일한 방식)
- 애니메이션 종료 + 후속 쿨타임 경과를 공격 종료 판정으로 삼는다
- 공격 후 쿨타임 값을 조절 가능한 데이터(SO)로 둔다
- 후보 3의 스텁 종료 판정을 실제 판정으로 교체해, "쿨타임 경과 + 범위 밖 → Chase" 전이를 성립시킨다

## Out of Scope
- 소리 채널 정의(발행 인터페이스·페이로드 타입) — **후보 1(sound-signal-channel)** 범위
- `Physics2D` 레이 센서, 센싱 주기 스케줄러, 피격 R3 구독, Blackboard 필드 추가 — **후보 2(enemy-blackboard-2d-sensor)** 범위
- 상태 4종·전이 조건 5종·`EnemyStateSO`/`EnemyTransitionConditionSO` 저작·전이 그래프·`PlatformEnemyAIProfile`·`PlatformEnemyAI` 조립 — **후보 3(platform-enemy-fsm)** 범위. 여기서는 `AttackState` 훅에만 붙인다
- 다단 콤보(적은 단일 공격 1종)
- 넉백·경직 반응
- 원거리 공격 적
- 다수 적 동시 실행 성능 최적화
- 공통 코어(`00_CommonFramework`)의 FSM 베이스·`EnemyBlackboard`·SO 저작 계층 재작성

## Technical Requirements

### 배치
- 신규 코드는 `Assets/20_ProjectB` 아래에 둔다
- `00_CommonFramework`의 FSM 베이스·SO 저작 계층은 **참조만** 한다

### 공격 실행 방식
- 기존 근접 콤보(`melee-combo`)와 **동일한 방식**을 따른다: 애니메이션 타이밍에 맞춰 애니메이션 이벤트로 히트박스를 On/Off 한다
- 히트박스 On/Off와 애니메이션 재생은 View(MonoBehaviour) 책임이다. `AttackState`는 순수 클래스로 남아 View에 실행을 요청하고 결과 신호를 받는다
- View → 상태로 올라오는 공격 종료 신호는 R3 `Subject`/`Observable`로 노출한다 (C# `event` 금지)

### 종료 판정 및 복귀
- `AttackState` 종료 판정은 **애니메이션 종료 + 후속 쿨타임**이다. 애니메이션이 끝난 뒤 짧은 쿨타임이 경과해야 다음 공격 또는 Chase 복귀가 가능하다
- 쿨타임 경과 후 공격 범위 안이면 다음 공격, 범위 밖이면 Chase로 복귀한다
- 쿨타임 누적 계산은 Unity API 비의존 순수 로직으로 두어 Play 없이 테스트 가능하게 한다
- 후보 3의 Attack → Chase 전이 조건에서 스텁 종료 판정을 이 판정으로 교체한다. 전이 그래프 구조 자체는 바꾸지 않는다
- Attack 중 피격 시 Chase로 전이하는 후보 3의 규칙은 유지된다

### 조절 가능 데이터 (ScriptableObject)
이번 범위에서 필요한 값: 공격 후 쿨타임. (공격 범위는 후보 3에서 정의된 값을 사용한다)

## Acceptance Criteria
- [ ] `AttackState` 진입 시 이동이 정지하고 공격 애니메이션이 재생된다
- [ ] 애니메이션 이벤트로 히트박스가 On/Off 되며, 방식이 기존 `melee-combo`와 동일하다
- [ ] 애니메이션 종료 후 쿨타임이 지나야 다음 공격 또는 Chase 복귀가 가능하다
- [ ] 쿨타임 값이 SO로 조절 가능하며, 값을 바꿔도 상태·조건 코드는 수정되지 않는다
- [ ] 쿨타임 경과 시 범위 안이면 재공격, 범위 밖이면 Chase로 복귀한다
- [ ] Attack 중 피격 시 Chase로 전이한다
- [ ] 쿨타임 판정이 Play 모드 없이 순수 테스트로 검증된다
- [ ] `AttackState`가 Unity API 비의존 순수 클래스로 남고, 히트박스·애니메이션 조작은 View에 있다
- [ ] `00_CommonFramework`의 기존 FSM 베이스·SO 저작 계층이 수정되지 않는다

## Open Questions
- 애니메이션 종료 신호를 애니메이션 이벤트로 받을지, Animator 상태 길이 기반으로 볼지
- 히트박스 컴포넌트를 `melee-combo`의 것을 그대로 재사용할지, 적용 프리팹만 분리할지
- 쿨타임 중 플레이어가 범위를 들락거릴 때 판정 시점을 쿨타임 종료 순간 1회로 볼지 지속 평가할지
