# Additive 씬 로딩 인프라 (additive-scene-loading)

## Overview

Addressables로 씬을 additive 로드·언로드하고, 그 씬 안의 `LifetimeScope`가 호출자가 지정한 부모 스코프에 붙도록 하는 공통 인프라. 베이스 씬을 유지한 채 콘텐츠 씬만 얹고 내리는 모든 흐름(룸 전환 등)의 토대이며, 기존 Single 로드(로딩 씬 경유·진행률) 흐름과는 독립적으로 동작한다.

## Goals

- `IAdditiveSceneLoader` 계약을 정의하고 기존 `SceneManager`가 `ISceneService`와 함께 구현하게 한다
- Addressables 주소(key) 기반으로 씬을 `LoadSceneMode.Additive`로 로드한다
- 로드 직전 `LifetimeScope.EnqueueParent(parentScope)`로 부모 스코프를 큐잉해, additive 씬의 `LifetimeScope`가 베이스 스코프를 부모로 잡게 한다
- 언로드 시 Addressables 씬 핸들을 함께 해제해 핸들 누수를 없앤다
- 기존 Single 로드 흐름과 `SceneState` 상태 머신을 변경 없이 유지한다
- 소비처가 `IAdditiveSceneLoader`만 보도록 의존을 좁힌다

## Out of Scope

- 룸 진행 로직, 스폰, 문, 페이드 (각각 별도 PRD)
- 로드 실패 시의 상위 복구 정책(로비 복귀 등) — 이 계층은 실패를 예외/로그로 드러내고 정책 판단은 하지 않는다
- Single 로드 흐름 개선, 로딩 진행률 UI 변경
- additive 씬의 Addressables 그룹·빌드 설정 구성
- 여러 additive 씬 동시 관리를 위한 씬 레지스트리·풀링
- 자동화 테스트 — 테스트 어셈블리 도입 전까지 검증 안 함

## Technical Requirements

### 배치

- `Assets/00_CommonFramework/00_Scripts/Manager/SceneManager/`
- 계약은 별도 파일 `IAdditiveSceneLoader.cs`, 구현은 기존 `SceneManager.cs`에 추가한다
- 공통 코어에 특정 룸·씬 이름 같은 구체 참조가 역류하지 않게 한다

### 계약

```csharp
public interface IAdditiveSceneLoader
{
    UniTask<Scene> LoadAdditiveSceneAsync(string key, LifetimeScope parentScope);
    UniTask UnloadSceneAsync(Scene scene);
}
```

- `SceneManager`가 `ISceneService`, `ILoadingSource`에 더해 `IAdditiveSceneLoader`를 구현한다
- 등록은 기존 `ProjectLifetimeScope`의 `builder.Register<SceneManager>(Lifetime.Singleton).AsImplementedInterfaces()`를 그대로 쓴다. 별도 등록을 추가하지 않는다

### 로드

- `Addressables.LoadSceneAsync(key, LoadSceneMode.Additive)`를 사용한다. `UnityEngine.SceneManagement.SceneManager` 직접 호출은 이 경로에서 쓰지 않는다
- `parentScope`가 null이 아니면 `using (LifetimeScope.EnqueueParent(parentScope))` 블록 안에서 로드를 await한다. 이 큐잉 없이는 additive 씬의 `LifetimeScope`가 부모를 잡지 못한다
- 반환값은 로드된 `Scene`이다
- 로드 실패(주소 누락·예외)는 삼키지 않는다. 에러 로그를 남기고 예외를 호출자에게 전파한다
- 기존 `_currentState`(`SceneState`)와 `_loadingProgress`는 이 경로에서 건드리지 않는다. Single 로드가 진행 중이어도 additive 로드는 별개로 동작한다

### 언로드 & 핸들 관리

- 로드 시 Addressables 씬 핸들(`SceneInstance` 핸들)을 로드된 `Scene`을 키로 내부 딕셔너리에 보관한다
- `UnloadSceneAsync(scene)`는 보관된 핸들을 찾아 `Addressables.UnloadSceneAsync(handle)`로 해제하고, 딕셔너리에서 제거한다
- 핸들이 없는(이 로더가 로드하지 않은) 씬을 넘기면 경고 로그를 남기고 아무 것도 하지 않는다
- `Dispose()` 시 남아 있는 핸들 보관 상태를 정리한다

### 설계 제약

- Unity API 의존이 불가피한 계층이므로 별도 순수 Module로 쪼개지 않는다. 대신 상태(핸들 맵)와 로드 절차 외의 정책 판단(재시도·대체 씬)은 이 클래스에 넣지 않는다
- 외부 알림이 필요해지면 C# event 금지, R3 `Subject`/`Observable`로 노출한다

## Acceptance Criteria

- [ ] `IAdditiveSceneLoader`를 주입받은 소비처가 주소를 넘기면 해당 씬이 additive로 로드되어 씬 목록에 추가된다
- [ ] 로드된 씬 안의 `LifetimeScope`가 넘긴 부모 스코프의 등록을 해소한다 (주입 실패 예외 없음)
- [ ] 로드 후에도 베이스 씬의 오브젝트가 재생성되거나 사라지지 않는다
- [ ] `UnloadSceneAsync` 호출 후 해당 씬이 씬 목록에서 사라진다
- [ ] 같은 씬을 여러 번 로드·언로드해도 Addressables 핸들 관련 경고·에러가 누적되지 않는다
- [ ] 잘못된 주소를 넘기면 에러 로그가 남고 예외가 호출자에게 전파된다 (조용히 성공 처리되지 않는다)
- [ ] additive 로드·언로드를 수행해도 기존 Single 로드(게임 선택 → 게임 씬) 흐름과 로딩 진행률이 이전과 동일하게 동작한다
- [ ] Play 모드 진행 중 콘솔에 에러가 없다

## Open Questions

- 동일 주소를 중복 로드하는 호출을 허용할지, 첫 인스턴스를 재사용할지 — 현재는 호출마다 새로 로드하는 것을 전제
- Addressables 그룹 구성과 씬 주소 명명 규칙을 누가 정의할지 (씬 에셋 준비 시점)
