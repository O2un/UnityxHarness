# hit-feedback-signal-and-hitstop

> 분해 1/2. 후속: [camera-shake-on-hit](camera-shake-on-hit.md)가 이 PRD의 `IHitFeedbackSignal`과 `HitFeedbackDataSO`에 의존한다.

## Overview
피격이 성립한 순간을 알리는 **공통 피격 신호**를 정의하고, 그 신호를 소비해 짧게 게임 시간을 멈추는 **히트스톱**을 구현한다. ProjectB(2D 플랫포머)의 전투에서 데미지가 숫자로만 적용되고 시각 피드백이 없어 맞았는지 체감되지 않는 문제의 절반(시간 정지)을 해결한다. 나머지 절반(카메라 셰이크)은 후속 PRD에서 이 신호를 구독해 붙인다.

## Goals
- 플레이어 피격과 적 피격을 하나의 신호 경로로 발행한다
- 신호를 소비해 히트스톱을 발생시킨다
- 플레이어 피격을 적 피격보다 강하게 주되, 강도 차이를 Inspector에서 조절할 수 있게 한다
- 히트스톱이 겹칠 때 시간이 무한정 멈추지 않도록 합성 규칙을 정한다
- 후속 셰이크 PRD가 코드 수정 없이 구독만으로 붙을 수 있는 신호 계약을 남긴다

## Out of Scope
- **카메라 셰이크 일체** — `camera-shake-on-hit` PRD 담당. 이 PRD에서는 Cinemachine을 건드리지 않는다
- **씬·프리팹 변경** — 이 PRD는 코드와 `.asset` 생성만으로 끝난다
- 히트 이펙트(파티클), 히트 사운드, 데미지 숫자 팝업
- 피격 시 스프라이트 플래시·넉백·무적 프레임
- ProjectA(3D)에의 연결 — 공통 인프라로 만들되 이번엔 ProjectB만 연결한다
- 사망 순간의 별도 강화 피드백 — 일반 피격과 동일하게 처리한다
- 무효타(빗나감) 분기 — 현재 코드에 해당 개념이 없다
- 별도 `TimeScaleManager` — ProjectB에 pause 기능이 없어 `timeScale` 소유권 경합이 없다
- 자동화 테스트 검증 (테스트 어셈블리 도입 전까지 검증 안 함)

## Technical Requirements

**배치 위치 (승인 완료)**
`Assets/00_CommonFramework/00_Scripts/Feedback/` 아래 `HitStop/` 중분류. 신호 계약(`IHitFeedbackSignal` 등)은 두 시스템이 공유하므로 `Feedback/` 직하에 둔다.

**신호 경로**
데미지 적용의 단일 관문은 `Assets/20_ProejctB/01_Scripts/Npc/Core/Damageable2DView.cs`의 `ApplyDamage(int)`이고, 플레이어(`Player2DContext`)와 적(`Npc2DContext`) 양쪽이 동일하게 이 컴포넌트를 쓴다. 여기서 신호를 1회 발행하면 두 케이스가 모두 커버된다.

- `IHitFeedbackSignal`: `Observable<HitFeedbackEvent>` 노출 (R3 `Subject` 기반, C# event 금지)
- `HitFeedbackEvent`: `ActorType Team`, `int Damage`, `Vector3 Position` readonly struct. `Position`은 이 PRD에서 쓰이지 않지만 후속 셰이크가 쓸 수 있도록 계약에 포함한다
- 발행 측은 `IHitFeedbackPublisher`로 분리해 구독자가 발행 권한을 갖지 않게 한다
- `Damageable2DView`가 발행자를 주입받아 `ApplyDamage`에서 발행. **미주입 시 기존 동작 유지(널 가드)** — 프리팹 재구성 없이 점진 적용 가능해야 한다

**히트스톱**
- `HitStopManager`: 전역 싱글턴 Manager, `ProjectBSceneScope`에 `RegisterEntryPoint`로 등록. `Time.timeScale`을 소유하고 매 프레임 `Time.unscaledDeltaTime`으로 잔여 시간을 감소시킨다
- `HitStopModule`: Unity API 비의존 순수 C#. 잔여 시간·합성 규칙과 timeScale 값 계산만 담당. Manager는 결과값을 `Time.timeScale`에 반영하기만 한다
- 합성 규칙: 겹치면 누적하지 않고 **더 긴 잔여 시간으로 덮어쓴다**(max)
- `timeScale`은 0이 아닌 소값(기본 0.05)으로 낮춘다. 완전 0은 Animator 이벤트·트리거 소실 위험이 있다
- 상한: 단일 히트스톱 최대 지속 0.2초로 클램프
- 해제 시 히트스톱 진입 직전에 저장해 둔 값으로 복원한다(1f 하드코딩 금지). 현재 코드베이스에 `timeScale` 조작 지점이 없음을 grep으로 확인했으나, 추후 pause가 추가돼도 깨지지 않도록 복원 방식을 유지한다

**파라미터 데이터**
- `HitFeedbackDataSO` (ScriptableObject): 팀(플레이어/적)별 프로필. 히트스톱 지속시간, timeScale 값, 데미지→강도 매핑 `AnimationCurve`를 갖는다. 후속 PRD가 여기에 셰이크 force 필드를 **추가**할 것이므로 프로필 구조를 확장 가능하게 잡는다
- 플레이어 피격 프로필 기본값을 적 피격보다 강하게 잡되(예: 지속 1.5배), 정확한 배율은 Inspector에서 튜닝한다. 코드에 배율을 상수로 박지 않는다
- 이 PRD 시점의 소비처는 `HitStopManager` 하나뿐이므로 전역 `RegisterInstance` 대신 해당 등록의 `WithParameter`로 넘긴다. 후속 PRD에서 소비처가 둘이 되면 그때 `RegisterInstance` 승격을 재검토한다

```
Damageable2DView.ApplyDamage
        └─> IHitFeedbackPublisher
                └─> IHitFeedbackSignal (Observable<HitFeedbackEvent>)
                        └─> HitStopManager -> HitStopModule -> Time.timeScale
                        └─> (후속) CameraShakeManager
```

## Acceptance Criteria
- [ ] 플레이어 근접 공격이 적에게 적중하면 화면이 순간 멈췄다가 즉시 복귀한다
- [ ] 플레이어 원거리 투사체(`Projectile2DView`) 적중에도 동일하게 히트스톱이 발생한다
- [ ] 적 근접 공격이 플레이어에게 적중하면 히트스톱이 발생하며, 적 피격보다 길게 멈춘다
- [ ] 강도 차이가 `HitFeedbackDataSO` 값만으로 조절되고 코드에 배율 상수가 없다
- [ ] 여러 적을 동시에 타격해도 히트스톱이 0.2초를 넘지 않는다
- [ ] 히트스톱 종료 후 `Time.timeScale`이 원래 값으로 복귀한다
- [ ] `HitFeedbackDataSO`에서 지속시간을 바꾸면 재컴파일 없이 반영된다
- [ ] 발행자를 주입받지 못한 `Damageable2DView`도 예외 없이 기존대로 동작한다
- [ ] Play 모드 진입부터 전투 1회까지 콘솔 에러·경고가 없다

## Open Questions
- 없음
