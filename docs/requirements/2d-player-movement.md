# 2D Player Movement PRD

## Overview
2D 플랫폼 액션의 플레이어 이동 기능. 좌우 이동과 점프를 Rigidbody2D 물리로 처리하고, 그 위에 코요테 타임·점프 버퍼·관성 보정·가변 점프 높이를 얹어 조작감을 값으로 튜닝한다. 카메라 데드존·룩어헤드까지 이동 조작감의 일부로 포함하되, 기존 3D 탑다운용 CameraManager와는 별개로 프로젝트 B 전용 2D 카메라 계층을 새로 구성한다.

## Goals
- 좌우 이동과 점프를 Rigidbody2D 속도(velocity) 기반 물리로 구현한다.
- 이동 판단(속도 계산·코요테/버퍼 판정·가감속 램프·가변 점프)을 Unity API에 의존하지 않는 순수 `PlayerMover` Module에 둔다.
- Rigidbody2D 구동·캐스트 기반 접지 판정 등 Unity 표현을 `PlayerView`에 분리한다.
- 조작감 보정(코요테 타임·점프 버퍼·관성·가변 점프 높이)을 조절 가능한 값으로 노출한다.
- 프로젝트 B 전용 2D 카메라 계층에서 Cinemachine Position Composer의 Dead Zone·Lookahead를 적용한다.
- 튜닝 값을 코드 상수가 아닌 `ScriptableObject` 데이터로 두고 Context가 주입한다.

## Out of Scope
- 대시, 벽점프, 이중(2단) 점프 — 단, velocity 기반 구동으로 향후 확장 여지를 남긴다.
- 공격, 피격, 체력
- 경사·이동 발판 등 특수 지형 상호작용(기본 물리 충돌 처리 범위 밖의 로직)
- 기존 3D 탑다운용 `CameraManager`/`CameraRelativeMoveModule` 재사용 — 본 기능은 별도 2D 카메라 계층을 사용한다.

## Technical Requirements

### 입력 계약
- 기존 `IInputReader`(`O2un.Input`, `00_CommonFramework/.../Manager/Input`)를 그대로 사용한다. 새 입력 경로를 만들지 않는다.
  - 좌우 입력: `ReadOnlyReactiveProperty<Vector2> Move` 를 `PlayerMover`가 구독한다. 2D 좌우 이동에는 `Move.x`를 사용한다.
  - 점프 입력: `Observable<Unit> IsJumpPressed` 를 구독해 "눌림"을 점프 버퍼 타이머로 기록한다. FixedUpdate가 한 프레임에 0·1·N회 돌 수 있으므로, 기록해 둔 점프 의도를 다음 물리 스텝에서 정확히 한 번만 소비한다(점프 1회 보장).

### 이동 방식
- Transform 직접 조작 금지. `Rigidbody2D`에 velocity를 부여한다. 벽·발판·경사 충돌과 중력은 물리 엔진에 위임한다.
- 점프는 `AddForce(Impulse)`가 아니라 **velocity 직접 부여**(velocity.y 세팅) 방식으로 구현한다. 향후 커스텀 2단 점프·대시 확장을 위한 결정.

### 틱 분리
- 좌우 목표 속도·점프 판정은 `Update`(Tick) 시점에 계산하고, Rigidbody2D velocity 적용은 `FixedUpdate`에서 수행한다 — 프레임률과 무관한 일정 이동 보장.

### 접지 판정
- 캐스트(Cast) 기반 접지 판정을 사용한다(`Physics2D.BoxCast`/`CircleCast` 등). 순간 겹침(Overlap)이 아니라 캐스트를 쓰는 이유는 코요테 타임·점프 버퍼가 접지 상태의 연속적인 진입/이탈 타이밍에 의존하기 때문이다.
- 접지 판정은 Unity 표현이므로 `PlayerView`가 수행하고, 그 결과(접지 여부)를 `PlayerMover`에 값으로 전달한다.

### 보정 (모두 값으로 튜닝)
- **코요테 타임**: 발판 이탈 직후 짧은 유예 동안 점프 입력을 허용한다.
- **점프 버퍼**: 착지 직전 누른 점프를 기억했다가 착지 순간 발동한다(점프 계약 위에서 동작).
- **관성**: 가속·감속에 램프를 적용해 무게감을 부여한다(즉시 최고속/정지 금지).
- **가변 점프 높이**: 점프 키를 짧게/길게 누르는 것에 따라 점프 높이가 달라진다. 점프 키를 떼면 상승 중인 velocity.y를 감쇠(cut)하여 낮은 점프를 만든다.

