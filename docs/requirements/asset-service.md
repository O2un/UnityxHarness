# AssetService — PRD

## Overview
Addressables의 `LoadAssetAsync<T>`와 로드 핸들 생명주기를 감싸는 에셋 로딩 Service. 게임 코드가 Addressables API·핸들 관리를 직접 다루지 않고, string 키만으로 에셋을 비동기 로드/해제하도록 추상화한다. `Resources.Load` 금지 규칙(CLAUDE.md §5)의 표준 대체 경로다.

## Goals
- `IAssetService` 인터페이스로 `LoadAsync<T>(string key)` / `Release(string key)`만 외부에 노출한다.
- 내부 `Dictionary<string, AsyncOperationHandle>`로 로드된 핸들을 캐싱한다.
- 동일 키의 로드가 진행 중일 때 재요청이 들어와도 재로드 없이 같은 로드를 공유한다.
- Addressables 로드를 UniTask로 결합해 `async/await`로 대기한다.
- VContainer `ProjectLifetimeScope`에 Singleton으로 등록하고 `IAssetService`로 노출한다.
- 로드 실패 시 해당 키를 캐시에서 제거한다.

## Out of Scope
- Addressables 카탈로그 업데이트·원격 번들 다운로드·다운로드 진행률 표시.
- 라벨 기반 일괄 로드(`LoadAssetsAsync`), `InstantiateAsync` 등 로드 외 API.
- 씬 전환 시 자동 일괄 해제·참조 카운팅 기반 GC 정책 (이번 단계는 키 단위 수동 Release).
- Addressable 그룹·주소 설정 등 에디터 측 에셋 등록 작업.

## Technical Requirements
- **레이어**: Manager 하위 배치. 파일/에셋 외부 시스템 접점이므로 `IAssetService` 인터페이스로 추상화.
- **네임스페이스**: `O2un.Manager` (기존 `ISceneService`·`IPoolService`와 동일 규칙).
- **외부 API** (string 키만 파라미터):
  ```csharp
  public interface IAssetService
  {
      UniTask<T> LoadAsync<T>(string key) where T : Object;
      void Release(string key);
  }
  ```
- **캐싱**: `Dictionary<string, AsyncOperationHandle>`. 키가 이미 존재하면 기존 핸들의 `.ToUniTask()`를 반환(로드 완료 여부 무관하게 재로드 금지).
- **비동기**: `Addressables.LoadAssetAsync<T>(key)` → `handle.ToUniTask()`로 대기.
- **실패 처리**: 로드 실패 시 에러 로그(`Debug.LogError`)를 출력하고, 캐시에서 해당 키 제거 후 예외를 전파한다.
- **해제**: `Release(key)` 시 캐시에서 핸들을 찾아 `Addressables.Release(handle)` 호출 후 딕셔너리에서 제거. 없는 키는 무시(no-op).
- **등록**: `ProjectLifetimeScope.Configure`에서 `builder.Register<AssetManager>(Lifetime.Singleton).AsImplementedInterfaces();`
- **배치 위치**: `00_CommonFramework` (공통 인프라).
  ```
  00_CommonFramework/00_Scripts/Manager/AssetManager/
  ├── AssetManager.cs
  └── IAssetService.cs
  ```

## Acceptance Criteria
- [ ] `IAssetService.LoadAsync<T>(key)`로 Addressable 에셋을 `await` 로드할 수 있다.
- [ ] 동일 키를 로드 완료 전에 두 번 요청해도 `LoadAssetAsync`는 한 번만 호출된다.
- [ ] 동일 키를 로드 완료 후 다시 요청하면 캐시된 결과를 즉시 반환한다.
- [ ] `Release(key)` 호출 시 핸들이 `Addressables.Release`되고 캐시에서 제거된다.
- [ ] 로드 실패 시 에러 로그가 출력되고, 해당 키가 캐시에 남지 않으며, 예외가 전파된다.
- [ ] VContainer에 Singleton으로 등록되어 `IAssetService`로 resolve된다.
- [ ] 어떤 코드도 `AssetManager` 구체 타입·`AsyncOperationHandle`을 직접 참조하지 않는다.

## Decisions (확정)
1. **로드 실패 시 반환 계약** — 예외를 throw한다.
2. **에러 처리** — 실패 시 `Debug.LogError`로 에러 로그를 출력한다.
3. **씬 전환 시 미해제 핸들 정책** — 이번 단계는 수동 Release만. 자동 정리는 후속 단계.
4. **배치 경로** — `Service/` 대분류를 신설하지 않고 `Manager/AssetManager/` 하위에 둔다. 네임스페이스는 `O2un.Manager`.
