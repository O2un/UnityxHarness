# Project A 코드 규칙 Eval 결과

점검 대상: `Assets/00_CommonFramework/00_Scripts/**`, `Assets/10_ProjectA/01_Scripts/**` (in-scope). `_3rd`, `99_3rd`, `51_3D_Resources/UnityChan`은 서드파티로 점검 제외.

## 요약

- Pass: 대부분의 금지 패턴·경계·의존 방향·Addressables 등록 항목
- Issue: 2건 (R3 lifecycle 미바인딩 재발 사례)
- Needs Play Check: 3건

---

## Pass 항목

**P1. 씬 전역 탐색 금지 (`FindObjectOfType`/`GameObject.Find`)**
- 근거: CLAUDE.md §5, convention.md 캐싱 규칙("FindObject는 사용하지 않는다")
- 상태: in-scope 코드 전 범위 grep 결과 0건. `GetComponentInParent(true)`(`NpcContext.cs:56`) 등은 자식/부모 캐싱으로 허용 범위. Pass

**P2. static Instance 싱글턴 직접 구현 금지**
- 근거: CLAUDE.md §5
- 상태: `static ... Instance` 패턴 0건. DI(VContainer)로 일관 주입. Pass

**P3. `Resources.Load` / `PlayerPrefs` 직접 접근 금지**
- 근거: CLAUDE.md §5 (AssetService/SaveService 사용)
- 상태: 0건. 에셋 로드는 전부 `IAssetService.LoadAsync`(`AssetManager.cs:15`) 경유. Pass

**P4. 비동기 혼용 금지 (Coroutine/Thread/Task)**
- 근거: CLAUDE.md §4
- 상태: `StartCoroutine`/`new Thread`/`IEnumerator`/`Task` 0건. 전부 UniTask. Pass

**P5. R3 복잡 Operator 체이닝 금지**
- 근거: CLAUDE.md §4 (SelectMany/FlatMap/Zip 등 금지)
- 상태: 0건. 사용 Operator는 `Subscribe`/`AddTo`뿐. Pass

**P6. Module 순수성 (Unity 씬·오브젝트 조작 없음)**
- 근거: CLAUDE.md §3 Module 규칙
- 상태: `*Module.cs` 중 씬 API 사용은 `PoolModule.cs`(`ObjectPool`/`Transform`)뿐이며, chain-log(2026-07-05 PoolManager)에 "PRD 명시 예외"로 기록됨. `WaveModule`/`SkillModule`/`ExperienceModule`/`LevelUpSelectionModule`은 순수 C#(`Mathf`/`Random`은 §3 허용 유틸). Pass

**P7. 의존 방향 (Module→Manager 역참조·Module간 직접 참조 없음)**
- 근거: CLAUDE.md §3 "역방향 금지, Module끼리 직접 참조 금지"
- 상태: Module들은 Manager를 참조하지 않고 인터페이스(`IActorQuery`, `ISkillContext` 등)로만 의존. `LevelUpSelectionModule`이 `IActorQuery` 통해 Player 조회 — 인터페이스 경유로 규칙 준수. Pass

**P8. 공통/프로젝트 코드 배치 경계**
- 근거: CLAUDE.md §2 (공통 인프라는 `00_CommonFramework`, 프로젝트 전용은 `10_ProjectA`)
- 상태: 인프라(AI/Actor/Combat 골격/Pool/Asset/Audio/Score/Inventory)는 `00_CommonFramework`, 콘텐츠(Npc AI 구현·Progression·GameManager·EnemySpawner·ItemDrop)는 `10_ProjectA`. chain-log 게이트 A 승인 기록과 일치. Pass

**P9. 네임스페이스 컨벤션 (`프로젝트명.대분류`)**
- 근거: convention.md 네임스페이스 규칙
- 상태: 전 파일 `O2un.*` 접두. 다만 아래 W1 참고(대소문자/일관성 경미 편차). Pass(규칙 위반 아님)

