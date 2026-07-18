# 룸 진행 루프 (room-progression)

> room-system PRD 분해 3/4. 선행 PRD 2건에 의존한다: `docs/requirements/spawn-system-promotion.md`(공통 `IEnemySpawner`·`KillBased` 웨이브·클리어 `Observable`)와 `docs/requirements/additive-scene-loading.md`(`IAdditiveSceneLoader`). 이 둘의 계약은 **그대로 소비하고 재정의하지 않는다.** 문 상호작용 View는 4/4 별도 PRD 범위다.

## Overview

한 스테이지를 이루는 룸 시퀀스를 순서대로 진행시키는 룸 진행 루프. 베이스 씬(플레이어·카메라·HUD·공통 시스템)은 유지한 채 룸 씬만 additive로 얹고 내리며, 룸 진입 → 스폰 → 클리어 → 다음 룸 전환 → 마지막 룸 클리어 시 스테이지 클리어 신호까지를 순수 C# `RoomModule`이 관장한다. 전환 순간은 페이드로 가리고, 룸 로드에 실패하면 게임 진행 불가로 보아 게임 선택 씬으로 복귀한다.

## Goals

- 룸 진행 로직을 Unity API 비의존 순수 `RoomModule`로 구축한다
- 룸 전환 순서를 페이드 아웃 → 언로드 → 로드 → 플레이어 배치 → 페이드 인 → 스폰으로 강제한다
- 전환 중 중복 전환 신호를 무시해 룸이 두 칸 넘어가지 않게 한다
- 룸 순서·씬 주소·웨이브 데이터·페이드 시간을 `RoomDataSO`/`StageDataSO`로 데이터화해 코드 수정 없이 스테이지를 엮는다
- 룸 씬마다 자체 `LifetimeScope`를 두고, 룸 쪽이 `IPlayerPlacer`로 베이스 씬 플레이어를 그 룸의 스폰 지점으로 이동시킨다
- 페이드 View를 `UniTask` 보간으로 구현해 로드·언로드 순간을 가린다
- 룸 로드 실패 시 에러 로그와 함께 `GameSelect` 씬으로 복귀한다
- 마지막 룸 클리어 시 스테이지 클리어 신호를 한 번 발행한다

## Out of Scope

- 문 상호작용 View·월드 스페이스 프롬프트·입력 처리 — 4/4 별도 PRD. 이 PRD는 `RoomModule`이 **전환 진입점(공개 메서드)** 만 노출한다
- `IAdditiveSceneLoader` 계약·구현·Addressables 핸들 관리 — 2/4에서 확정. 여기서는 소비만 한다
- 공통 스폰 시스템 승격·`KillBased` 웨이브 판정·`IEnemySpawner` 계약 — 1/4에서 확정. 여기서는 소비만 한다
- 절차적 지형 생성, 미니맵, 체크포인트, 분기 스테이지 진행 규칙
- 룸 간 플레이어 상태(체력·자원) 인계 규칙
- 스테이지 클리어 이후 결과 화면·다음 스테이지 진입 흐름
- 실제 룸 씬 에셋 저작 및 Addressables 그룹 구성 (계약과 데이터 구조만 준비)
- 자동화 테스트 — 테스트 어셈블리 도입 전까지 검증 안 함

## Technical Requirements

### 배치

- `RoomModule`, `RoomDataSO`, `StageDataSO`, 페이드 View, `IPlayerPlacer` 구현, 룸 씬 스코프 → `Assets/20_ProejctB/01_Scripts/Room/` (폴더명 `20_ProejctB`는 현재 리포지토리의 실제 철자)
- 공통 코어(`00_CommonFramework`)에 룸 이름·씬 주소 같은 프로젝트 B 구체 참조가 역류하지 않게 한다

### RoomModule (순수 C#)

- `MonoBehaviour` 상속·`Transform`/`GameObject` 조작·씬 탐색 없음. `new`로 생성 가능해야 한다
- 생성자 주입: `StageDataSO`, `IAdditiveSceneLoader`, `IEnemySpawner`, 페이드 계약, `ISceneService`
- 보유 상태: 현재 룸 인덱스, 룸 상태

