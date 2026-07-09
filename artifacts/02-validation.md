# 02. 검증 (4단계 게이트)

작성: unity-ai-operator (MCP for Unity 수동 실측)
일시: 2026-07-09 (ExperienceSystem, PRD 2)

## 게이트 진행 요약
| 단계 | 결과 | 비고 |
| --- | --- | --- |
| 1. 컴파일 | ✅ 통과 | refresh_unity(force/compile), 에러 0 |
| 2. 런타임 에러 (Play) | ✅ 통과 | `_experienceData` NRE 없음, 콘솔 에러 0 |
| 3. 기능 점검 (실측) | ✅ 통과 | 임시 EntryPoint로 IExperienceReader/Writer/IExpGainedPublisher 실측 후 제거 |
| 4. 기능 점검 (사용자) | ⏳ 대기 | 사용자 최종 확인 필요 |

## 배선 내역
- `Assets/10_ProjectA/03_Data/ExperienceData.asset` 신규 생성 (guid `a5d98c30b1ff045c9bb8616f36ce162e`).
  - `_requiredExpCurve` 키: (level1, 10), (level5, 50), (level10, 150), pre/post infinity = ClampForever(2). 우상향, extrapolate 시 0/음수로 떨어지지 않음.
- `Assets/Scenes/GameScene.unity`의 `GameSceneScope._experienceData`에 위 asset 할당 후 씬 저장 완료.

## 1단계 컴파일
- `refresh_unity(mode=force, compile=request)` → `read_console(types=[error])` 결과 0건.

## 2단계 런타임 에러
- Play 모드 진입, `read_console(types=[error, warning])` 결과 0건. `_experienceData` null 관련 NRE 없음 확인.

## 3단계 기능 점검 (실측)
검증 방법: `_TempExperienceTestRunner`(임시 `IStartable`, DI로 `IExperienceReader`/`IExperienceWriter`/`IExpGainedPublisher` 주입)를 `GameSceneScope.Configure()`에 일시적으로 `RegisterEntryPoint`하여 Play 모드에서 로그로 실측. 검증 완료 후 등록 라인과 스크립트 파일을 모두 제거하고 재컴파일 클린 확인함(원복 완료).

실측 로그:
```
[ExpTest] initial exp=0 level=1
[ExpTest] after Gain(5) exp=5 level=1
[ExpTest] after Gain(200) exp=55 level=6 levelUpCount=5
[ExpTest] after Publish(7) exp=2 (expBefore=55) levelUpCount=6
```

- **누적**: `Gain(5)` 후 `CurrentExp`가 0→5로 증가. 레벨1 필요치(10) 미만이라 레벨업 없음 — 정상.
- **다중 레벨업 + 이월**: `Gain(200)`으로 exp 205(5+200)가 누적되며 레벨이 1→6으로 5단계 한 번에 상승, 초과분이 55로 이월됨. `OnLevelUp`이 정확히 5회 발행됨(`levelUpCount=5`) — while 루프 기반 다중 레벨업·이월·이벤트 발행 횟수 모두 일치.
- **퍼블리셔 통합 배선**: `IExpGainedPublisher.Publish(7)` 호출 시 `ExpGainedChannel.OnGained` → `ExperienceGainContext.Initialize()` 구독 → `ExperienceWriter.Gain(7)` 경로를 통해 `CurrentExp`가 55→62로 반영된 뒤 레벨6 필요치(60)를 넘어 다시 레벨업(2로 이월, `levelUpCount` 5→6) — `ExperienceGainContext` 배선이 실제로 동작함을 간접 확인.

## 4단계 (사용자 확인)
- 대기 중. 게이트 1~3은 모두 통과.

## 재검증 (code-reviewer major 반영 후)
- 대상: `Assets/10_ProjectA/01_Scripts/Progression/Experience/ExperienceModule.cs` — Reactive 필드를 생성자 아래로 재배치(필드 선언 순서만 변경, 로직 변경 없음).
- `refresh_unity(mode=force, compile=request)` → 완료 대기(`editor_state` idle) → `read_console(types=[error])` 결과 0건. 컴파일 정상 통과.
- 씬·에셋 변경 없음.