**P10. Addressables 등록·참조 정합성 (코드 상수 ↔ 그룹 등록)**
- 근거: chain-log 2026-07-06 재발방지("정식 API 등록·해소 검증 필요")
- 상태: 코드 사용 키 `Enemy_Slime`/`Enemy_Dash`/`Enemy_ArmoredMelee`(`WaveDataSO` 경유), `Item_Gem`, `sfx/*`×5, `bgm/battle`가 전부 `AssetGroups/*.asset`에 `m_Address`로 등록됨. Pass (단, 런타임 카탈로그 해소는 N3 참조)

**P11. DI 죽은 등록 / RegisterInstance 중복**
- 근거: CLAUDE.md §4 "아무도 주입받지 않는 RegisterInstance 제거, WithParameter와 중복 주의"
- 상태: `GameSceneScope.cs:56-59` — `_experienceData`를 `RegisterInstance`하면서 동시에 `ExperienceModule`에 `WithParameter("requiredExpCurve", _experienceData.RequiredExpCurve)`로 커브를 넘김. `ExperienceDataSO` 타입을 주입받는 소비처가 `ExperienceModule` 하나뿐이라면 CLAUDE.md가 경계한 "WithParameter로 다 넘기면서 원본 SO도 RegisterInstance하는 중복"에 해당할 수 있음. 다만 `ExperienceGainContext` 등 다른 소비처가 SO 타입을 직접 받는지 코드상 확정 불가 → 경미. 현재는 위반으로 단정하지 않고 Pass 처리하되 정리 후보로 기록.

---

## Issue 항목

### I1. `HudVM`의 `CurrentHP` 구독이 lifecycle에 바인딩되지 않음 (미해제 구독)
- 파일·라인: `Assets/00_CommonFramework/00_Scripts/UI/Hud/HudVM.cs:17-20`
- 규칙·기록 근거: convention.md "구독은 `.AddTo(DisposableR3)`"; chain-log에서 R3 필드 순서·구독 누락이 다수 실행(2026-07-09 ExperienceSystem major1 "Reactive 필드 순서 위반", 2026-07-05 이후 반복)에서 재발한 항목.
- 현재 상태: `playerData.CurrentHP.Subscribe(...)`에 `.AddTo(...)`도 없고 반환 `IDisposable`을 필드로 잡지도 않음. `Dispose()`(24행)는 `_currentHp`만 해제하고 이 구독은 방치 → `HudVM` 파기 후에도 `playerData.CurrentHP`가 살아있는 동안 콜백이 유지되어 파기된 `_currentHp.Value`에 기록 시도(ObjectDisposedException 위험) 및 구독 누수.
- 다음 확인 방법: 코드상 확정된 위반. 재현은 씬 전환/재시작으로 `HudVM` 재생성 시 콜백 중복 여부를 Play에서 관찰 가능.

### I2. `ItemDropContext`가 `IDisposable` 미구현 — `_disposables`/`pickSubscription` 미해제
- 파일·라인: `Assets/10_ProjectA/01_Scripts/Actor/Item/ItemDropContext.cs:11, 19, 50, 65-70`
- 규칙·기록 근거: convention.md 구독 lifecycle 규칙; chain-log 2026-07-09 ItemDropSystem 항목(하류 계약 배선체).
- 현재 상태: 클래스가 `IAsyncStartable`만 구현하고 `IDisposable`은 미구현. 50행 `_killEvent.OnKilled.Subscribe(...).AddTo(_disposables)`로 담아둔 `_disposables`(19행)를 해제하는 `Dispose()`가 없어 VContainer 스코프 파기 시 정리되지 않음. 또한 66행 `pickSubscription`은 픽업 콜백 안에서만 self-dispose하므로, 픽업 없이 아이템이 파기/스코프 종료되면 누수. `GameSceneScope.cs:54`는 `RegisterEntryPoint<ItemDropContext>()`로 등록 → 스코프 종료 시 Dispose 훅이 없어 구독이 남음.
- 다음 확인 방법: 코드상 확정된 위반. Play에서 씬 재시작 반복 시 kill 이벤트당 드랍 콜백 중복 여부로 실증 가능.

