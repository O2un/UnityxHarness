# 스폰 시스템 공통 승격 (spawn-system-promotion)

> room-system PRD 분해 1/4. 후속 PRD(additive 씬 로딩, 룸 진행, 문·페이드 연출)가 공통 스폰 계약에 의존하므로 이 문서가 선행한다.

## Overview

프로젝트 A 전용으로 작성된 적 스폰·웨이브 시스템과 처치 알림 계약을 `00_CommonFramework`로 승격해 두 프로젝트가 공유한다. 승격 과정에서 공통 코어에 남아 있으면 안 되는 **3D 물리 API 의존을 제거**하고, 시간 기반으로 고정된 웨이브 트리거를 **처치 기반도 선택 가능하도록** 일반화한다. 프로젝트 B의 룸 시스템은 룸마다 "적을 세우고 전부 처치되면 클리어"를 요구하는데, 현재 스포너는 시간축 타임라인만 알고 3D `CharacterController`·`Physics`에 묶여 있어 그대로는 2D 플랫포머에서 쓸 수 없다.

이번 작업은 **동작 추가가 아니라 이동과 일반화**다. 프로젝트 A의 기존 스폰 동작은 회귀 없이 그대로 유지되어야 한다.

## Goals

- `IEnemyKillEvent`·`EnemyKilledInfo`·`EnemyKillEvent`를 `00_CommonFramework`로 승격한다
- 스폰·웨이브 시스템(`EnemySpawnManager`, `WaveModule`, `WaveDataSO`, `SpawnRequest`, 샘플러)을 `00_CommonFramework`로 승격한다
- 공통 스포너에서 `CharacterController`·`Physics.IgnoreLayerCollision` 등 3D 전용 API 의존을 제거하고 프로젝트 측 구현으로 내린다
- 웨이브 진행 트리거를 시간 기반/처치 기반 중 **데이터로 선택**할 수 있게 일반화한다
- 스포너 소비처가 구체 클래스가 아니라 인터페이스에 의존하도록 계약을 분리한다
- 승격 후 프로젝트 A의 스폰·클리어·웨이브 표시 동작이 이전과 동일하게 유지된다

## Out of Scope

- 룸 전환·additive 씬 로딩·문 상호작용·페이드 (후속 PRD)
- 프로젝트 B의 실제 웨이브 데이터 저작 및 룸별 스폰 배치 (계약과 구현만 준비하고 콘텐츠는 만들지 않음)
- 스폰 풀링 정책·오브젝트 재사용 전략 변경 (기존 `IPoolService` 흐름 그대로 사용)
- 적 AI·전투·드롭 로직 변경
- 자동화 테스트 — 테스트 어셈블리 도입 전까지 검증 안 함

## Technical Requirements

### 현재 상태

| 항목 | 현재 위치 | 비고 |
|---|---|---|
| `EnemySpawnManager` | `Assets/10_ProjectA/01_Scripts/Manager/EnemySpawner/` | `IAsyncStartable`·`ITickable`·`IDisposable` |
| `WaveModule`, `WaveDataSO`, `SpawnRequest` | 동일 폴더 | `WaveModule`은 이미 순수 C# |
| `AnnulusSampler`, `GaussianSampler` | 동일 폴더 | 이미 순수 C# |
| `IEnemyKillEvent`, `EnemyKillEvent`, `EnemyKilledInfo` | `Assets/10_ProjectA/01_Scripts/Actor/Item/` | |
| `IAssetService`, `IPoolService`, `IActorQuery`, `EnemyContext` | `00_CommonFramework` | **이미 공통** — 의존 방향 문제 없음 |

승격 대상이 의존하는 서비스가 전부 이미 공통이므로 이번 이동으로 새로 생기는 역방향 의존은 없다.

### 배치

