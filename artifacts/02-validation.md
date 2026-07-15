# 02. 검증 (4단계 게이트)

작성: unity-validate hook (MCP 미연동 → 수동 검증 필요)
일시: 2026-07-16 00:45

## 게이트 진행 요약
| 단계 | 결과 | 시간 |
| --- | --- | --- |
| 1. 컴파일 | 🔧 수동 검증 필요 | 00:45 |
| 2. 런타임 에러 | 🔧 수동 검증 필요 | 00:45 |
| 3. 기능 점검 (자동) | 🔧 수동 검증 필요 | 00:45 |
| 4. 기능 점검 (사용자) | ⏳ 대기 (hooks 뷰어에서 제출 예정) | - |

## 수동 검증 체크리스트 (MCP 미연동)
1. [ ] 콘솔에 컴파일 에러가 없는지 확인
2. [ ] Play 모드 진입 후 LogError·Exception이 없는지 확인
3. [ ] 대상 기능이 의도대로 동작하는지 확인
4. [ ] 이상 시 콘솔 에러 복사 → debugger에 전달

> MCP for Unity가 실행 중이 아니어서 1~3단계 자동 검증을 건너뛰었습니다. Unity 에디터에서 직접 확인 후 4단계에 입력하세요.

## 이번 변경의 정적 검증

- `git diff --check`: 통과. 공백 오류가 없다.
- `Player2DContext`: `Update()`와 `FixedUpdate()` 선언이 검색되지 않는다.
- `Player2DActor`: `IActorTickable`, `IActorFixedTickable`을 모두 구현한다.
- `ActorManager`: VContainer `ITickable`, `IFixedTickable`을 구현하며, 등록된 `IActor` 중 Actor용 인터페이스 구현체만 별도 목록에 포함한다. Actor용 인터페이스 자체는 `IActor`를 상속하지 않아 불필요한 계약 결합이 없다.
- 활성 상태 필터: `Unregister`에서 활성 집합과 Tick 목록을 제거하고, 스냅샷 호출 직전에도 활성 여부를 확인한다.
- `ProjectBSceneScope`와 ProjectA `GameSceneScope` 모두 `RegisterEntryPoint<ActorManager>().AsSelf()`를 사용한다.
- `PlayerActor`, `NpcActor`, `Player2DActor`가 필요한 Actor Tick 인터페이스를 구현한다.
- Player/NPC/Player2D Context에서 Actor의 `Tick`/`FixedTick` 직접 호출이 검색되지 않는다.
- 보조 `dotnet build Assembly-CSharp.csproj --no-restore`: 제한 시간 내 출력 없이 완료되지 않아 판정 근거로 사용하지 않았다.

## Gate 3 설정

- `.claude/hooks/.viewer-state/gate3-test.json`을 이번 중앙 루프 변경에 맞게 갱신했다.
- `gate3_run_test`는 `MonoBehaviour`만 생성할 수 있어 순수 C# `ActorManager`를 직접 실행할 수 없다. 설정은 `Player2DContext` 타입 생성 회귀를 대상으로 하고, 중앙 루프 계약은 위 정적 검사와 실제 Play 검증으로 보완한다.
- `inject=false`: Gate 3 임시 오브젝트는 `ProjectBSceneScope._sceneInitializables` 경로 밖이어서 실제 씬 스코프의 주입 및 `Init` 흐름을 재현할 수 없다.

## 남은 수동 검증

1. Unity 에디터에서 스크립트 refresh 후 컴파일 오류가 없는지 확인한다.
2. ProjectB 씬 Play 후 Console의 `Error`/`Exception`이 없는지 확인한다.
3. 이동과 점프가 정상이며, `PlayerView` 비활성화 또는 Context 파괴 후 Actor 호출이 중단되는지 확인한다.
4. Tick 도중 Actor가 해제된 경우 같은 스냅샷에서 호출되지 않는지 확인한다.
5. ProjectA Play에서 플레이어/적이 정상 속도로 한 번씩만 Tick되는지 확인한다.
