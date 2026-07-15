# 개선 기록 (improvement-log)

## 2026-07-15 · 2D Player Movement Part 1 (이동·점프 코어) 구현

### 구현 내용
2D 플랫포머 이동·점프 기본기. Rigidbody2D velocity 기반 좌우 이동 + velocity.y 직접 부여 점프, BoxCast 접지 판정, Update(계산)/FixedUpdate(적용) 틱 분리. 전부 `Assets/20_ProejctB/01_Scripts/`, 네임스페이스 `O2un.ProjectB.Platformer`(기존 3D `O2un.Actors`와 완전 분리 — 사용자 승인). CommonFramework 무변경.

- **PlayerMover** (순수 Module): `SetMoveInput`/`QueueJump`/`ResolveVelocity(bool grounded, float currentVy)`. 점프 플래그는 `grounded && queued`일 때만 소비 후 즉시 false → FixedUpdate N회에도 재점프 없음, 공중 유지 후 접지 순간 1회. MovementData 스칼라만 캐싱, Vector2 값 타입만 사용(Unity API 비의존).
- **PlayerView** (Mono, `[RequireComponent(Rigidbody2D)]`): `ApplyVelocity`→`linearVelocity`(Unity6), `VerticalVelocity`, `CheckGrounded(mask,size,dist)`=콜라이더 하단 중심 원점 BoxCast, `FollowTarget => transform`(Part3 카메라 훅).
- **MovementData** (SO): MaxMoveSpeed/JumpVelocity/GroundMask/GroundCastDistance/GroundCastSize. 캐스트 파라미터도 SO 노출(Part2 코요테/버퍼 접지 타이밍 튜닝 대비).
- **Player2DContext** (Mono, ISceneInitializable): `[Inject] IInputReader` + `[SerializeField]` MovementData/PlayerView. Update=SetMoveInput, FixedUpdate=CheckGrounded→ResolveVelocity→ApplyVelocity. `IsJumpPressed.Subscribe(_=>QueueJump()).AddTo`(R3 허용 범위).
- **ProjectBSceneScope** (신규 LifetimeScope): GameSceneScope 패턴 차용, `_sceneInitializables`+RegisterBuildCallback. IInputReader/MovementData는 등록 안 함(부모 스코프 해소 + 인스펙터 직접 할당).

### 결정 (사용자 승인)
- 배치: `20_ProejctB` 프로젝트 전용(PRD 명시). Actor 레이어 생략(기능 2개, Context가 Mover 직접 소유).
- 접지: BoxCast + 캐스트 파라미터 SO 노출.
- **씬 구조(§6-E)**: Demo.unity 안에 ProjectLifetimeScope + ProjectBSceneScope 함께 배치 → 단독 재생만으로 IInputReader 해소. (정식 부트 체인 경유 아님 — Part 1 검증 단순화 목적.)

### 검증
- Gate 1 컴파일: 통과(refresh scope=all 후 에러 0). 신규 .cs는 full refresh 필요(scripts-only는 미컴파일 — 반복되는 하네스 이슈).
- Gate 2 Play: 진입~3초 에러 0, DI가 Player2DContext에 IInputReader 주입 성공.
- Gate 3·4: 미완 — 사람이 Demo.unity 재생해 실조작 확인 필요.
- 리뷰: blocker 0/major 0/minor 2(둘 다 안전/오탐). 의존 방향·틱 분리·점프 1회·R3·금지패턴 통과.

### 다음 실행 규칙 (미완 항목)
- **Gate 3·4 수동 확인**: Demo.unity 재생 → 좌우 이동 / 점프 1입력 1회 / 공중 점프는 접지 순간 발동 / 착지 후 재점프 / 프레임률 무관 이동 / Ground 관통 없음. 체크리스트는 `artifacts/02-validation.md` part1 섹션.
- **M1(BoxCast 자기 히트)**: 현재 GroundMask=Ground(layer8)만이라 안전. Player 레이어가 마스크에 섞이면 항상 grounded=true 되므로 씬 재배선 시 주의.
- **Part 2 입력 준비**: PlayerMover의 점프/접지 계약 + IsGrounded 캐시가 코요테/버퍼 훅. MovementData가 코요테·버퍼·가변점프 값 확장 지점. PlayerView.FollowTarget이 Part 3 카메라 대상.
- **씬 구조 재검토**: Demo에 ProjectLifetimeScope 직접 배치는 검증용 단순화. 실제 게임 흐름(부트 체인)에 편입할 땐 이 배선 조정 필요.
