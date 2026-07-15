# 2D Player Movement — Part 3: 프로젝트 B 2D 카메라 계층 PRD

> 분해: `2d-player-movement.md` → Part 3/3. 선행: Part 1(팔로우 타깃 Transform). Part 2(조작감)와는 독립 — Part 1 이후 병렬 가능.

## Overview
프로젝트 B 전용 2D 직교(Orthographic) 카메라 계층을 신규 구성한다. 기존 `CameraManager`는 프로젝트 A용 3D 탑다운(XZ 평면 basis) 구현이므로 재사용하지 않고, 2D용으로 완전히 새로 만든다. Cinemachine Position Composer의 Dead Zone·Lookahead로 이동 조작감을 완성하고, 관련 값을 `ScriptableObject`로 노출한다.

## Goals
- 프로젝트 B 전용 2D 직교 카메라 계층을 새로 구성한다.
- Part 1 `PlayerView`의 Transform을 팔로우 타깃으로 지정한다.
- Cinemachine Position Composer의 Dead Zone·Lookahead를 적용한다.
- Dead Zone·Lookahead 크기를 `ScriptableObject`(CameraData) 값으로 노출한다.

## Depends On (Part 1 출력)
- `PlayerView`가 노출하는 팔로우 타깃 Transform.

## Out of Scope
- 이동·점프(Part 1), 조작감 보정(Part 2).
- 기존 3D `CameraManager`/`CameraRelativeMoveModule` 재사용·수정 — 본 파트는 별도 2D 계층.
- 카메라 흔들림, 줌, 컷신 전환 등 부가 카메라 연출.

## Technical Requirements

### 카메라 구성
- 2D 직교(Orthographic) 카메라 + Cinemachine 카메라 구성. 프로젝트 A용 3D `CameraManager`와 별개.
- Position Composer의 Dead Zone·Lookahead 사용:
  - **Dead Zone**: 캐릭터가 데드존 안에서 움직일 때 카메라가 따라오지 않음.
  - **Lookahead**: 진행·낙하 방향으로 화면을 미리 밈.

### 역할 분리
- 2D 카메라 계층은 팔로우 타깃 지정과 카메라 값 적용만 담당. `PlayerMover`/`PlayerView` 구현에 의존하지 않고 Transform만 받는다.
- 카메라 값 적용 경계는 이동 계층과 서로의 구현에 의존하지 않는다.

### 데이터
- Dead Zone 크기, Lookahead 크기를 `ScriptableObject`(CameraData)로 두고 Context가 주입해 2D 카메라 계층에 전달.

### 배치
- `Assets/20_ProjectB`. `00_CommonFramework` 경계를 따르되 기존 3D 카메라 코드는 수정하지 않는다.

## Acceptance Criteria
- [ ] 프로젝트 B 전용 2D 직교 카메라 계층이 신규로 구성된다(기존 3D CameraManager 미사용).
- [ ] Part 1 `PlayerView`의 Transform이 팔로우 타깃으로 지정된다.
- [ ] 캐릭터가 데드존 안에서 움직일 때 카메라가 따라오지 않는다.
- [ ] 진행·낙하 방향으로 룩어헤드가 화면을 미리 민다.
- [ ] Dead Zone·Lookahead 크기가 `ScriptableObject`로 노출되어 코드 수정 없이 조정 가능하다.
- [ ] 코드가 `Assets/20_ProjectB`에 배치된다.

## Open Questions
- 2D 카메라 계층 형태: 기존 `CameraManager` 패턴(순수 Manager + CinemachineCamera 주입)을 2D용으로 새로 만들지, 더 단순한 형태로 둘지 — 설계 단계 확정.
- CameraData SO를 이동 계층 SO와 별도 파일로 둘지 통합할지.