### 카메라 보정 (프로젝트 B 전용 2D 계층)
- 기존 `CameraManager`(3D 탑다운·XZ 평면 basis)는 사용하지 않는다. 프로젝트 B 전용 2D 직교(Orthographic) 카메라 계층을 새로 구성한다.
- Cinemachine Position Composer의 Dead Zone·Lookahead를 사용한다. Dead Zone·Lookahead 크기는 `ScriptableObject` 값으로 노출한다.
- 팔로우 타깃 지정은 이 2D 카메라 계층이 담당한다.

### 역할 분리
- `PlayerMover`(순수 C# Module): 좌우 목표 속도·코요테 유예·점프 버퍼·가변 점프·관성 램프 판정을 소유. Unity API에 의존하지 않음.
- `PlayerView`(Unity): Rigidbody2D velocity 적용, 캐스트 기반 접지 감지.
- 2D 카메라 계층: 카메라 값 적용. 세 경계는 서로의 구현에 의존하지 않는다.

### 데이터
- 코요테 타임·점프 버퍼 시간, 가감속 램프, 최고 속도·점프 힘(초기 velocity.y), 가변 점프 감쇠 계수, 카메라 Dead Zone·Lookahead 크기를 `ScriptableObject`로 두고 Context가 입력받아 Mover·2D 카메라 계층에 전달한다.

### 배치
- 프로젝트 B 전용 로직이므로 `Assets/20_ProjectB`에 둔다. 공통 코어(`00_CommonFramework`)의 Actor/Context/View/Module 경계는 그대로 따른다. 기존 `00_CommonFramework`의 3D 이동/카메라 코드는 수정하지 않는다.

## Acceptance Criteria
- [ ] 좌우 입력(`IInputReader.Move.x`)으로 캐릭터가 Rigidbody2D velocity 기반으로 이동한다.
- [ ] 점프 입력(`IInputReader.IsJumpPressed`)으로 캐릭터가 점프하며, 한 입력당 정확히 한 번만 점프한다.
- [ ] 프레임률이 변동해도 이동 속도가 일정하다(Update 계산 / FixedUpdate 적용 분리).
- [ ] 접지 판정이 캐스트 기반으로 이루어진다.
- [ ] 발판 이탈 직후 코요테 타임 내 점프 입력이 받아들여진다.
- [ ] 착지 직전 누른 점프가 점프 버퍼 시간 내에 착지 순간 발동한다.
- [ ] 가속·감속에 관성 램프가 적용되어 즉시 최고속/정지가 아니다.
- [ ] 점프 키를 짧게 누르면 낮게, 길게 누르면 높게 점프한다(가변 점프 높이).
- [ ] 캐릭터가 데드존 안에서 움직일 때 카메라가 따라오지 않는다.
- [ ] 진행·낙하 방향으로 룩어헤드가 화면을 미리 민다.
- [ ] 모든 튜닝 값(보정 시간·램프·감쇠 계수, 속도·점프 힘, 카메라 값)이 `ScriptableObject`로 노출되어 코드 수정 없이 조정 가능하다.
- [ ] `PlayerMover`가 Unity API(MonoBehaviour/Transform/GameObject)에 의존하지 않는다.
- [ ] 코드가 `Assets/20_ProjectB`에 배치된다.

## Resolved Decisions
- **입력 시스템**: 기존 `IInputReader`(`Move: ReadOnlyReactiveProperty<Vector2>`, `IsJumpPressed: Observable<Unit>`) 그대로 사용.
- **접지 판정**: 캐스트(Cast) 방식 — 코요테 타임·점프 버퍼 대응.
- **점프 적용**: velocity 직접 부여 방식 — 향후 2단 점프·대시 확장 대비.
- **가변 점프 높이**: 포함(점프 키 릴리즈 시 상승 velocity 감쇠).
- **카메라**: 기존 3D `CameraManager`는 부적합. 프로젝트 B 전용 2D 카메라 계층(Cinemachine Position Composer Dead Zone·Lookahead)을 신규 구성.

## Open Questions
- 프로젝트 B 2D 카메라 계층의 구체 구성: 기존 `CameraManager` 패턴(순수 Manager + CinemachineCamera 주입)을 2D용으로 새로 만들 것인지, 아니면 더 단순한 형태로 둘 것인지 — 설계(unity-architect) 단계에서 확정.
- 가변 점프 감쇠 방식: 점프 키 릴리즈 시 velocity.y에 곱할 계수(cut multiplier)로 둘지, 상승 중 별도 중력 배율로 둘지.
