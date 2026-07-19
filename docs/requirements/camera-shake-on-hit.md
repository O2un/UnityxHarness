# camera-shake-on-hit

> 분해 2/2. 선행: [hit-feedback-signal-and-hitstop](hit-feedback-signal-and-hitstop.md)이 제공하는 `IHitFeedbackSignal`과 `HitFeedbackDataSO`가 있어야 착수 가능하다.

## Overview
선행 PRD가 만든 피격 신호를 구독해, 적중 순간 카메라를 흔든다. 히트스톱(시간 정지)에 충격의 방향감·강도를 더해 타격 피드백을 완성한다.

## Goals
- 피격 신호를 구독해 카메라를 흔든다
- 플레이어 피격을 적 피격보다 크게 흔들되, 세기를 Inspector에서 조절할 수 있게 한다
- 히트스톱으로 `timeScale`이 낮아진 동안에도 흔들림이 정상 속도로 재생되게 한다
- Manager가 Cinemachine 타입에 직접 의존하지 않는 추상 경계를 둔다

## Out of Scope
- 히트스톱 로직 — 선행 PRD 담당. 이 PRD에서 `Time.timeScale`을 건드리지 않는다
- 히트 이펙트·사운드·데미지 숫자
- 게임패드 진동
- 피격 위치 기반 감쇠 — 씬 공유 ImpulseSource 1개를 쓰기로 확정했다
- ProjectA(3D)에의 연결
- 자동화 테스트 검증

## Technical Requirements

**배치 위치**
`Assets/00_CommonFramework/00_Scripts/Feedback/CameraShake/`.

**셰이크**
- Cinemachine 3.1.7이 이미 프로젝트에 있고 `2D_GameScene.unity`가 `CinemachineCamera`를 사용하므로 Cinemachine Impulse로 구현한다
- `CameraShakeManager`: `IHitFeedbackSignal`을 구독해 `HitFeedbackDataSO`의 팀별 프로필에서 force를 계산하고 `IImpulseEmitter.Emit(force)`를 호출
- `IImpulseEmitter`: Impulse 접점 추상화. 구현체 `CinemachineImpulseEmitter`(MonoBehaviour)가 `CinemachineImpulseSource.GenerateImpulseWithForce`를 호출한다. Manager는 Cinemachine 타입을 참조하지 않는다
- 흔들림은 `timeScale` 영향을 받지 않아야 하므로 Impulse를 unscaled 시간 기준으로 동작하도록 설정한다

**데이터 확장**
- 선행 PRD의 `HitFeedbackDataSO` 팀별 프로필에 셰이크 force 필드를 **추가**한다(새 SO를 만들지 않는다)
- 소비처가 `HitStopManager`/`CameraShakeManager` 둘이 되므로, 선행 PRD의 `WithParameter` 주입을 `RegisterInstance` 승격으로 바꿀지 이 시점에 판단한다

**씬 작업 (승인 게이트 B 대상)**
- 게임플레이 `CinemachineCamera`에 `CinemachineImpulseListener` 추가
- 씬 전역 공유 `CinemachineImpulseSource` + `CinemachineImpulseEmitter` 오브젝트 1개 배치, `ProjectBSceneScope`에 `IImpulseEmitter`로 등록
- 변경 전 커밋 권고

```
IHitFeedbackSignal ──> CameraShakeManager ──> IImpulseEmitter ──> CinemachineImpulseSource
                                                                          └─> ImpulseListener (CinemachineCamera)
```

## Acceptance Criteria
- [ ] 플레이어 근접 공격이 적에게 적중하면 카메라가 흔들린다
- [ ] 플레이어 원거리 투사체 적중에도 동일하게 흔들린다
- [ ] 적 공격이 플레이어에게 적중하면 적 피격보다 크게 흔들린다
- [ ] 흔들림이 0.5초 이내에 원위치로 수렴한다
- [ ] 히트스톱으로 시간이 느려진 동안에도 흔들림이 정상 속도로 재생된다
- [ ] 세기 차이가 `HitFeedbackDataSO` 값만으로 조절된다
- [ ] `CameraShakeManager`가 Cinemachine 타입을 직접 참조하지 않는다
- [ ] Play 모드 진입부터 전투 1회까지 콘솔 에러·경고가 없다

## Open Questions
- 없음
