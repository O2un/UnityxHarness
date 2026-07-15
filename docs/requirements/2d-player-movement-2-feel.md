# 2D Player Movement — Part 2: 조작감 보정 PRD

> 분해: `2d-player-movement.md` → Part 2/3. 선행: Part 1(이동·점프 코어). Part 3(카메라)와는 독립 — Part 1 이후 병렬 가능.

## Overview
Part 1의 이동·점프 코어 위에 조작감 보정 4종(코요테 타임·점프 버퍼·관성 램프·가변 점프 높이)을 얹는다. 모든 보정 파라미터를 `ScriptableObject` 값으로 노출해 코드 수정 없이 튜닝 가능하게 한다. 새 이동 방식이나 카메라는 도입하지 않고, 오직 Part 1의 판정을 다듬는다.

## Goals
- 코요테 타임·점프 버퍼·관성·가변 점프 높이를 `PlayerMover`(Part 1 Module)의 판정에 통합한다.
- 4종 보정의 판단 로직을 Unity API에 의존하지 않는 순수 로직으로 유지한다.
- 모든 보정 값을 `ScriptableObject`(Part 1의 MovementData 확장)로 노출한다.

## Depends On (Part 1 출력)
- `PlayerMover` 점프/접지 판정 계약.
- 접지 여부 상태 및 점프 소비 지점.
- MovementData SO(확장 대상).

## Out of Scope
- 이동·점프 기본 구동(Part 1), 카메라(Part 3).
- 대시, 벽점프, 이중 점프.

## Technical Requirements

### 보정 (모두 값으로 튜닝, 순수 로직)
- **코요테 타임**: 발판 이탈(접지 false 전이) 직후 짧은 유예 동안 점프 입력을 허용. 유예 시간은 SO 값.
- **점프 버퍼**: 착지 직전 누른 점프(`IsJumpPressed` 기록)를 버퍼 시간 동안 기억했다가 착지(접지 true 전이) 순간 발동. 버퍼 시간은 SO 값. Part 1의 "기록 후 한 번 소비" 규칙 위에서 동작.
- **관성**: 좌우 가속·감속에 램프를 적용해 즉시 최고속/정지가 아니게 함. 가속·감속 램프 값은 SO로 분리 노출.
- **가변 점프 높이**: 점프 키를 짧게/길게 누르는 것에 따라 높이가 달라짐. 점프 키 릴리즈 시 상승 중인 velocity.y를 감쇠(cut)하여 낮은 점프를 만든다. 감쇠 계수는 SO 값.

### 입력
- 가변 점프에는 점프 키 릴리즈 시점이 필요. 기존 `IInputReader`가 릴리즈를 노출하지 않으면, `IInputReader` 확장 여부를 설계 단계에서 확정(Open Question).

### 역할 분리
- 4종 보정 판정은 `PlayerMover`(순수 Module)에 둔다. 접지 전이 감지에 필요한 값(접지 여부, 시간 델타)은 `PlayerView`가 값으로 전달.
- Unity 표현(velocity 실제 적용)은 Part 1의 `PlayerView`가 계속 담당.

### 데이터
- 코요테 타임, 점프 버퍼 시간, 가속 램프, 감속 램프, 가변 점프 감쇠 계수를 MovementData SO에 추가.

## Acceptance Criteria
- [ ] 발판 이탈 직후 코요테 타임 내 점프 입력이 받아들여진다.
- [ ] 착지 직전 누른 점프가 점프 버퍼 시간 내에 착지 순간 발동한다.
- [ ] 가속·감속에 관성 램프가 적용되어 즉시 최고속/정지가 아니다.
- [ ] 점프 키를 짧게 누르면 낮게, 길게 누르면 높게 점프한다(가변 점프 높이).
- [ ] 보정 4종 값(코요테·버퍼 시간, 가속·감속 램프, 감쇠 계수)이 `ScriptableObject`로 노출되어 코드 수정 없이 조정 가능하다.
- [ ] 보정 판정이 `PlayerMover`(Unity API 비의존)에 위치한다.
- [ ] 점프 1회 보장(Part 1) 계약이 버퍼/코요테 도입 후에도 유지된다.

## Open Questions
- 가변 점프 감쇠 방식: 릴리즈 시 velocity.y에 곱할 cut 계수로 둘지, 상승 중 별도 중력 배율로 둘지.
- `IInputReader`가 점프 키 릴리즈(또는 홀드 상태)를 노출하는지, 확장이 필요한지.
