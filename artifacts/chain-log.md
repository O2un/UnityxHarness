# 체인 로그

이 프로젝트에서 하네스로 진행한 작업 이력의 축약본. 한 실행이 끝날 때 improvement-log.md의 핵심만 요약해 누적한다.
Phase 0의 기본 입력은 improvement-log.md이며, 이 파일은 과거 내역 확인이 필요할 때만 참조한다.

## 2026-07-11 · skill-upgrade-content 구현

- 배치(게이트 A 승인): 공통 스킬·히트박스 확장은 `00_CommonFramework`, 카드 SO와 후보 필터링은 `10_ProjectA`; 카드 데이터는 `03_Data/SkillUpgradeCards` 전용 폴더.
- Lv2~Lv5 카드셋 SO 4개에 총 36장(스킬 3종 × 단계 4 × 분기 3) 구성. 카드별 에셋은 만들지 않음.
- 선택 카드 ID를 스킬 인스턴스 `SkillStats`에 기록하고, 다음 단계 카드셋의 부모 ID 일치 조건으로 분기를 축소. `SkillModule`은 조회 API만 담당.
- ShotCount/PierceCount/Aura Range/HitCount를 기존 스킬·히트박스 요청에 배선. 정적 `dotnet build` 경고·오류 0. Unity Play 검증은 열린 에디터 세션을 방해하지 않아 대기.

---

## 2026-07-02 · 하네스 초기 구성
- 4단계: — (하네스 구성만, 게임플레이 코드 없음 → 검증 미실행)
- 개선점: Agent 4 + Skill 2 + hooks + CLAUDE.md 포인터 구성 완료. 코드 배치 위치는 오픈 퀘스천(orchestrator 게이트 A).
- 다음 입력: 플레이어 이동(Topdown3D) 첫 구현 → 4단계 게이트 실검증

## 2026-07-05 · 캐릭터 이동(CharacterMover) 구현
- 4단계: ①컴파일 ✅ ②Play ✅ ③기능 ✅ (④사용자 대기). 하네스 첫 게임플레이 실검증 완료.
- 배치(게이트 A): 전부 00_CommonFramework. 신규 MoveStats/CharacterMover/IMoveDirectionProvider/CameraRelativeMoveModule/ActorView/ICameraBasisProvider, PlayerMover·PlayerView 삭제.
- 리뷰: blocker 0, M1(필드 선언순서)·m1(PlanarRight 방어) 반영.
- 하네스 교훈: (1) `enabledMcpjsonServers` 이름이 `.mcp.json` 서버키와 달라(`unity-mcp`≠`UnityMCP`) MCP 도구가 세션에 로드 안 됨 → `UnityMCP`로 수정. (2) 스크립트 삭제 시 프리팹 missing script가 남아 프리팹 저장이 거부됨 → `RemoveMonoBehavioursWithMissingScript` 선행 필요. (3) autoInjectGameObjects에 PlayerContext 누락 시 주입 안 돼 이동 불가 → 보정.
- 다음 입력: game-plan 순서 2번 — 적 스폰 + 추격 AI(ChaseDirectionProvider로 IMoveDirectionProvider 교체, CharacterMover/ActorView 재사용).

