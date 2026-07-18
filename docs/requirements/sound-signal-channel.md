# sound-signal-channel

> 분해 순서 1/4 (platform-enemy-ai 분해) · Depends On: 없음 · Provides: 소리 채널 인터페이스 + 페이로드 타입

## Overview
플레이어 공격·착지 등 게임 내 "소리"를 발행하고 구독하는 공통 R3 채널. 소리는 적 AI 전용 신호가 아니라 플레이어·환경 등 여러 주체가 발행하는 범용 신호이므로 `00_CommonFramework`에 둔다. 이후 적 AI의 센서가 이 채널을 구독해 청취 판정에 사용한다.

## Goals
- 소리 발행/구독을 담당하는 R3 채널을 `00_CommonFramework`에 정의한다
- 소리 페이로드 타입(발생 위치·소리 세기 또는 종류 등 청취 판정에 필요한 최소 정보)을 정의한다
- 청취 반경 판정 규약을 정의한다: 청취 반경 안이면 **차폐 무관하게** 들린 것으로 본다
- 발행자(플레이어·환경)와 구독자(적 센서)가 서로를 모르는 단방향 신호 경로를 만든다
- 공통 베이스에 Player·특정 씬 등 구체 참조가 역류하지 않게 한다

## Out of Scope
- 적의 시야/지면/벽 `Physics2D` 레이 센싱, 센싱 주기 스케줄러, `EnemyBlackboard` 확장, 피격 R3 구독 — **후보 2(blackboard-2d-sensor)** 범위
- `PatrolState`·`DetectState`·`ChaseState`·`AttackState`, 전이 조건 5종, `EnemyStateSO`/`EnemyTransitionConditionSO` 저작, 전이 그래프, `PlatformEnemyAIProfile`·`PlatformEnemyAI` — **후보 3(platform-enemy-fsm)** 범위
- 애니메이션 이벤트 히트박스 On/Off, 공격 쿨타임 SO, Attack→Chase 복귀 판정 — **후보 4(enemy-melee-attack-execution)** 범위
- 실제 오디오 재생(AudioSource·믹서). 이 채널은 **게임플레이 신호**이지 사운드 출력이 아니다
- 소리 차폐(벽 뒤 감쇠) 계산
- 기존 `00_CommonFramework` FSM 베이스·SO 저작 계층·`EnemyBlackboard`·`ActorManager` 수정 (소리 채널 신규 추가만 허용)

## Technical Requirements

### 배치
- 신규 코드는 `Assets/00_CommonFramework` 아래에 둔다
- 기존 공통 코어 파일은 수정하지 않는다. 이번 작업은 **신규 추가**만이다

### 채널
- R3 `Subject`로 발행하고 `Observable`로만 외부에 노출한다 (C# `event` 금지)
- 발행 측 메서드와 구독 측 노출을 인터페이스로 추상화해, 구독자가 구체 클래스를 참조하지 않게 한다
- VContainer 싱글턴으로 등록·주입한다. `Container.Resolve<>()` 직접 호출 금지
- 구독은 `AddTo`로 수명 관리한다

### 페이로드
- 소리 발생 위치(2D 월드 좌표)를 포함한다
- 청취 판정에 쓰일 세기 또는 종류 구분을 포함한다 (공격·착지 등)
- 순수 데이터 타입이며 Unity 씬·오브젝트 참조를 갖지 않는다

### 청취 반경 판정 규약
- 판정은 **구독자(청취자)** 가 자신의 청취 반경으로 수행한다. 채널은 필터링하지 않고 그대로 흘린다
- 발생 위치와 청취자 위치의 거리가 청취 반경 이하이면 들린 것으로 본다
- 차폐(벽)는 고려하지 않는다
- 판정 자체는 Unity API에 비의존한 순수 계산으로 두어 Play 없이 테스트 가능하게 한다

## Acceptance Criteria
- [ ] 소리 채널이 `00_CommonFramework`에 있고, R3 `Subject`로 발행 / `Observable`로 구독한다
- [ ] 소리 페이로드 타입이 발생 위치와 세기(또는 종류)를 담는다
- [ ] 청취 반경 판정이 순수 로직으로 분리되어 Play 모드 없이 테스트 가능하다
- [ ] 청취 반경 안이면 차폐 여부와 무관하게 들린 것으로 판정된다
- [ ] 채널이 VContainer로 등록·주입되며 `Container.Resolve<>()`를 직접 호출하지 않는다
- [ ] 채널·페이로드가 Player나 특정 씬의 구체 타입을 참조하지 않는다
- [ ] 기존 `00_CommonFramework` FSM 베이스·SO 저작 계층·`EnemyBlackboard`가 수정되지 않는다

## Open Questions
- 페이로드의 소리 구분을 세기(float 반경/볼륨)로 둘지, 종류 enum(공격·착지)으로 둘지, 둘 다 둘지
- 채널을 소리 종류별로 분리할지 단일 채널로 둘지
- 청취 반경을 청취자 개별 SO 값으로만 둘지, 발행 측 세기와 조합할지
