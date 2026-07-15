# 개선 기록 (improvement-log)

## 2026-07-16 · ActorManager 중앙 프레임 루프

### 구현

- `IActorTickable`과 `IActorFixedTickable`을 순수 기능 계약으로 추가했다. 두 인터페이스는 `IActor`를 상속하지 않는다.
- ProjectA/B `ActorManager`가 동일하게 VContainer `ITickable`/`IFixedTickable` 엔트리포인트로 실행되며, 등록된 활성 Actor 중 기능 인터페이스를 구현한 대상만 호출한다.
- `PlayerActor`, `NpcActor`, `Player2DActor`가 필요한 Actor Tick 인터페이스를 구현한다.
- Player/NPC/Player2D Context의 Actor 직접 `Update`/`FixedUpdate` 호출을 제거했다.
- NPC 공격 SkillModule도 NpcActor가 소유해 중앙 Actor Tick에서 함께 실행·해제한다.

### 검증 및 리뷰

- `git diff --check`와 구조 검증은 통과했다.
- Unity MCP 미연결로 Gate 1 컴파일, Gate 2 Play 콘솔, Gate 3 기능 자동검증은 수동 확인 대기다.
- 최종 코드 리뷰: blocker 0 / major 0 / minor 0.
- 씬·프리팹·에셋 변경은 없다.

### 다음 확인

- Unity Refresh 후 컴파일 오류가 없는지 확인한다.
- ProjectB Play에서 이동·점프가 정상이고 View 비활성화 또는 Context 파괴 후 Actor 호출이 멈추는지 확인한다.
- ProjectA Play에서 기존 Player/NPC 루프가 한 번만 실행되는지 확인한다.
