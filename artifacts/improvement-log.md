# 개선 기록 (improvement-log)

이 파일은 **가장 최근 실행 1건**만 담는 단일 스냅샷이다. 다음 실행의 Phase 0-A가 그대로 읽어 연속 작업 기준으로 삼는다. 새 실행이 끝나면 통째로 덮어쓴다(이전 내용은 덮어쓰기 전에 chain-log.md로 축약 누적).

---

## 2026-07-09 · LevelUpSelection(PRD 3 · 경험치 루프 마지막) 구현

### 무엇을 했나
- `docs/requirements/level-up-selection.md`(4 PRD 분해 중 3건째, 마지막은 게임오버 등 별도) 구현. PRD 2(`ExperienceSystem`)의 `IExperienceReader.OnLevelUp`을 구독해 게임을 멈추고, 능력 후보 3개를 버튼으로 보여준 뒤 선택 시 적용하고 재개하는 View/VM/Context UI 계층.
- scope-gate 판정: 한 번에 진행(걸린 기준 1개 — 레이어 다수 신설).
- **공통 인프라(00_CommonFramework) 확장** — PRD 요구사항(레벨 덮어쓰기)을 만족할 근거 데이터가 전혀 없어 게이트A 승인 하에 신설:
  - `ISkillDefinition`/`SkillDefinitionSO`에 `SkillId`(string)/`Level`(int) 필드 추가.
  - `SkillModule` 내부 저장을 `List<SkillSlot>`(정의+타이머 nested class)로 교체, `bool AcquireOrUpgrade(ISkillDefinition)` 신설(미보유=신규 슬롯, 보유+상위레벨=교체·쿨다운 유지, 그 외=무시).
  - 신규 `IPlayerSkillReceiver` 인터페이스, `PlayerActor`가 구현해 `SkillModule.AcquireOrUpgrade`에 위임.
  - 기존 스킬 SO/구현 3쌍(Melee/Projectile/Aura)에 `skillId, level` 생성자 인자 추가.
- 신규 파일(`Assets/10_ProjectA/01_Scripts/Progression/LevelUpSelection/`, 네임스페이스 `O2un.Progression`):
  - `LevelUpSkillPoolSO` — 후보 풀 데이터(SkillDefinitionSO[]).
  - `LevelUpSelectionViewModel` — 순수 C#, `IsVisible`/`CandidateLabels`(ReactiveProperty)·`OnCandidateChosen`(Subject) 노출.
  - `LevelUpSelectionView` — `HudView` 패턴, 버튼 3개 배열 직접 바인딩(버튼뷰 별도 분리 안 함 — 구현 단순성 우선, 설계에서 재량으로 남긴 사항).
  - `LevelUpSelectionContext` — `HudContext` 패턴(씬 MonoBehaviour, `[Inject] Init`), `OnLevelUp` 구독·`_pendingCount` 큐잉·`Time.timeScale` 정지재개·후보 무작위 추출(0개면 즉시 스킵)·`IActorQuery.Player as IPlayerSkillReceiver` 지연 캐스팅으로 적용.
- 수정: `GameSceneScope.cs`에 `_levelUpSkillPool` 필드 + `RegisterInstance` 추가.

### 사용자 승인 (게이트 A)
- 배치: `10_ProjectA` 확정, 신규 폴더 `Progression/LevelUpSelection`(`O2un.Progression`) 확정.
- 공통 인프라(`ISkillDefinition`/`SkillDefinitionSO`/`SkillModule`/`PlayerActor` + 신규 `IPlayerSkillReceiver`, 스킬 3쌍) 수정 승인.
- 열린 질문 확정: 후보 0개 시 즉시 스킵·재개, 레벨 교체 시 쿨다운 유지(리셋 안 함), SkillId는 인스펙터 수동 입력(자동보정 없음), 버튼뷰 분리는 gameplay-engineer 재량.

### 게이트 B (에셋·씬 배선, 승인 후 unity-ai-operator 수행)
- `Assets/10_ProjectA/03_Data/LevelUpSkillPool.asset` 신규 생성 — 기존 3개 SO에 `SkillId`/`Level` 값 채움 + "동일 스킬 상위 레벨" 테스트용 복제본 2개(`MeleeSwingSkill_Lv2`, `ProjectileSkill_Lv2`) 신설, 5종 등록.
- `Assets/Scenes/GameScene.unity` — 기존 `Canvas` 하위 `LevelUpSelectionPanel`(+버튼 3개) 배치, `LevelUpSelectionContext` GameObject 배치, `GameSceneScope._levelUpSkillPool` 배선, 씬 저장.

