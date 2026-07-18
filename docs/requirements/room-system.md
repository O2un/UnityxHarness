# 룸 전환 & 스테이지 구조 (room-system)

> **이 문서는 상위 개요다. 구현은 아래 4개 PRD로 분해되어 있으며, 착수 시에는 각 PRD를 참조한다.**
>
> 1. [spawn-system-promotion.md](spawn-system-promotion.md) — 스폰 시스템 공통 승격
> 2. [additive-scene-loading.md](additive-scene-loading.md) — Additive 씬 로딩 인프라
> 3. [room-progression.md](room-progression.md) — 룸 진행 루프 (1·2에 의존)
> 4. [room-door-interaction.md](room-door-interaction.md) — 문 상호작용 (3에 의존)
>
> 1과 2는 서로 의존하지 않아 병렬 진행 가능하다.

## Overview

2D 플랫폼 스테이지를 여러 룸으로 나눠 잇는 룸 전환 시스템. 플레이어·카메라·HUD·공통 시스템이 담긴 **베이스 씬은 로드된 채 유지**하고, 방마다 바뀌는 맵 지오메트리와 스폰 배치만 **additive 룸 씬**으로 얹고 내린다. 한 룸의 사이클은 `스폰 → 웨이브 반복 → 클리어 판정 → 문 열림 → 문 상호작용 → 화면 전환 → 다음 룸`이며, 스테이지는 이 룸들의 시퀀스다.

## Goals

- 베이스 씬을 재로드하지 않고 룸 씬만 additive로 교체하는 룸 전환 흐름을 구축한다
- 룸 진행 로직을 Unity API에 의존하지 않는 순수 `RoomModule`로 분리한다
- 룸 순서·씬 주소·스폰 정보·페이드 시간을 데이터(ScriptableObject)로 두어 코드 수정 없이 스테이지를 엮는다
- additive 룸 씬 안의 오브젝트가 베이스 씬의 공통 시스템을 주입받도록 부모 스코프 수동 주입 경로를 만든다
- 스폰·처치 알림 등 핵심 시스템을 `00_CommonFramework`로 승격해 두 프로젝트가 공유한다
- 룸의 모든 웨이브를 처치하면 클리어로 판정하고 다음 룸으로 가는 문을 연다
- 문 범위 진입 시 문 View의 월드 스페이스 프롬프트를 띄우고 공격 키 입력으로 전환을 시작한다
- 페이드 아웃/인으로 룸 로드·언로드 순간을 가린다
- 마지막 룸 클리어 시 스테이지 클리어 신호를 발행한다

## Out of Scope

- 절차적 지형 생성 (룸 소스 추상만 만들고 구현은 하지 않음)
- 룸 내부 세이브/체크포인트, 미니맵, 분기 스테이지 진행 규칙 (문 계약만 다중 문을 허용하고, 분기 선택 로직은 만들지 않음)
- 룸 간 플레이어 상태(체력·자원) 인계 규칙의 세부 설계
- 스테이지 클리어 이후 결과 화면·다음 스테이지 진입 흐름
- 자동화 테스트 — 테스트 어셈블리 도입 전까지 검증 안 함

## Technical Requirements

### 배치

- additive 로드·언로드·부모 스코프 주입 → `Assets/00_CommonFramework/00_Scripts/Manager/SceneManager/`
- 스폰 시스템과 처치 알림 계약은 공통으로 승격 → `00_CommonFramework` 하위 해당 기능 폴더
- `RoomModule`, 룸/스테이지 데이터 SO, 문 View, 페이드 View는 프로젝트 B 전용 → `Assets/20_ProejctB/01_Scripts/Room/` (폴더명 `20_ProejctB`는 현재 리포지토리의 실제 철자)
- 공통 코어에 특정 룸·씬 이름 같은 구체 참조가 역류하지 않게 한다

### 공통 SceneService 확장

현재 `O2un.Manager.ISceneService`는 `UniTask LoadSceneAsync(string sceneName)` 하나뿐이며 **Single 모드 + `UnityEngine.SceneManagement` 직접 호출**이다. Additive·Addressables·부모 스코프 주입 경로가 전부 없으므로 신규 작성한다.

**구현은 기존 `SceneManager`에 두고, 계약만 인터페이스로 분리한다.**

```csharp
public interface IAdditiveSceneLoader
{
    UniTask<Scene> LoadAdditiveSceneAsync(string key, LifetimeScope parentScope);
    UniTask UnloadSceneAsync(Scene scene);
}
```