```csharp
public enum RoomState
{
    Idle,
    Loading,
    Playing,
    Cleared,
    Transitioning,
}
```

- 외부 알림은 C# `event` 금지, R3 `Subject`/`Observable`로 노출한다 (예: `OnRoomEntered`, `OnRoomCleared`, `OnStageCleared`, `OnLoadFailed`)
- 공개 표면은 스테이지 시작 진입점과 **전환 진입점** 두 가지다. 전환 진입점은 목적지 식별자를 인자로 받아, 문이 여러 개인 룸에도 대응 가능한 형태로 둔다 (이번 범위에서는 룸당 문 1개 전제)

```csharp
public interface IRoomProgression
{
    Observable<int> OnRoomEntered { get; }
    Observable<Unit> OnRoomCleared { get; }
    Observable<Unit> OnStageCleared { get; }
    UniTask BeginStageAsync();
    void RequestTransition(string destinationId);
}
```

- `RequestTransition`은 상태가 `Cleared`가 아니면 무시한다. `Transitioning`/`Loading` 중 들어온 중복 신호도 무시한다 (예외를 던지지 않고 조용히 무시)
- 룸 클리어 판정은 `IEnemySpawner.OnCleared` 구독으로만 받는다. 잔존 수 계산을 `RoomModule`이 다시 하지 않는다
- 구독은 R3 `Subscribe` + 소유 `CompositeDisposable`로 관리하고 `Dispose()`에서 해제한다

### 전환 순서

아래 순서를 그대로 강제한다. 순서를 바꾸면 맵이 사라지거나 플레이어가 빈 공간에 노출된다.

1. 페이드 아웃 (`StageDataSO`의 페이드 아웃 시간만큼 대기)
2. 현재 룸 씬 언로드 (`IAdditiveSceneLoader.UnloadSceneAsync`)
3. 다음 룸 씬 로드 (`IAdditiveSceneLoader.LoadAdditiveSceneAsync(key, parentScope)`)
4. 플레이어 배치 (`IPlayerPlacer`)
5. 페이드 인
6. 웨이브 스폰 시작 (`IEnemySpawner.Begin()`)

- 첫 룸 진입도 같은 순서를 타되 2단계(언로드)는 건너뛴다
- 마지막 룸을 클리어하면 전환 대신 `OnStageCleared`를 **한 번만** 발행하고 상태를 종료 상태로 고정한다

### 룸 로드 실패 처리

- 룸 로드 실패(주소 누락·로드 예외)는 **게임 진행 불가**로 간주한다. 복구·재시도하지 않는다
- 2/4의 `IAdditiveSceneLoader`는 실패 시 예외를 전파하므로, `RoomModule`이 `try/catch`로 받아 정책을 수행한다
- 실패 시: 실패 사유를 에러 로그로 남기고 → 페이드를 유지(페이드 인하지 않음)한 채 → 기존 `ISceneService.LoadSceneAsync(SCENE_NAME.GAME_SELECT_SCENE)`로 `GameSelect` 씬(`Assets/90_Scenes/GameSelect.unity`)에 Single 복귀한다
- 실패를 삼키고 다음 룸으로 진행하지 않는다. `OnLoadFailed`를 발행한 뒤 상태를 종료 상태로 고정한다

### 룸 데이터

- `RoomDataSO`: 룸 additive 씬의 Addressables 주소(key) + 그 룸의 웨이브 데이터(`WaveDataSO`, 트리거 모드 `KillBased`)
- `StageDataSO`: `RoomDataSO` 시퀀스(순서 있는 리스트) + 페이드 아웃 시간 + 페이드 인 시간
- 룸 순서는 리스트 순서로만 결정한다. 코드에 룸 이름을 하드코딩하지 않는다
- DI 등록: `StageDataSO`의 소비처가 `RoomModule` 하나뿐이므로 전역 `RegisterInstance` 대신 `RoomModule` 등록의 `WithParameter(...)`로 넘긴다

### 페이드 View (MonoBehaviour)

- 전체 화면 오버레이(베이스 씬 소유)의 알파를 `UniTask`로 보간한다. 코루틴 금지
- 계약으로 추상화하고 `RoomModule`은 계약만 본다