- 스폰·웨이브 → `Assets/00_CommonFramework/00_Scripts/Manager/EnemySpawner/`
- 처치 알림 계약 → `Assets/00_CommonFramework/00_Scripts/Actor/Enemy/` (`EnemyContext`와 같은 폴더)
- 프로젝트 A 전용 3D 어댑터 → `Assets/10_ProjectA/01_Scripts/Manager/EnemySpawner/`

### 네임스페이스

승격 대상은 `O2un.Manager` / `O2un.Actors` 네임스페이스를 쓰고, 공통 코어도 **같은 네임스페이스를 이미 사용한다.** 따라서 파일 이동 시 네임스페이스를 바꾸지 않으며, 소비처의 `using` 수정도 발생하지 않는다. 이동은 `.cs`와 `.meta`를 함께 옮겨 GUID를 보존한다 (`WaveData_Test.asset` 등 기존 SO 참조가 끊기지 않아야 한다).

### 3D 의존 제거

현재 `EnemySpawnManager`에 있는 두 지점이 3D 전용이다.

```csharp
// StartAsync — 3D 물리 전용
Physics.IgnoreLayerCollision(enemyLayer, enemyLayer, true);

// Teleport — 3D CharacterController 전용
CharacterController controller = target.GetComponent<CharacterController>();
```

- **배치**: 스폰 위치 적용을 계약으로 분리한다. 공통 스포너는 계약만 호출하고 컴포넌트를 직접 다루지 않는다.

```csharp
public interface ISpawnPlacer
{
    void Place(EnemyContext enemy, Vector3 position);
}
```

- 프로젝트 A는 `CharacterController`를 끄고 켜는 기존 동작을 그대로 옮긴 구현을 등록한다. 프로젝트 B는 `Transform` 위치만 세팅하는 2D용 구현을 등록한다
- **레이어 충돌 무시**: `Physics.IgnoreLayerCollision` 호출을 공통 `StartAsync`에서 제거하고 프로젝트 A 측 초기화로 내린다. 2D는 `Physics2D` 계열이라 공통이 어느 쪽에도 묶이면 안 된다
- 승격 후 공통 스폰 코드에는 `CharacterController`·`Physics`·`Physics2D` 참조가 남아 있지 않아야 한다

### 웨이브 트리거 일반화

현재 `WaveModule`은 `GetSpawnsAt(float time)` 하나뿐이고, `EnemySpawnManager.Tick()`이 `_timer += Time.deltaTime`으로 타임라인을 밀어 시간 기반만 지원한다. 룸 시스템은 처치 기반이 필요하다.

- `WaveDataSO`에 트리거 모드를 데이터로 둔다 (예: `SpawnTriggerMode { Time, KillBased }`)
- `Time` 모드: 기존 타임라인 동작을 그대로 유지한다 (프로젝트 A 경로)
- `KillBased` 모드: 룸 진입 시 첫 웨이브를 배치하고, **그 웨이브의 적이 전부 처치되면** 다음 웨이브를 배치한다. 마지막 웨이브까지 처치되면 클리어
- 웨이브 진행 판단(현재 웨이브 잔존 수 → 다음 웨이브/클리어)은 `WaveModule` 쪽 **순수 C#**에 둔다. `MonoBehaviour` 상속·`Transform`/`GameObject` 조작·씬 탐색 없이 `new`로 생성 가능해야 한다
- `KillBased` 모드에서는 `Tick()`의 시간 누적 경로를 타지 않는다

### 소비처 계약 분리

현재 `GameSceneScope`가 `RegisterEntryPoint<EnemySpawnManager>().AsSelf()`로 등록하고 `GameManager`가 **구체 클래스** `EnemySpawnManager`를 생성자에서 받는다. 구체 클래스 직접 참조 금지 규칙 위반이며, 프로젝트 B도 같은 구체 타입에 묶이게 된다.

`GameManager`가 실제로 쓰는 표면은 `ReachedWave`, `TotalWaves`, `OnCleared`, `Begin()`, `Reset()` 다섯 개다. 이를 공통 인터페이스로 분리한다.

