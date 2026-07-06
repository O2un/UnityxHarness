# EnemySpawner & WaveModule — PRD

## Overview
시간 기반 웨이브 스폰 시스템. `WaveModule`이 웨이브 데이터(어드레서블 Key·스폰 시각·개수·위치)를 들고 현재 시간에 맞춰 "지금 스폰할 몬스터 목록"을 계산하고, `EnemySpawnManager`가 EntryPoint Tick으로 그 목록을 받아 실제 스폰을 처리한다. 뱀서형 MVP의 적 공급 루프를 담당한다.

## Goals
- `WaveModule`(순수 C#)이 웨이브 데이터를 보유하고, 현재 시간을 받아 이번 프레임에 스폰할 몬스터 요청 목록을 반환한다.
- `EnemySpawnManager`가 초기화(`IAsyncStartable`) 시 `WaveModule`이 참조하는 **모든 어드레서블 프리팹을 프리로드**하고 풀에 등록한다.
- `EnemySpawnManager`가 EntryPoint로 등록되어 매 Tick 누적 타이머 값을 `WaveModule`에 전달하고, 반환된 목록대로 스폰한다.
- 스폰은 `IPoolService`를 통해 풀에서 획득(Get)하는 방식으로 처리한다.
- 이미 지나간 웨이브를 중복 스폰하지 않는다 (각 웨이브는 1회만 소비).

## Out of Scope
- 적 개별 AI(추격·공격)·체력·사망 처리 — 스폰만 담당.
- 스폰된 적의 풀 반납(사망 시 Release) 로직 — 소유 시스템에서 처리.
- 난이도 스케일링·무한 웨이브 생성 알고리즘.
- 스폰 위치 산출 로직(랜덤 링·화면 밖 등) 고도화 — 데이터로 주어진 위치를 사용.

## Technical Requirements
- **레이어**:
  - `WaveModule` — 순수 C# Module. Unity API 비의존, `new` 생성 가능. 시간→스폰 목록 계산만 담당.
  - `EnemySpawnManager` — Manager. VContainer EntryPoint(`ITickable` + `IAsyncStartable`)로 등록. `WaveModule` 소유, `IAssetService`·`IPoolService` 주입.
- **배치 위치**: `00_CommonFramework` (공통 인프라).
- **웨이브 데이터 주입 경로**: **ScriptableObject**. 웨이브 정의를 SO 에셋으로 작성하고, `EnemySpawnManager`가 이를 참조하여 `WaveModule`을 생성/초기화한다.
- **웨이브 데이터 단위**: `{ string addressableKey, float spawnTime, int count, Vector3 position }` (필드명은 구현 시 확정).
- **WaveModule API 예시**:
  ```csharp
  public sealed class WaveModule
  {
      public IReadOnlyList<string> RequiredKeys { get; }        // 프리로드 대상
      public IReadOnlyList<SpawnRequest> GetSpawnsAt(float time); // 이번 시각에 소비할 웨이브
  }
  ```
- **프리로드**: 초기화 시 `WaveModule.RequiredKeys`를 순회하며 `IAssetService.LoadAsync<GameObject>(key)`로 모두 로드하고, 로드된 프리팹을 `EnemySpawnManager`가 직접 `IPoolService.Register(key, prefab)`로 풀에 등록한다.
- **초기화 타이밍**: `EnemySpawnManager`가 `IAsyncStartable.StartAsync`에서 프리로드를 완료한 뒤, 완료 플래그를 세운다. Tick 스폰은 프리로드 완료 이후에만 동작한다.
- **시간 기준**: **게임 시작 후 자체 누적 타이머**. `Tick()`마다 `Time.deltaTime`을 누적한 값을 기준 시각으로 사용한다(씬 로드 기준 `Time.time`이 아닌 자체 timer로 일시정지 등에 대응 가능).
- **Tick 스폰**: `ITickable.Tick()`에서 프리로드 완료 시 누적 timer를 증가시키고 `WaveModule.GetSpawnsAt(timer)`를 호출, 반환된 각 `SpawnRequest`에 대해 `IPoolService.GetHandle<EnemyContext>(key).Get()`으로 인스턴스를 얻어 위치를 설정한다.
- **스폰 대상 타입**: `EnemyContext`. 모든 적은 공통 `EnemyContext` 컴포넌트를 가지며, 풀 핸들 타입은 `IPoolHandle<EnemyContext>`로 확정한다.
- **의존성**: `IAssetService`(프리로드), `IPoolService`/`IPoolHandle<EnemyContext>`(스폰). 두 인터페이스만 통해 접근, 구체 타입 직접 참조 금지.
- **등록**: `ProjectLifetimeScope.Configure`에서 `builder.RegisterEntryPoint<EnemySpawnManager>();` (필요 시 `.AsSelf()`).
- **파일 배치**:
  ```
  00_CommonFramework/Gameplay/EnemySpawner/
  ├── EnemySpawnManager.cs
  ├── WaveModule.cs
  ├── SpawnRequest.cs        (데이터 struct)
  └── WaveDataSO.cs          (ScriptableObject 웨이브 정의)
  ```
  (대분류 폴더명은 구현 직전 최종 확인)

## Acceptance Criteria
- [ ] `WaveModule`이 `new`로 생성되며 Unity API에 의존하지 않는다.
- [ ] 웨이브 데이터가 ScriptableObject로 정의되고 `WaveModule`에 주입된다.
- [ ] 초기화(`IAsyncStartable`) 시 `WaveModule`이 참조하는 모든 어드레서블 프리팹이 프리로드되고 `EnemySpawnManager`가 풀에 등록한다.
- [ ] `EnemySpawnManager`가 EntryPoint Tick으로 동작하며 자체 누적 타이머 값을 `WaveModule`에 전달한다.
- [ ] 지정된 `spawnTime`에 해당 웨이브의 `count`만큼 적이 지정 위치에 스폰된다.
- [ ] 각 웨이브는 정확히 1회만 스폰된다(중복·누락 없음).
- [ ] 스폰이 `IPoolService.GetHandle<EnemyContext>`를 통해 이뤄져 재사용된 인스턴스가 활용된다.
- [ ] 프리로드 완료 전에는 스폰이 발생하지 않는다.
- [ ] 어떤 코드도 `AssetManager`/`PoolManager` 구체 타입을 직접 참조하지 않는다.

## Resolved Decisions
1. **배치 위치** — `00_CommonFramework` (공통).
2. **웨이브 데이터 주입 경로** — ScriptableObject.
3. **시간 기준** — 게임 시작 후 자체 누적 타이머.
4. **프리로드 대기 방식** — `IAsyncStartable`로 독립 처리.
5. **스폰 대상 타입 `T`** — `EnemyContext`.
6. **풀 등록 주체** — 프리로드 시 `EnemySpawnManager`가 직접 `IPoolService.Register`.
