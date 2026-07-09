# 개선 기록 (improvement-log)

이 파일은 **가장 최근 실행 1건**만 담는 단일 스냅샷이다. 다음 실행의 Phase 0-A가 그대로 읽어 연속 작업 기준으로 삼는다. 새 실행이 끝나면 통째로 덮어쓴다(이전 내용은 덮어쓰기 전에 chain-log.md로 축약 누적).

---

## 2026-07-09 · ExperienceSystem(PRD 2 · 경험치 누적·레벨업) 구현

### 무엇을 했나
- `docs/requirements/experience-system.md`(4 PRD 분해 중 2건째) 구현. 경험치 루프 가운데 — PRD 1(`ItemDrop`, 이미 커밋됨)의 `IExpGainedSource.OnGained` 계약을 입력으로 받아 누적·레벨판정하고, `LevelUpEvent`를 결과로 발행.
- scope-gate 판정: 한 번에 진행(단일 Module 경계, 신규 파일 3~4개 수준).
- 신규 파일 (전부 `Assets/10_ProjectA/01_Scripts/Progression/Experience/`, 네임스페이스 `O2un.Progression`):
  - `ExperienceModule` — 순수 C#(`new` 가능), `IExperienceReader`+`IExperienceWriter`+`IDisposable`. 생성자로 `AnimationCurve`만 받아 `.Evaluate()`만 호출(SO 자체는 미보유). `Gain(amount)`이 while 루프로 초과분 이월하며 레벨마다 `LevelUpEvent` 1회씩 발행. `RequiredExp(level) = Math.Max(1, Mathf.RoundToInt(curve.Evaluate(level)))`로 무한루프 가드.
  - `IExperienceReader`/`IExperienceWriter` — `PlayerDataStore` 선례를 따른 읽기/쓰기 분리.
  - `LevelUpEvent` — readonly struct, `NewLevel` 필드만(YAGNI, PreviousLevel은 자명해서 제외).
  - `ExperienceDataSO` — `AnimationCurve` 인스펙터 편집 지점(신규 SO, `ItemDropDataSO`/`WaveDataSO` 패턴).
  - `ExperienceGainContext` — `IInitializable`, `IExpGainedSource.OnGained` 구독 → `IExperienceWriter.Gain` 호출. `ExperienceModule`이 아이템 도메인을 직접 참조하지 않도록 배선만 전담(PRD 본문엔 없는 신규 클래스, 게이트A에서 승인).
- 수정: `GameSceneScope.cs`에 `_experienceData` 필드 + 3개 DI 등록 라인 추가.

### 사용자 승인 (게이트 A)
- 배치: `10_ProjectA` 전용, 신규 폴더 `Progression/Experience` 확정.
- `ExperienceGainContext` 신설 승인(`ItemDropContext`와 유사한 배선 전용 엔트리포인트).
- `ExperienceDataSO` 신설 승인(raw 필드 대신 SO 패턴 유지).
- `IExperienceReader`/`IExperienceWriter` 분리 유지 승인.

### 게이트 B (에셋·씬 배선, 승인 후 unity-ai-operator 수행)
- `Assets/10_ProjectA/03_Data/ExperienceData.asset` 신규 생성(레벨1→10, 레벨5→50, 레벨10→150, ClampForever 우상향 커브 — 무한 레벨업 가정 보장).
- `Assets/Scenes/GameScene.unity`의 `GameSceneScope._experienceData`에 할당, 씬 저장.

