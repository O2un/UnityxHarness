# 체인 로그

이 프로젝트에서 하네스로 진행한 작업 이력의 축약본. 한 실행이 끝날 때 improvement-log.md의 핵심만 요약해 누적한다.
Phase 0의 기본 입력은 improvement-log.md이며, 이 파일은 과거 내역 확인이 필요할 때만 참조한다.

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