- `SceneManager`가 `ISceneService`와 `IAdditiveSceneLoader`를 함께 구현한다. 소비처(`RoomModule`)는 `IAdditiveSceneLoader`만 본다
- 로드 직전 `LifetimeScope.EnqueueParent(parentScope)`를 `using`으로 감싸 그 사이에 `Addressables.LoadSceneAsync(key, LoadSceneMode.Additive)`를 실행한다. 이 큐잉 없이는 additive 씬의 LifetimeScope가 베이스 스코프를 부모로 잡지 못한다
- 언로드 시 Addressables 씬 핸들을 함께 해제한다
- 기존 Single 로드 흐름(로딩 씬 경유·진행률)과 상태 머신을 건드리지 않는다

### RoomModule (순수 C#)

- `MonoBehaviour` 상속·`Transform`/`GameObject` 조작·씬 탐색 없음. `new`로 생성 가능
- 생성자 주입: 스테이지 데이터, `IAdditiveSceneLoader`, 스폰 계약, 페이드 계약
- 보유 상태: 현재 룸 인덱스, 룸 상태(`Loading`/`Playing`/`Cleared`/`Transitioning`)
- 외부 알림은 C# event 금지, R3 `Subject`/`Observable`로 노출 (예: `OnRoomEntered`, `OnStageCleared`, `OnLoadFailed`)
- 전환 순서를 이 순서 그대로 강제: 페이드 아웃 → 현재 룸 언로드 → 다음 룸 로드 → 플레이어 배치 → 페이드 인 → 스폰
- 전환 중 중복 문 신호는 무시한다

### 룸 로드 실패 처리

- 룸 로드 실패(주소 누락·로드 예외)는 **게임 진행 불가**로 간주한다. 복구·재시도하지 않는다
- 실패 시 페이드를 유지한 채 기존 `ISceneService`의 Single 로드로 **로비(게임 선택) 씬으로 복귀**한다
- 실패 사유를 에러 로그로 남긴다. 실패를 삼키고 다음 룸으로 진행하지 않는다

### 룸 데이터

- `RoomDataSO`: additive 씬 Addressables 주소 + 그 룸의 웨이브 시퀀스
- `StageDataSO`: `RoomDataSO` 시퀀스 + 페이드 아웃/인 시간
- DI 등록: 소비처가 `RoomModule` 하나뿐이면 전역 `RegisterInstance` 대신 `WithParameter(...)`로 넘긴다

### 스폰 & 클리어

- 스폰 트리거는 **시간 기반이 아니라 처치 기반**이다. 룸 진입 시 첫 웨이브를 배치하고, 그 웨이브의 적이 전부 처치되면 다음 웨이브를 배치한다. 마지막 웨이브까지 전부 처치되면 룸 클리어다
- `RoomModule`은 스포너 구체 클래스가 아니라 인터페이스(예: `IRoomSpawner`: 스폰 시작 + `Observable<Unit> OnCleared`)에만 의존한다
- 웨이브 진행 판단(현재 웨이브 잔존 수 → 다음 웨이브/클리어)은 스폰 쪽 순수 Module이 갖는다. `RoomModule`은 룸 클리어 신호만 받는다

### 공통 승격 대상

프로젝트 A의 스폰 시스템을 그대로 참조하면 프로젝트 경계 역류가 되므로, 핵심 부분을 공통으로 옮긴다.

- `IEnemyKillEvent`는 인터페이스이므로 `00_CommonFramework`로 승격한다. ProjectA는 승격된 계약을 그대로 쓴다
- 스폰 시스템도 공통으로 승격하되, 아래를 정리한 뒤 옮긴다
  - `Teleport`의 `CharacterController`, `StartAsync`의 `Physics.IgnoreLayerCollision` 등 **3D API 의존을 제거**한다. 공통 코어는 2D/3D 어느 쪽 컴포넌트에도 직접 묶이지 않아야 하며, 배치·충돌 처리 차이는 프로젝트 측 구현으로 내린다
  - 시간 기반 웨이브(`Tick`+`_timer`) 트리거를 **처치 기반 트리거**로 일반화한다. 시간 기반이 필요하면 트리거 종류를 데이터로 고른다
- 승격 후 ProjectA의 기존 스폰 동작이 그대로 유지되어야 한다 (회귀 없음)

### 문 View (MonoBehaviour)

