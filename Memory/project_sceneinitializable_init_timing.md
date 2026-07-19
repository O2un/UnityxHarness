---
name: project-sceneinitializable-init-timing
description: ISceneInitializable.Init()은 LifetimeScope.Awake 시점에 실행되므로 다른 컴포넌트의 OnEnable 결과물에 의존하면 안 된다
metadata: 
  node_type: memory
  type: project
  originSessionId: ae1dd09c-869e-40a0-a89c-27ff6ceccc65
  modified: 2026-07-19T09:25:09.471Z
---

`ProjectBSceneScope`의 `RegisterBuildCallback` → `InitializeSceneComponents`가 `ISceneInitializable.Init()`을 호출하는데, 이는 LifetimeScope의 Awake 단계다. 따라서 Init() 안에서는 다른 컴포넌트가 자기 `OnEnable`/`Awake`에서 만드는 런타임 객체가 아직 없을 수 있다.

실제 사례: `UIDocument.rootVisualElement`가 Init() 시점에 null이라 HUD 바인딩이 통째로 스킵됐다.

**Why:** Init()은 DI 주입 완료 시점을 보장할 뿐, Unity 컴포넌트 생명주기 완료를 보장하지 않는다.

**How to apply:** Init()에서는 주입 검증과 플래그 설정만 하고, 다른 컴포넌트의 런타임 산출물이 필요한 바인딩은 `Start()`로 미룬다. `PlayerHudView`가 이 패턴의 참고 구현이다.