```csharp
public interface IEnemySpawner
{
    Observable<Unit> OnCleared { get; }
    int ReachedWave { get; }
    int TotalWaves { get; }
    void Begin();
    void Reset();
}
```

- `EnemySpawnManager`가 `IEnemySpawner`를 구현하고, 등록은 `.As<IEnemySpawner>()`를 포함한다
- `GameManager`는 `IEnemySpawner`만 본다
- 외부 알림은 C# `event` 금지, R3 `Subject`/`Observable`로 노출한다 (기존 `_onCleared` 방식 유지)

### DI 등록

- `WaveDataSO`는 `EnemySpawnManager` 한 곳만 소비하므로 전역 `RegisterInstance` 대신 등록의 `WithParameter(...)`로 넘긴다
- `ISpawnPlacer`는 프로젝트별 스코프에서 각자의 구현을 등록한다. **미등록 시를 대비해 위치만 세팅하는 기본 구현을 공통에 두고, 프로젝트가 등록하면 덮어쓴다** (등록 필수로 강제하지 않는다)
- `LifetimeScope` 밖에서 `Container.Resolve<>()` 직접 호출 금지

### 스폰 배치 · Tick

- **`KillBased` 모드는 고정 스폰 지점만 지원한다.** `SpawnPlacement.PlayerRadius`(플레이어 주변 원환 배치)는 `Time` 모드 전용으로 남기고, 룸 시스템은 룸 씬에 배치된 스폰 지점만 사용한다
- **`EnemySpawnManager`는 `ITickable`을 계속 구현하되, 트리거 모드가 `KillBased`면 `Tick` 등록을 하지 않는다.** `KillBased` 전용 프로젝트에서 매 프레임 빈 `Tick`이 도는 것을 막는다
- **적 프리팹은 프로젝트 B에서도 `EnemyContext`를 요구한다.** 공통 스포너의 `EnemyContext` 하드 요구를 유지하고, 프로젝트 B의 Monster 프리팹 쪽을 맞춘다 (승격 범위를 최소로 유지)

## Acceptance Criteria

- [ ] 스폰·웨이브·처치 알림 파일이 `00_CommonFramework` 아래로 이동했고 `10_ProjectA`에는 3D 어댑터만 남는다
- [ ] 공통 스폰 코드에 `CharacterController`·`Physics`·`Physics2D` 참조가 없다
- [ ] 컴파일 에러가 없고, 기존 `WaveData_Test.asset`의 스크립트 참조가 끊기지 않는다 (인스펙터에 Missing Script 없음)
- [ ] 프로젝트 A를 Play 모드로 실행하면 적이 이전과 같은 시점·위치에 스폰된다
- [ ] 프로젝트 A에서 적을 전부 처치하면 이전과 같이 클리어 신호가 한 번 발행된다
- [ ] 프로젝트 A의 웨이브 표시(`ReachedWave`/`TotalWaves`)가 이전과 동일하게 갱신된다
- [ ] 프로젝트 A에서 적끼리 서로 밀지 않는다 (레이어 충돌 무시가 프로젝트 측으로 이동한 뒤에도 유지)
- [ ] `GameManager`가 구체 `EnemySpawnManager`가 아니라 `IEnemySpawner`를 주입받는다
- [ ] 트리거 모드를 `KillBased`로 둔 데이터에서, 첫 웨이브를 전부 처치해야 다음 웨이브가 배치된다
- [ ] `KillBased` 모드에서 마지막 웨이브까지 처치하면 클리어 신호가 한 번 발행된다
- [ ] `Time` 모드 데이터의 동작이 승격 전과 동일하다
- [ ] Play 모드 진행 중 콘솔에 에러가 없다

## Open Questions

(없음 — 4건 모두 확정되어 Technical Requirements에 반영)