```csharp
public interface IScreenFader
{
    UniTask FadeOutAsync(float duration);
    UniTask FadeInAsync(float duration);
}
```

- Single 로드용 로딩 화면(`Loading` 씬)과 별개다. 룸 전환은 로딩 씬을 경유하지 않는다
- 페이드 아웃 상태에서는 오버레이가 입력을 가리지 않아도 되지만, 전환 중 입력은 `RoomModule`의 상태 검사로 이미 무시되므로 별도 입력 차단을 넣지 않는다

### DI / 플레이어 배치

- `RoomModule`은 베이스 씬 스코프인 `ProjectBSceneScope`에 등록하고, 이 스코프가 additive 로드 시 부모 스코프로 넘어간다
- **룸 씬마다 자체 `LifetimeScope`를 둔다.** 스코프 없는 룸 씬은 허용하지 않는다
- 룸 씬 스코프는 그 룸의 스폰 지점을 담당하는 컴포넌트를 갖고, **룸 쪽이 베이스 씬 플레이어를 스폰 지점으로 이동시킨다.** 베이스 씬이 룸 안을 탐색하지 않으므로 씬 탐색 금지 규칙과 충돌하지 않는다
- 플레이어는 부모 스코프에서 계약으로 해소한다. 구체 클래스(`Player2DActor` 등) 직접 참조 금지

```csharp
public interface IPlayerPlacer
{
    void PlaceAt(Vector3 position);
}
```

- `IPlayerPlacer` 구현은 베이스 씬 측(플레이어 소유)에 두고 `ProjectBSceneScope`에 등록한다. 룸 씬 스코프는 이를 주입받아 자기 스폰 지점 좌표로 호출한다
- `LifetimeScope` 밖에서 `Container.Resolve<>()` 직접 호출 금지
- 룸 씬 스코프가 부모를 잡지 못하면(주입 실패) 로드 실패와 동일하게 취급한다

## Acceptance Criteria

- [ ] 룸을 넘어가도 베이스 씬의 플레이어·카메라·HUD가 재생성되지 않고 그대로 유지된다
- [ ] 다음 룸이 additive로 로드되고 이전 룸 씬은 언로드되어 씬 목록에 남지 않는다
- [ ] 룸 씬 안의 오브젝트가 베이스 씬 공통 시스템을 주입받아 동작한다 (주입 실패 예외 없음)
- [ ] 룸 진입 시 플레이어가 그 룸의 스폰 지점으로 이동한다
- [ ] 룸 진입 시 웨이브 스폰이 시작되고, 전부 처치하면 룸 클리어 신호가 한 번 발행된다
- [ ] 클리어 전에는 전환 진입점을 호출해도 룸이 넘어가지 않는다
- [ ] 전환 중 페이드가 로드·언로드 순간을 가려 맵이 사라지거나 튀어나오는 장면이 보이지 않는다
- [ ] 전환 진입점을 연속으로 여러 번 호출해도 룸이 한 칸만 넘어간다
- [ ] `StageDataSO`에서 룸을 추가·재배열하면 코드 수정 없이 스테이지 순서가 바뀐다
- [ ] 마지막 룸을 클리어하면 스테이지 클리어 신호가 한 번 발행되고, 추가 전환이 일어나지 않는다
- [ ] 룸 씬 주소를 잘못된 값으로 두면 에러 로그가 남고 페이드가 유지된 채 `GameSelect` 씬으로 복귀한다
- [ ] 로드 실패 후 다음 룸으로 진행하거나 스테이지 클리어 신호가 발행되지 않는다
- [ ] 기존 Single 로드(게임 선택 → 게임 씬) 흐름과 로딩 진행률이 이전과 동일하게 동작한다
- [ ] Play 모드 진행 중 콘솔에 에러가 없다

## Open Questions

- 페이드 오버레이를 베이스 씬 HUD 캔버스에 둘지 전용 캔버스로 분리할지 (렌더 순서상 HUD 위여야 함)
- 룸 클리어 후 문이 열리기 전까지의 대기 연출 유무 — 현재는 클리어 즉시 전환 가능 상태로 전제
- 로드 실패로 `GameSelect`에 복귀했을 때 사용자에게 알릴 UI가 필요한지 (현재는 에러 로그만)