- `Collider2D`(`isTrigger = true`) + `OnTriggerEnter2D`/`OnTriggerExit2D`로 플레이어 범위 판정
- 클리어 전에는 비활성 — 열리기 전에는 프롬프트도 신호도 발생하지 않는다
- 프롬프트 UI는 **문 View가 소유한 월드 스페이스 UI**다. 베이스 씬 HUD를 거치지 않는다. 범위 안 + 열린 상태에서 표시, 이탈 시 숨김
- 입력은 공통 `IInputReader.IsAttackPressed` (`Observable<Unit>`) 구독. 범위 밖에서 누른 입력은 무시
- **한 룸에 문이 여러 개일 수 있도록** 설계한다. 문은 자신의 목적지 식별자를 들고 R3 신호로 전달하며, `RoomModule`은 문 개수를 가정하지 않는다. (이번 범위에서는 룸당 문 1개만 배치)
- `RoomModule`에는 신호만 전달하고 진행 판단은 하지 않는다

### 페이드 View (MonoBehaviour)

- 전체 화면 오버레이의 알파를 `UniTask`로 보간. 코루틴 금지
- Single 로드용 로딩 화면과 별개 — 로딩 씬을 경유하지 않는다

### DI / 플레이어 배치

- `RoomModule`은 베이스 씬 스코프(`ProjectBSceneScope` 계열)에 등록하고, 이 스코프가 additive 로드 시 부모로 넘어간다
- **룸 씬마다 자체 `LifetimeScope`를 둔다.** 스코프 없는 룸 씬은 허용하지 않는다
- 룸 씬 스코프는 그 룸의 스폰 지점·스폰 배치를 담당하는 컴포넌트를 갖고, **룸 쪽에서 베이스 씬 플레이어를 스폰 지점으로 이동시킨다.** 베이스 씬이 룸 안을 탐색하지 않으므로 씬 탐색 금지 규칙과 충돌하지 않는다
- 플레이어는 부모 스코프에서 계약(예: `IPlayerPlacer`)으로 해소한다. 구체 클래스 직접 참조 금지
- `LifetimeScope` 밖에서 `Container.Resolve<>()` 직접 호출 금지

## Acceptance Criteria

- [ ] 룸을 넘어가도 베이스 씬의 플레이어·카메라·HUD가 재생성되지 않고 그대로 유지된다
- [ ] 다음 룸이 additive로 로드되고 이전 룸 씬은 언로드되어 씬 목록에 남지 않는다
- [ ] 룸 씬 안의 오브젝트가 베이스 씬 공통 시스템을 주입받아 동작한다 (주입 실패 예외 없음)
- [ ] 룸 진입 시 플레이어가 그 룸의 스폰 지점으로 이동한다
- [ ] 룸 진입 시 첫 웨이브가 배치되고, 웨이브를 전부 처치하면 다음 웨이브가 배치된다
- [ ] 마지막 웨이브까지 처치하면 문이 열린다. 적이 남아 있으면 열리지 않는다
- [ ] 문이 열린 뒤 범위에 들어가면 문의 월드 스페이스 프롬프트가 뜨고, 벗어나면 사라진다
- [ ] 프롬프트가 뜬 상태에서 공격 키를 누르면 전환이 시작된다. 범위 밖 입력은 무시된다
- [ ] 전환 중 페이드가 로드·언로드 순간을 가려 맵이 사라지거나 튀어나오는 장면이 보이지 않는다
- [ ] 전환 중 문 입력을 반복해도 룸이 두 번 넘어가지 않는다
- [ ] 데이터에 룸을 추가·재배열하면 코드 수정 없이 스테이지 순서가 바뀐다
- [ ] 마지막 룸을 클리어하면 스테이지 클리어 신호가 한 번 발행된다
- [ ] 룸 씬 주소를 잘못된 값으로 두면 에러 로그와 함께 로비 씬으로 복귀한다
- [ ] 스폰 시스템 공통 승격 후에도 ProjectA의 기존 스폰 동작이 그대로 유지된다
- [ ] Play 모드 진행 중 콘솔에 에러가 없다

## Open Questions

- 공통 스포너에서 3D 배치 로직을 걷어낼 때 ProjectA 쪽 대체 구현(`CharacterController` 이동, 레이어 충돌 무시)을 어디에 둘지 — ProjectA 전용 어댑터로 내리는 것을 전제했으나 구현 시 확인 필요
- 로비(게임 선택) 씬의 실제 씬 이름/주소
