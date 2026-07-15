# 2D Player Movement — Part 1: 이동·점프 코어 PRD

> 분해: `2d-player-movement.md` → Part 1/3. 선행 없음. 출력이 Part 2·Part 3의 입력이 된다.

## Overview
2D 플랫폼 액션의 이동·점프 기본기. Rigidbody2D velocity 기반 좌우 이동과 점프를 구현하고, 캐스트 기반 접지 판정과 Update/FixedUpdate 틱 분리로 프레임률과 무관한 일정 이동을 보장한다. 조작감 보정(코요테·버퍼·관성·가변 점프)과 카메라는 이 파트 범위 밖이며, Part 2·3이 이 코어의 계약 위에 얹는다.

## Goals
- 좌우 이동과 점프를 Rigidbody2D velocity 기반 물리로 구현한다.
- 이동 판단(좌우 목표 속도·점프 판정)을 Unity API에 의존하지 않는 순수 `PlayerMover` Module에 둔다.
- Rigidbody2D velocity 적용·캐스트 기반 접지 판정 등 Unity 표현을 `PlayerView`에 분리한다.
- 기본 튜닝 값(최고 속도·점프 초기 velocity)을 `ScriptableObject`로 두고 Context가 주입한다.

## Out of Scope
- 코요테 타임·점프 버퍼·관성 램프·가변 점프 높이 — Part 2.
- 2D 카메라 데드존·룩어헤드 — Part 3.
- 대시, 벽점프, 이중 점프, 공격/피격/체력, 특수 지형 상호작용.

## Technical Requirements

### 입력 계약
- 기존 `IInputReader`(`O2un.Input`, `00_CommonFramework/.../Manager/Input`)를 그대로 사용한다.
  - 좌우 입력: `ReadOnlyReactiveProperty<Vector2> Move` 를 구독, `Move.x`를 좌우 이동에 사용.
  - 점프 입력: `Observable<Unit> IsJumpPressed` 를 구독해 "눌림"을 기록. FixedUpdate가 한 프레임에 0·1·N회 돌 수 있으므로, 기록한 점프 의도를 다음 물리 스텝에서 정확히 한 번만 소비(점프 1회 보장).

### 이동 방식
- Transform 직접 조작 금지. `Rigidbody2D`에 velocity를 부여. 중력·충돌은 물리 엔진에 위임.
- 점프는 `AddForce(Impulse)`가 아니라 velocity.y 직접 부여 방식 — 향후 2단 점프·대시 확장 대비.

### 틱 분리
- 좌우 목표 속도·점프 판정은 `Update`(Tick)에서 계산, Rigidbody2D velocity 적용은 `FixedUpdate`에서 수행.

### 접지 판정
- 캐스트(Cast) 기반 접지 판정(`Physics2D.BoxCast`/`CircleCast` 등). 순간 겹침(Overlap)이 아니라 캐스트를 쓰는 이유는 Part 2의 코요테/버퍼가 접지 진입/이탈 타이밍에 의존하기 때문이다(코어 단계부터 접지 계약을 캐스트로 확정해 둔다).
- 접지 판정은 `PlayerView`가 수행하고, 결과(접지 여부)를 값으로 `PlayerMover`에 전달.

### 역할 분리
- `PlayerMover`(순수 C# Module): 좌우 목표 속도·점프 판정 소유. Unity API 비의존.
- `PlayerView`(Unity): Rigidbody2D velocity 적용, 캐스트 기반 접지 감지, 팔로우 대상이 될 Transform 노출.

### 데이터
- 최고 속도·점프 초기 velocity.y를 `ScriptableObject`(MovementData)로 두고 Context가 주입. Part 2가 이 SO를 확장한다.

### 배치
- `Assets/20_ProjectB`. `00_CommonFramework`의 Actor/Context/View/Module 경계를 따른다. 기존 3D 이동/카메라 코드는 건드리지 않는다.

## Acceptance Criteria
- [ ] 좌우 입력(`IInputReader.Move.x`)으로 캐릭터가 Rigidbody2D velocity 기반으로 이동한다.
- [ ] 점프 입력(`IInputReader.IsJumpPressed`)으로 캐릭터가 점프하며, 한 입력당 정확히 한 번만 점프한다.
- [ ] 프레임률이 변동해도 이동 속도가 일정하다(Update 계산 / FixedUpdate 적용 분리).
- [ ] 접지 판정이 캐스트 기반으로 이루어진다.
- [ ] 최고 속도·점프 힘이 `ScriptableObject`로 노출되어 코드 수정 없이 조정 가능하다.
- [ ] `PlayerMover`가 Unity API(MonoBehaviour/Transform/GameObject)에 의존하지 않는다.
- [ ] `PlayerView`가 팔로우 타깃으로 쓸 Transform을 노출한다(Part 3 입력).
- [ ] 코드가 `Assets/20_ProjectB`에 배치된다.

## Provides (다음 파트 입력)
- `PlayerMover` 점프/접지 판정 계약 → Part 2가 그 위에 보정을 얹음.
- 접지 여부 상태 및 점프 소비 지점 → Part 2 코요테/버퍼의 훅.
- `PlayerView`의 팔로우 타깃 Transform → Part 3 카메라 팔로우 대상.
- MovementData SO → Part 2가 필드 확장.

## Open Questions
- 접지 캐스트 형태(BoxCast vs CircleCast)와 캐스트 원점/거리 파라미터를 SO로 뺄지 코드 상수로 둘지 — 설계 단계 확정.