### 4단계 게이트
- ①컴파일 ✅ (1차 CS0246 2건 — 신규 .cs 미임포트 상태에서 컴파일된 것, `refresh_unity(force)`로 해소. Minor 반영 후 재검증도 에러 0).
- ②런타임 ✅ 이번 변경 관련 콘솔 에러 0. (이번 PRD와 무관한 기존 버그 1건 발견 — 하단 참고)
- ③기능 ✅ **정량 실측**(orchestrator가 직접 `execute_code`로 DI 컨테이너 조작):
  - 1차 실측에서 버그 발견: `LevelUpSelectionContext`가 `GameSceneScope.autoInjectGameObjects`(VContainer 씬 MonoBehaviour 자동주입 목록)에 등록되지 않아 `Init` 자체가 호출 안 됨(`_pool` null). unity-ai-operator가 게이트B 배선 시 GameObject/컴포넌트는 만들었지만 이 리스트 등록을 누락했던 것.
  - `SerializedObject` API로 `autoInjectGameObjects`에 등록 후 `EditorSceneManager.SaveScene`으로 수정.
  - 재실측: 단일 레벨업(`Gain(20)`) → 정지+패널 표시+후보3개 정상. 선택(`ChooseCandidate(0)`, 상위레벨 후보) → `melee_swing Lv1→Lv2` 정확히 덮어쓰기(스택 병합 아님), 재개. 다중 레벨업(`Gain(500)`→8레벨) → `_pendingCount` 8→...→0 정확히 소진, 매번 새 후보 갱신, 마지막에 정상 재개.
- ④사용자 ⏳ (viewer).

### 리뷰 (code-reviewer)
- blocker 0 / major 0 / minor 2(둘 다 반영 완료).
- M1: `LevelUpSkillPoolSO.Candidates`가 `_candidates` null일 때 방어 없음 → `?? Array.Empty<SkillDefinitionSO>()`로 수정.
- M2: `LevelUpSelectionView`가 버튼·라벨 배열 길이 불일치 가드 없음 → `Mathf.Min(길이)` 루프 상한으로 수정.
- 설계(`01-design.md` §0/§2/§7/§10) 전 항목이 구현과 1:1 일치, 컨벤션·의존 방향(Manager→Module, Context가 인터페이스만 경유) 위반 없음.

### 하네스 교훈
- **`autoInjectGameObjects` 등록은 씬 MonoBehaviour Context를 새로 배치할 때 매번 빠뜨리기 쉬운 단계다.** `HudContext`/`PlayerContext`처럼 `[Inject]`로 주입받는 씬 컴포넌트는 GameObject·컴포넌트 배치만으로는 부족하고, `LifetimeScope.autoInjectGameObjects` 배열(씬 yaml 기준 `GameSceneScope` 컴포넌트의 필드)에 반드시 등록해야 VContainer가 `Init`을 호출한다. 등록이 빠지면 컴파일·Play 콘솔 모두 에러 없이 "조용히" 아무 일도 안 일어나는 상태가 되어(예외도 로그도 없음) 발견이 어렵다 — 반드시 기능 실측(이벤트 강제 발생 후 상태 확인)으로 검증해야 잡힌다.
- **씬 파일을 Unity 에디터가 Play 모드 등으로 열어둔 상태에서 직접 텍스트 편집하면 위험하다.** Unity는 disk 변경을 즉시 반영하지 않고, 나중에 자체적으로 씬을 저장하면 수동 편집이 덮어써질 수 있다. 대신 `execute_code`로 `SerializedObject`/`SerializedProperty` API를 통해 Editor 공식 경로로 수정하고 `EditorSceneManager.SaveScene`으로 저장하는 것이 안전하다.
- **unity-ai-operator 세션에 `execute_code`가 노출되지 않는 경우가 있다** — 이번에도 씬 배선 담당 세션엔 없었다. orchestrator가 직접 `mcp__UnityMCP__execute_code`/`manage_editor`/`read_console`을 ToolSearch로 로드해 정량 실측을 대신 수행했다. 향후 세션에서도 이 패턴(에이전트가 못하면 orchestrator가 직접 MCP 툴로 보완)을 활용할 수 있다.
- **Stop hook의 자동 검증이 orchestrator가 작성한 상세 `02-validation.md`를 덮어쓸 수 있다** — 마커 파일(`validate-requested`)이 남아 있으면 Stop 시점에 자동 검증이 실행되어 파일을 짧은 자동 요약으로 교체한다. 상세 수동 실측 기록은 자동 검증 이후 다시 append해야 유실되지 않는다.

### 남은 개선/후속 (비차단)
- `Assets/00_CommonFramework/00_Scripts/Combat/Hitbox/AttackHitboxView.cs:71` — Play 모드 종료 시 `NullReferenceException` 다발(풀 해제 타이밍 레이스, `OnDespawned()`에서 `_hitbox=null` 후 `Update()`의 `_active` 체크와 `_hitbox.Tick` 호출 사이 경합으로 추정). 이번 PRD와 무관한 기존 버그 — 별도 세션에서 debugger/gameplay-engineer 투입 필요.
- 유닛테스트: `SkillModule.AcquireOrUpgrade`는 순수 C#이라 EditMode 테스트로 신규습득/상위레벨교체/동레벨무시/빈SkillId 케이스 자동화 가능 — 아직 미작성.

### 다음 테스트 (다음 실행 입력)
- game-plan 순서상 경험치 루프(처치→아이템드랍→누적→레벨업→선택→적용→재개)가 이번으로 완전히 닫혔다. 다음은 game-plan 문서의 남은 우선순위(게임오버·스코어 등) 확인 필요 — `docs/design/game-plan.md` 재확인 후 다음 PRD 결정.
- 위 히트박스 NRE 버그는 우선순위 판단 후 별도 착수 권장.