---

## Needs Play Check 항목

### N1. `Time.timeScale` 전역 상태 복원의 실제 동작
- 파일·라인: `Assets/10_ProjectA/01_Scripts/Manager/GameManager/GameManager.cs:87, 133`
- 규칙·기록 근거: 전역 상태 부작용은 코드만으로 순서 보장 확인 불가.
- 현재 상태: `Playing`이 아닐 때 `timeScale=0`. `Dispose()`에서 1f 복원. 그러나 `EnemySpawnManager.Tick`/`NpcContext.Update`가 `Time.deltaTime` 기반이라 일시정지·레벨업 팝업 중 스폰·AI 정지가 의도대로 되는지, 재시작 시 timeScale 복원 타이밍이 맞는지는 런타임 관찰 필요.
- 다음 확인 방법: Play에서 레벨업/일시정지/재시작 시 timeScale과 스폰·AI 정지·복귀 확인.

### N2. 씬 MonoBehaviour Context의 `autoInjectGameObjects` 등록 여부
- 파일·라인: `GameSceneScope.cs`(전반), `NpcContext.cs:28` `[Inject] Construct`, 각 View의 `Bind`
- 규칙·기록 근거: chain-log 2026-07-09 LevelUpSelection 교훈 "autoInjectGameObjects 등록 누락 시 조용히 아무 일도 안 함(예외·로그 없음)" — 재발 방지 대상.
- 현재 상태: `GameSceneScope`에는 씬 오브젝트용 `RegisterComponentInHierarchy`가 `AudioPlayerView` 1건뿐. `NpcContext`/UI View들의 메서드 주입(`autoInjectGameObjects`) 등록은 코드가 아니라 씬/LifetimeScope 인스펙터 설정에 있어 코드 정적 점검으로 확인 불가.
- 다음 확인 방법: Play에서 각 Context의 `Construct`/`Bind` 실제 호출 여부(주입 성공)를 기능 동작으로 확인.

### N3. Addressables 런타임 카탈로그 해소
- 파일·라인: `EnemySpawnManager.cs:59`, `ItemDropContext.cs:37`, `AudioManager` 로드부, `AssetGroups/*.asset`
- 규칙·기록 근거: chain-log 2026-07-06 재발방지 "그룹 YAML 편집만으론 런타임 미해소, 카탈로그 미갱신 시 LoadAsync 조용히 대기".
- 현재 상태: 주소 등록은 그룹 asset에 존재(P10). 하지만 카탈로그/PlayMode Script 반영 여부는 정적으로 확인 불가하며, async EntryPoint가 예외를 삼켜 무증상 스폰0가 될 수 있음. 참고: chain-log가 언급한 `Enemy_Basic` 키는 현재 코드 참조가 없어(현 웨이브는 `Enemy_Slime` 등 사용) 정합성 문제 아님.
- 다음 확인 방법: Play에서 `LoadResourceLocationsAsync` 해소 카운트(=1)와 실제 스폰·드랍·오디오 재생 확인.

---

## 참고(비차단, 경미)

- W1. `namespace O2un.Data`/`O2un.Data `, `O2un.UI`/`O2un.UI ` 등 동일 네임스페이스에 후행 공백 편차가 파일별로 섞여 있음(`ProjectLifetimeScope`가 참조하는 `O2un.Data`). 컴파일 무해, 규칙 위반 아님. 정리 후보.
- W2. `GameSceneScope.cs:56` `RegisterInstance(_experienceData)`의 실제 소비처 유무는 P11 참조. 소비처가 `ExperienceModule` 단독이면 `WithParameter`와 중복이므로 죽은 등록 정리 후보.