### 4단계 게이트
- ①컴파일 ✅ (초기 통과 + 리뷰 반영 후 재검증까지 2회 모두 에러 0).
- ②런타임 ✅ Play 콘솔 에러 0, `_experienceData` null NRE 없음.
- ③기능 ✅ **정량 실측**(unity-ai-operator, 임시 `IStartable` 러너로 실측 후 완전 제거·원복):
  - `Gain(5)`: exp 0→5, 레벨업 없음(필요치 10 미만) — 정상.
  - `Gain(200)`: exp 205 누적 → 레벨 1→6, 5단계 동시 상승, 초과분 55 이월, `OnLevelUp` 정확히 5회 발행 — 다중 레벨업 계약 충족.
  - `IExpGainedPublisher.Publish(7)`: `ExpGainedChannel → ExperienceGainContext → ExperienceModule.Gain` 배선 통합 확인(exp 55→62, 필요치 60 초과로 재차 레벨업) — PRD1↔PRD2 하류 계약 연결 검증 완료.
  - 임시 테스트 코드는 컴파일 클린 상태로 완전 원복 확인.
- ④사용자 ⏳ (viewer).

### 리뷰 (code-reviewer)
- blocker 0 / major 1(반영 완료) / minor 2(비차단, 기존 관례 그대로라 미반영).
- **Major(반영 완료)**: `ExperienceModule.cs`가 Reactive 필드(`_currentExp`/`_currentLevel`/`_onLevelUp`)를 생성자 위에 선언 — convention §5(생성자 위=DI 받은 것만, 생성자 아래=Reactive) 위반. 필드 순서 재배치로 수정, 재컴파일 검증 통과.
- **Minor(비차단, 보류)**: (1) `while` 조건과 루프 본문에서 `RequiredExp` 중복 호출(설계 의사코드와 동일 구조, 성능상 미미). (2) `GameSceneScope._experienceData` null 가드 없음(기존 `_itemDropData`/`_waveData`도 동일 패턴이라 이번 변경이 새로 만든 문제 아님).
- 설계(`01-design.md`)-구현 매칭: 클래스 6개 전부 시그니처·책임 일치, DI 등록 코드도 설계 §4 초안과 완전 동일. 이탈 없음.

### 하네스 교훈
- **AnimationCurve를 Module에 값으로만 주입하는 패턴**: `ScriptableObject` 자체가 아니라 `.RequiredExpCurve` 값만 생성자에 전달하면, Module이 Unity Object 수명(에셋 로드/언로드)에 결합되지 않고 `new`로 완전히 독립적인 유닛 테스트가 가능해진다. "Unity API 의존 금지"의 실질 기준은 "GameObject/Component/Collider/Time처럼 라이브 씬 상태에 결합됐는가"이지 "UnityEngine 네임스페이스를 한 글자도 안 쓰는가"가 아니다 — `AnimationCurve`/`Vector3`/`Mathf` 같은 값 타입은 예외.
- **PRD가 명시하지 않은 배선 클래스(Context)가 필요할 수 있다**: PRD는 도메인 로직(`ExperienceModule`)만 명시했지만, 상류 계약(`IExpGainedSource`)과 연결하려면 별도 엔트리포인트가 필요했다. 설계 단계에서 이런 "PRD엔 없지만 배선을 위해 필요한 신규 클래스"를 명시적으로 게이트 A 승인 대상에 포함시켜야 사용자가 범위 확장을 인지할 수 있다.

### 남은 개선/후속 (비차단)
- Minor 2건(위 참고)은 필요 시 후속 정리.
- 유닛테스트: `ExperienceModule`은 완전한 순수 C#이라 EditMode 테스트로 다중 레벨업·이월·커브 엣지케이스(0/음수 반환) 자동화 가능 — 아직 미작성.
- PRD 3(`LevelUpSelection`)이 아직 없어 `LevelUpEvent` 실사용 검증(레벨업 선택 UI, 일시정지)은 못함. `IExperienceReader.OnLevelUp`을 그대로 구독하면 된다.

### 다음 테스트 (다음 실행 입력)
- game-plan/PRD 순서상 다음은 **PRD 3 `LevelUpSelection`**(레벨업 시 선택지 UI + 게임 일시정지 + 능력 적용). `docs/spec/LevelUpSelection.md`가 이미 준비돼 있다. 입력은 이번 `IExperienceReader.OnLevelUp`.