## 2026-07-05 · PoolManager(오브젝트 풀링 인프라) 구현
- 4단계: ①컴파일 ✅ ②런타임 ✅ ③기능(라이브 스모크) ✅ (④사용자·실적용 게이트 B 대기). PRD `docs/requirements/pool-manager.md` 구현.
- 배치(게이트 A): PRD가 `00_CommonFramework` 확정 → 게이트 A skip. 신규 `Manager/PoolManager/{IPoolHandle,IPoolService,PoolModule,PoolManager}.cs`, `GameSceneScope`에 `Register<PoolManager>(Singleton).As<IPoolService>()` 1줄.
- 구조: `PoolManager`(Manager)→`PoolModule<T>`(Module, `UnityEngine.Pool.ObjectPool<T>` 래핑). string키 `Dictionary<string,object>` 박싱. create는 `resolver.Instantiate(prefab)`로 DI 유지. 중복 Register 무시, 미등록 KeyNotFound, 타입불일치 InvalidCast.
- 스모크(execute_code, codedom): Get→Release→re-Get 동일 InstanceID 재사용 확인, activeSelf 토글 OK, 예외 처리 OK.
- 리뷰: blocker 0 / major 0 / minor 2(비차단). `PoolModule`의 Unity API 사용은 PRD 명시 예외.
- 하네스 교훈: (1) MCP 도구는 세션 시작 시점에만 로드 — 브리지를 세션 중간에 켜도 도구 미로딩, 세션 재시작 필요. (2) execute_code는 Roslyn 미설치 → codedom(C#6) 폴백, using 금지·확장메서드(VContainer Register/Resolve) 미해석 → FQN+비제네릭 API 또는 대상 직접 생성으로 테스트. (3) 에디트 모드 스모크에서 `Object.Destroy` 경고는 코드결함 아님(런타임 정확).
- 다음 입력: 여전히 game-plan 순서 2번(적 스폰 + 추격 AI). 스폰 시 PoolManager 실적용(Enemy 프리팹, 게이트 B) 가능.

## 2026-07-06 · EnemySpawner & WaveModule 구현
- 4단계: ①컴파일 ✅ ②런타임 ✅ ③기능 ✅ (④사용자 대기). PRD `docs/requirements/enemy-spawner.md` 구현. PoolManager 실적용 달성.
- 배치(게이트 A): PRD가 `00_CommonFramework` 확정 → skip. 신규 `Manager/EnemySpawner/{SpawnRequest,WaveDataSO(+WaveEntry),WaveModule,EnemySpawnManager}.cs`.
- 구조: `EnemySpawnManager`(EntryPoint: IAsyncStartable+ITickable) → `WaveModule`(순수 C#, 시간→SpawnRequest, `_nextIndex`로 1회 소비, 재사용 버퍼). StartAsync에서 RequiredKeys를 `IAssetService.LoadAsync<GameObject>`로 프리로드→`IPoolService.Register(key, EnemyContext)`. Tick 자체타이머로 `GetHandle<EnemyContext>(key).Get()` 스폰. WaveEntry를 독립 struct로 분리해 WaveModule의 SO 비참조(순수성) 유지.
- DI 편차: PRD는 ProjectLifetimeScope였으나 IPoolService가 GameSceneScope에만 있어 **GameSceneScope에 RegisterInstance(_waveData)+RegisterEntryPoint<EnemySpawnManager>()** 로 확정.
- 게이트 B(승인): Enemy_Basic.prefab(Capsule+EnemyContext, Addressable `Enemy_Basic`), WaveData_Test.asset(웨이브 2건), GameScene의 GameSceneScope._waveData 할당. 검증: spawnTime 도달 시 5개(3@(0,0,5)+2@(3,0,0)) 정확, 중복 없음.
- 리뷰: blocker 0 / major 1(M1: Tick GetHandle null 미방어 NRE → `if(null==handle)continue` 추가) / minor 4(m1 필드순서 반영, m2~m4 비차단 보류).
- 하네스 교훈: (1) **Addressables는 그룹 YAML 직접 편집만으론 런타임 미해소** — 설정 캐시/카탈로그 미갱신으로 LoadAsync가 조용히 대기, async EntryPoint가 예외를 삼켜 무증상 스폰0. `AddressableAssetSettings.CreateOrMoveEntry`(정식 API)로 등록 후 `LoadResourceLocationsAsync`로 해소(=1) 검증해야 함. (2) unity-ai-operator 툴셋에 execute_code/execute_menu_item이 없어 Addressables 정식 등록·PlayModeScript 설정 불가 → orchestrator가 직접 execute_code로 처리. (3) Play Mode Script는 이미 index0(Use Asset Database)였음 — 문제는 빌더가 아니라 엔트리 캐시 미반영이었음.

## 2026-07-06 · EnemySpawner & WaveModule (요약)
- `docs/requirements/enemy-spawner.md` 구현 완주(게이트4 육안만 대기). 신규 `Manager/EnemySpawner/`: SpawnRequest·WaveDataSO(+WaveEntry struct)·WaveModule·EnemySpawnManager. GameSceneScope에 RegisterInstance(_waveData)+RegisterEntryPoint. 게이트①②③ ✅(스폰5=3+2 정확). 리뷰 blocker0/major1(Tick GetHandle null방어)/minor4. (상세 위 항목)

## 2026-07-07 · AIBase FSM 공통 인프라 (요약)
- `docs/requirements/ai-base.md` 구현. 신규 `00_CommonFramework/00_Scripts/AI/`(namespace `O2un.AI`, 전부 순수 C#): IState·ITransitionCondition·Transition(null가드)·StateMachine(Dict<IState,List<Transition>> 그래프, 등록순=우선순위, 프레임당 1전이 후 Tick)·BaseEnemyAI(abstract, protected AddTransition/SetInitial, Actor엔 Tick(dt)만 노출·역참조 없음). 게이트①컴파일 ✅(정적)·Stop hook 위임, ②③④ 순수C+씬무변경으로 해당없음/대기. 리뷰 blocker0/major0/minor3(m2 Transition null가드 반영). 교훈: MCP 미노출 세션 주의, PRD 경로표기(`Gameplay/AI/`)가 실관례(`00_Scripts/`)와 상시 불일치.
- 다음 입력: `docs/requirements/npc-actor.md`(소비측 — SeekPlayerState/Blackboard/ChaseAIProfile).

## 2026-07-07 · NpcActor 추격 AI(소비측) 구현
- 4단계: ①컴파일 ✅ ②런타임 ✅ ③기능(라이브 스모크) ✅ ④사용자 대기. 슬라임 5마리가 Player로 수렴(거리 3.33→1.33) — Player 등록 배선 검증.
- 배치(게이트 A): 10_ProjectA/01_Scripts/Actor/Npc (Project A 전용). 신규 EnemyBlackboard/SeekPlayerState/ChaseEnemyAI/ChaseAIProfile(SO)/NpcActor/NpcContext.
- 사용자 지시 반영: PlayerActor를 IActor로 만들어 PlayerContext에서 Register(Player 등록 공백 해소). 몬스터=기존 Slime(Enemy_Slime). ChaseAIProfile은 NpcContext 인스펙터 주입(GameSceneScope 배선 원복).
- 3차 지시: 풀 Get/Release 등록·해제는 IPoolable(OnSpawned/OnDespawned) 사용(OnEnable 금지) → EnemyContext.SetLifecycleCallbacks. 접지=y 0 고정(중력 폐기).
- 교훈: 신규 .cs는 refresh scope=all force로 임포트해야 CS0246 회피. 중첩 struct set_property는 도트경로(_stats._moveSpeed). 에디터 비포커스 시 runInBackground=true로 프레임 진행.
- 후속(비차단): FSM/NpcActor 유닛테스트 미작성. Unregister(OnDespawned) 런타임 미실측. 군집 분리 없음.

## 2026-07-08 · AttackSystem(자동공격·히트박스·피격·처치) 구현 (요약)
- `docs/requirements/attack-system.md` 구현. 골격 공통(`00_CommonFramework/00_Scripts/Combat/`: Skill·Hitbox·Health), 콘텐츠 프로젝트(`10_ProjectA/.../Combat/`: Melee/Projectile/Aura 스킬+SO, EnemyHealth, MonsterDataSO). PlayerActor/Context·NpcActor/Context·GameSceneScope 배선. 게이트①②③✅(정성)·④대기. 리뷰 blocker0/major1(설계문서 §3.1 내부모순)/minor8. 런타임 버그 2건(AttackHitboxView 트리거 NRE·PoolModule teardown MissingRef) 수정. 교훈: 풀 재사용 View의 released 인스턴스 트리거 경합→핵심참조 null가드+Collider.enabled 토글, 풀 teardown Unity-null 가드, operator execute_code 미노출로 정량게이트 한계.

## 2026-07-08 · 스폰 시스템 확장(도넛 랜덤 + 정규분포 시간분산) + 풀 체력 리셋 버그 (요약)
- 배치(게이트A): 00_CommonFramework(스폰 시스템이 이미 공통). R1(체력리셋 버그): `NpcContext.Build()`의 `GetComponentInParent<EnemyContext>()`가 비활성 오브젝트를 못 찾아 사망개체가 풀반납도 못함 → `(true)` includeInactive로 수정. R2(도넛스폰): `WaveEntry`에 SpawnPlacement(Fixed/PlayerRadius)+Min/MaxRadius, 순수 `AnnulusSampler`(면적균등). R3(정규분포 시간분산): `WaveEntry`에 SpawnTiming(Burst/NormalSpread), `WaveModule` 타임라인 사전전개, 순수 `GaussianSampler`(Box-Muller).
- 게이트①②③✅(정량 execute_code 실측: R1 반납/재사용 정상화, R2 반경3~7 전량, R3 [10,20] 전량+중앙밀집). ④대기.
- 게이트B 후속버그: CharacterController 활성 상태에서 `transform.position` 직접 대입 시 다음 `Controller.Move()`가 원점으로 스냅백 → `EnemySpawnManager.Teleport`(CC enabled 토글 후 대입)로 수정, 재검증(원점이탈 0/11마리) 완료.
- 교훈: 풀링 대상 컴포넌트 조회는 `GetComponentInParent/Children(true)` 필수(비활성 스킵 회피). CC 소유 오브젝트 스폰 위치는 CC를 껐다 켜야 내부 위치 동기화.

## 2026-07-09 · ItemDropSystem(PRD 1 · 경험치 아이템 드랍) 구현 (요약)
- `docs/requirements/item-drop-system.md`(4 PRD 분해 중 1건) 구현, 커밋 `4a6c79e`("경험치 아이템 드랍 추가"). 배치 10_ProjectA 전용.
- 신규: `ItemActor`(순수, Amount+OnPicked)·`ItemView`(IPoolable, 트리거→Actor 위임)·`ItemDropContext`(IAsyncStartable, kill구독→스폰→OnPicked구독→하류계약 발행+Release)·`IEnemyKillEvent`/`EnemyKillEvent`(신규 kill 이벤트, `NpcContext.OnDeath()`에서 위치+exp 발행)·`IExpGainedPublisher`/`IExpGainedSource`/`ExpGainedChannel`(PRD2로 넘기는 하류 계약, Subject 기반 단방향). `MonsterDataSO`에 exp 필드 신설.
- 이 실행의 상세 게이트·리뷰 로그는 improvement-log.md 갱신 시점(다음 실행)에 유실됨 — 커밋 메시지·산출 인터페이스로만 추적 가능. 하류 계약(`IExpGainedSource.OnGained`)은 PRD2(ExperienceSystem)가 그대로 재사용.

## 2026-07-09 · ExperienceSystem(PRD 2 · 경험치 누적·레벨업) 구현 (요약)
- `docs/requirements/experience-system.md`(4 PRD 분해 중 2건) 구현, 커밋 `a88ab27`. 배치 `10_ProjectA/01_Scripts/Progression/Experience`(namespace `O2un.Progression`).
- 신규: `ExperienceModule`(순수 C#, `IExperienceReader`+`IExperienceWriter`+`IDisposable`, `Gain(amount)` while루프 이월+레벨마다 `LevelUpEvent` 발행)·`LevelUpEvent`(`NewLevel`만)·`ExperienceDataSO`(AnimationCurve)·`ExperienceGainContext`(PRD엔 없던 배선 전용 IInitializable, `IExpGainedSource.OnGained` 구독).
- 게이트①②③✅(정량 실측: Gain(200)→5단계 동시 상승, 이월 55, 이벤트 5회 확인). ④대기.
- 리뷰 blocker0/major1(Reactive 필드 순서 위반, 반영)/minor2(비차단).
- 교훈: AnimationCurve처럼 값 타입은 Module에 주입해도 Unity 비의존 원칙 위반 아님(GameObject/Component/Time급 라이브 상태 결합 여부가 실질 기준). PRD 미명시 배선 Context는 게이트A 승인 대상에 명시 필요.
- 다음 입력: PRD 3 `LevelUpSelection`, `IExperienceReader.OnLevelUp` 재사용.

## 2026-07-09 · LevelUpSelection(PRD 3 · 경험치 루프 마지막) 구현
- 4단계: ①②③✅(정량 실측: 단일 레벨업 정지+후보3개+선택적용, 다중 레벨업 8→0 큐잉 정상 소진). ④사용자 확인 완료.
- 배치: 10_ProjectA. 공통 확장(게이트A 승인): `ISkillDefinition`/`SkillDefinitionSO`에 SkillId/Level, `SkillModule`을 `List<SkillSlot>`로 교체+`AcquireOrUpgrade` 신설, `IPlayerSkillReceiver` 신설.
- 신규: `LevelUpSkillPoolSO`/`LevelUpSelectionViewModel`/`LevelUpSelectionView`/`LevelUpSelectionContext`(`O2un.Progression`).
- 리뷰 blocker0/major0/minor2(둘 다 반영: 후보풀 null방어, 버튼-라벨 길이 가드).
- 교훈: (1) 씬 MonoBehaviour Context는 GameObject 배치만으론 부족, `autoInjectGameObjects` 등록 누락 시 조용히 아무 일도 안 함(예외·로그 없음) — 기능 실측 필수. (2) 씬을 에디터가 열어둔 채 텍스트 직접 편집 위험, SerializedObject API+EditorSceneManager.SaveScene가 안전. (3) unity-ai-operator 세션에 execute_code 없을 때 orchestrator가 직접 MCP 툴로 보완 가능.
- 다음 입력: game-plan 다음 우선순위(게임오버 등) 확인 필요. 별건: `AttackHitboxView.cs:71` Play종료 시 NRE(무관 기존 버그, 별도 착수 권장).

## 2026-07-10 · enemy-ai-content → 3PRD 분해, 1번째(ai-profile-state-so) 구현
- 원 요청 `enemy-ai-content`(일반추적+빠른돌진형+중장갑근접형)를 scope-gate로 분리 필요 판정(기준 5개 걸림) → `ai-profile-state-so`(선행)/`dash-enemy`/`armored-melee-enemy` 3개 PRD로 분해, 사용자 승인.
- 1번째 `ai-profile-state-so` 구현: `ChaseAIProfile.Build()`의 `new SeekPlayerState(...)` 하드코딩을 제거하고 `EnemyStateSO`(abstract SO 팩토리)/`SeekPlayerStateSO`(구체 팩토리)로 분리. `ChaseAIProfile`은 `_seekState`(EnemyStateSO) SerializeField로 조립.
- 배치: 10_ProjectA(EnemyBlackboard/CharacterMover가 이미 프로젝트 타입이라 유일 선택지).
- 4단계: ①컴파일✅ ②Play콘솔✅ ③기능(추적 몹이 4.78→1.04 거리로 정상 추적, 회귀없음)✅ ④사용자 대기.
- 리뷰 blocker0/major0/minor2(비차단: null방어 제안, CreateAssetMenu 경로 스타일).
- 하네스 이슈: `manage_asset(create)`가 임의 ScriptableObject 타입 미지원(Folder/Material/PhysicsMaterial만) → orchestrator가 `execute_code`로 `AssetDatabase.CreateAsset` 직접 호출해 대신 생성. execute_code에 `using` 문 불가(FQN 필요), HTML엔티티(`&lt;`) 오삽입 주의.
- TeamCreate/TaskCreate MCP 도구가 이번 환경에 없음 → orchestrator가 Agent 툴로 unity-architect/gameplay-engineer/unity-ai-operator/code-reviewer를 순차 스폰해 동일 파이프라인 유지.
- 다음 입력: `dash-enemy`(추적→준비→돌진→회복) 또는 `armored-melee-enemy`(느린추적→근접공격) 착수 — 이 PRD가 만든 `EnemyStateSO` 팩토리 패턴(§0-1)과 §5-2 확장 형태(전이 조건도 SO화, StateEntry/TransitionEntry 그래프)를 그대로 따를 것. 열린 질문: 다중상태 그래프 조립을 전용 서브클래스 vs 범용 GraphEnemyAI 중 dash-enemy 설계 시점에 확정 필요.

## 2026-07-10 · dash-enemy(추적→준비→돌진→회복) 구현
- `ai-profile-state-so`가 확립한 EnemyStateSO 팩토리 패턴 위에 빠른 돌진형 몬스터 구현. 배치: 10_ProjectA.
- 핵심 설계: `IStateProgress`(상태 자체 완료여부) 인터페이스로 조건 SO 중복 저장 회피 — 준비→돌진/돌진→회복/회복→추적 3개 전이는 SO 없이 `StateCompleteCondition`(범용) 코드 조립, 추적→준비 1개만 `DistanceWithinRangeConditionSO`. 전용 `DashEnemyAI : BaseEnemyAI`가 그래프 조립(범용 GraphEnemyAI는 채택 안 함, 재사용처 아직 1개뿐).
- 열린 질문(게이트A 승인): `EnemyAIProfileSO` 추상 SO 신설 → `ChaseAIProfile`이 상속하도록 1줄 변경, `NpcContext._profile` 타입 일반화(A안 채택, DashNpcContext 복제 B안 기각). `IStateProgress`/`StateCompleteCondition`은 10_ProjectA에 배치(YAGNI, 후속 재사용 시 공통 승격 검토).
- 충돌 무시: `CharacterController.detectCollisions` 토글. Module 규칙 준수 위해 `DashState`(순수C#)→`CharacterMover.CollisionEnabled`(의도)→`NpcActor.ApplyMovement()`→`ActorView.SetCollisionDetection()`(실행) 경계로 분리.
- 4단계: ①②③✅(로직레벨 결정론 검증 + Play모드 실물 이동 확인) ④사용자 대기.
- 리뷰 blocker0/major0(1건 추가검증으로 해소)/minor3(전부 비차단).
- **사전 버그 발견·수정**: SlimePBR/DashEnemyPBR 프리팹의 `CharacterController.height=0`(퇴화 콜라이더) — 저속 Slime은 안 티났지만 고속 Dash가 사실상 못 움직임. 사용자 승인 후 height=1.8로 수정, Slime 이동 품질도 부수 개선.
- 하네스 교훈: (1) Play 세션 중 `FindObjectsByType`로 "첫 active 개체"를 찾는 테스트 방식은 웨이브 스포너가 만든 다른 몬스터와 뒤섞일 위험 — GameObject 이름 태깅으로 정확히 추적할 것. (2) execute_code에서 R3 `Observable.Subscribe(lambda)`는 codedom이 델리게이트 변환 실패로 컴파일 안 됨 — 대신 동기 프로퍼티(`IsDead` 등)로 우회 확인. (3) Play 모드가 스크립트 에러 없이 조용히 중간에 꺼지는 경우가 있음(에디터/MCP 연결 이슈로 추정) — Time.time이 멈췄거나 `Application.isPlaying=false`인지 항상 재확인. (4) `manage_asset(create)`가 임의 SO를 지원 안 하는 문제는 이번에도 재현 — execute_code로 직접 생성 패턴 계속 유효. (5) 로직 레벨(순수 C# 직접 인스턴스화) 검증이 실물 Play 모드보다 훨씬 결정론적이고 빠르게 FSM 정확성을 증명함 — 환경 이슈(콜라이더 등)와 로직 정확성을 분리해서 검증하는 전략이 유효했음.
- 다음 입력: `armored-melee-enemy`(느린추적→근접공격) 남음 — `dash-enemy`와 병렬 가능, 동일하게 `EnemyAIProfileSO`/`EnemyStateSO` 패턴 재사용.

## 2026-07-11 · armored-melee-enemy(느린 추적→근접 공격) 구현
- PRD: `docs/requirements/armored-melee-enemy.md`. 배치: `10_ProjectA`.
- 구현: `ArmoredMeleeEnemyAI`, `MeleeAttackState(SO)`, 매 틱 거리 이탈 조건, TurtleShell 기반 `ArmoredMeleeEnemyPBR` 프리팹, 중장갑 몬스터 데이터와 Addressable `Enemy_ArmoredMelee` 등록.
- 공격: 적 전용 `EnemyMeleeSwingSkillSO`와 `NearestPlayerStrategy`를 추가해 적끼리 공격하지 않고 플레이어를 타깃. 공격 중 0.5초 정지, 공격 종료 후 사거리 이탈 시에만 추적 복귀.
- 히트박스: 플레이어 Transform 기준 앞쪽 로컬 오프셋과 상대 회전을 유지하며 이동·회전 추적.
- 프리팹: 슬라임 루트 Animator 제거, 중첩된 TurtleShell Animator 사용. 스폰 시 CharacterController 위치·회전 초기화.
- 핵심 결함과 수정: TurtleShell 외형만 교체하고 기존 슬라임 루트 Animator를 제거하지 않아 잘못된 애니메이션이 재생됨 → 루트 Animator 제거, 중첩 TurtleShell Animator 사용.
- 핵심 결함과 수정: 공격 스킬 데이터가 중장갑 몬스터 데이터에 올바르게 연결·검증되지 않았고, 기존 `NearestEnemyStrategy`를 재사용해 적을 타깃할 수 있었음 → `EnemyMeleeSwingSkillSO`/`NearestPlayerStrategy`를 추가하고 `EnemyMeleeSwingSkill_Armored.asset`을 연결.
- 핵심 결함과 수정: FSM 사거리와 실제 히트박스 사거리가 어긋나고 공격 종료 전에 추적 상태로 복귀함 → 사거리 재조정, 공격 지속시간 0.5초 도입, 공격 종료 및 사거리 이탈 이후에만 추적 복귀.
- 핵심 결함과 수정: 소유자 추적을 추가하는 과정에서 히트박스가 플레이어 원점으로 이동하고 앞쪽 오프셋을 잃음 → 소유자 기준 로컬 위치·회전 오프셋을 저장해 앞쪽에서 추적하도록 수정.
- 검증: Unity 에셋 타입 로드 및 콘솔 컴파일 오류 0 확인. Play 기능 검증은 MCP 세션 중단으로 완주하지 못함.
- 하네스 기록 누락은 실행 관리상의 별도 문제이며, 이번 기능 결함의 원인으로 분류하지 않는다.
- 커밋: `a64e229 feat: add armored melee enemy`.


---

# [ARCHIVED] Skill Upgrade Content 설계 (2026-07-11)
(audio-system 실행으로 대체됨. 상세는 chain-log.md 참조)

## [축약] 2026-07-11 · skill-upgrade-content
- LevelUpSelectionModule 후보 산출을 단계별 카드셋 기반으로 확장. LevelUpSkillCardSetSO 4개 + 카드 36장.
- 부모 카드 ID 기반 분기 필터링. 투사체 다발/관통·오라 반경·근접 다중타격 배선.
- 컴파일 0에러 확인, Play·사용자확인 대기였음.

## [축약 archive] 2026-07-12 · audio-system (직전 improvement-log)
- BGM 1채널 + SFX PlayOneShot 최소 오디오. GameAudioBinder가 처치·레벨업·경험치·사망·피격 구독, Playing 전이 시 bgm/battle 재생.
- 미완: SFX 클립 5개 Addressables 미등록. Gate4는 UI Start 수동 확인 필요.
