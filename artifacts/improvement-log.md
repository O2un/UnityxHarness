# 개선 기록 (improvement-log)

## 2026-07-16 · ProjectB 2D Player 벽 마찰·페이싱 수정

### 구현

- `PlayerNoFriction.physicsMaterial2D`를 추가했다. friction/bounciness 모두 0이며, `Demo.unity` Unitychan의 BoxCollider2D에만 연결했다. 벽을 향해 이동해도 접촉 마찰이 낙하를 막지 않는다.
- `PlayerView`가 `LateUpdate`에서 실제 `Rigidbody2D.linearVelocity.x`를 읽어 SpriteRenderer `flipX`를 갱신한다. ±0.01 이내의 속도에서는 마지막 방향을 유지한다.
- 이동·점프 Module, 입력, DI 및 기존 카메라 변경은 건드리지 않았다.

### 검증 및 리뷰

- 정적 배선: PhysicsMaterial2D GUID와 Player BoxCollider2D 연결을 확인했다.
- Unity MCP 미연결로 Gate 1(컴파일), Gate 2(Play 콘솔), Gate 3(기능 자동검증)은 수동 확인 대기다.
- 코드 리뷰: blocker 0 / major 0 / minor 0. 초기 평가값-앞 규칙 위반 1건은 수정 완료했다.

### 다음 확인

- Unity에서 Demo.unity를 재생해 벽 입력을 유지해도 계속 낙하하는지, 좌우 실제 속도에 따라 스프라이트가 반전되고 정지 시 마지막 방향을 유지하는지 확인한다.
