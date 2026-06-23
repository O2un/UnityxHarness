## Overview
카메라 방향 기준으로 플레이어가 3D 공간을 이동하는 기능

## Problem / Goal
TopDown 뷰에서 입력 방향이 카메라 회전과 무관하게 작동해
플레이어가 의도한 방향으로 이동하지 못하는 문제 해결

## Functional Requirements
- **PlayerMover**: IInputReader.Move(Vector2)를 받아 카메라 forward/right 기준으로 world-space Vector3 velocity로 변환한다
- **PlayerMover**: 카메라 방향 기준은 Main Camera(CinemachineBrain)의 Transform — CinemachineCamera Transform이 아님
- **PlayerMover**: 변환된 velocity를 ReadOnlyReactiveProperty<Vector3>로 노출한다
- **PlayerView**: PlayerActor가 SetVelocity(Vector3)로 전달한 값을 CharacterController.Move()로 적용한다 (Rigidbody·Transform.position 직접 조작 금지)
- **PlayerActor**: PlayerMover.Velocity → PlayerView.SetVelocity 구독을 CompositeDisposable로 관리한다
- 이동 속도는 PlayerMover 외부에서 주입 가능해야 한다

## Constraints
- PlayerMover · PlayerActor: 순수 C# 클래스 (MonoBehaviour 없음), CompositeDisposable로 구독 관리
- PlayerMover 생성자에 카메라 Transform을 주입 — PlayerContext가 new할 때 Camera.main.transform을 넘긴다
- PlayerView: MonoBehaviour, CharacterController를 GetComponent로 참조
- IInputReader 인터페이스로 입력 소스를 분리한다

## Out of Scope
- 대시, 점프, 회전 애니메이션
