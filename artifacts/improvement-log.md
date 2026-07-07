# 개선 기록 (improvement-log)

이 파일은 **가장 최근 실행 1건**만 담는 단일 스냅샷이다. 다음 실행의 Phase 0-A가 그대로 읽어 연속 작업 기준으로 삼는다. 새 실행이 끝나면 통째로 덮어쓴다(이전 내용은 덮어쓰기 전에 chain-log.md로 축약 누적).

---

## 2026-07-06 · EnemySpawner & WaveModule 구현

### 무엇을 했나
- `docs/requirements/enemy-spawner.md`(PRD) 구현. 설계(orchestrator가 01-design 정리) → 게이트 A(PRD 확정 skip) → 구현 → 게이트 1(컴파일) → **게이트 B 사용자 승인** → unity-ai-operator 씬·에셋 → 게이트 2·3 → 리뷰(code-reviewer) → M1·m1 수정까지 완주. 게이트 4(사용자 육안)만 대기.
- 신규(`00_CommonFramework/00_Scripts/Manager/EnemySpawner/`): `SpawnRequest.cs`(readonly struct), `WaveDataSO.cs`(+독립 `WaveEntry` struct, `[CreateAssetMenu]`), `WaveModule.cs`(순수 C#), `EnemySpawnManager.cs`(EntryPoint). 수정: `DI/GameSceneScope.cs`에 `[SerializeField] WaveDataSO _waveData` + `RegisterInstance(_waveData)` + `RegisterEntryPoint<EnemySpawnManager>()`.
- 구조: `EnemySpawnManager`(IAsyncStartable+ITickable) 소유 `WaveModule`. StartAsync가 `WaveModule.RequiredKeys`를 `IAssetService.LoadAsync<GameObject>`로 프리로드→`GetComponent<EnemyContext>()`→`IPoolService.Register(key, ctx)`, 완료 후 `_ready`. Tick이 자체 누적 `_timer`(Time.deltaTime)로 `WaveModule.GetSpawnsAt(timer)` 호출→각 SpawnRequest마다 `GetHandle<EnemyContext>(key).Get()` 후 위치 설정.
- `WaveModule`: 생성자에서 SpawnTime 정렬 + RequiredKeys distinct. `GetSpawnsAt`는 `_nextIndex` 포인터로 단조증가 timer 기준 1회 소비(중복·누락 없음), count를 개별 SpawnRequest로 전개, 재사용 `_buffer`로 매 Tick 할당 회피. **SO 타입 비참조**(WaveEntry 독립 struct)로 순수성 유지.
- 결정: DI 등록은 PRD의 ProjectLifetimeScope가 아니라 **GameSceneScope**(IPoolService가 거기에만 있음; 자식 스코프가 부모 IAssetService까지 봄). 배치는 PRD 확정 00_CommonFramework, 폴더는 기존 관례 `Manager/`.

### 4단계 게이트
- ①컴파일 ✅ (콘솔 에러 0, validate_script standard 에러 0; GetComponent 경고는 오탐 — `if(null==context)` 이미 처리)
- ②런타임 ✅ (Play 진입 콘솔 에러/경고/예외 0)
- ③기능 ✅ (Play 후 `FindObjectsByType<EnemyContext>`=5: (0,0,5)3 + (3,0,0)2 = WaveData와 정확 일치, 시간 경과 후에도 5 유지 → 각 웨이브 1회 소비)
- ④사용자 ⏳ (viewer 피드백 대기)

### 게이트 B (승인 완료)
- `Assets/00_CommonFramework/10_Prefabs/Enemy/Enemy_Basic.prefab`(Capsule+EnemyContext), Addressable 주소 `Enemy_Basic`.
- `.../Enemy/WaveData_Test.asset`(WaveDataSO, 웨이브 2건: {Enemy_Basic,t1,c3,(0,0,5)},{Enemy_Basic,t2,c2,(3,0,0)}).
- `Assets/AddressableAssetsData/AssetGroups/Enemy.asset` 엔트리 등록, `Assets/Scenes/GameScene.unity` GameSceneScope._waveData 할당.

### 리뷰
- blocker 0 / major 1 / minor 4. **M1**(Tick의 `GetHandle` null 미방어 → 프리로드 실패 key 도달 시 NRE) → `if(null==handle)continue` 추가로 수정. **m1**(비-DI 필드 `_ready`/`_timer` 생성자 위) → 생성자 아래로 이동. m2(GetHandle 반복조회)·m3(버퍼 반환 계약)·m4(_waveData null 가드)는 비차단 보류.

### 아쉬웠던 점 / 원인 → 반영
- **Addressables는 그룹 `.asset` YAML 직접 편집만으론 런타임에서 해소되지 않는다.** 설정 내부 캐시/카탈로그가 갱신 안 돼 `LoadAsync`가 조용히 대기 → async EntryPoint(`StartAsync`)가 예외/행을 삼켜 **에러 0인데 스폰 0**의 무증상 실패(게이트 3 1차 FAIL). 정식 API `AddressableAssetSettings.CreateOrMoveEntry(guid, group)`+`entry.address`+`SetDirty`+`SaveAssets`로 등록하고 `LoadResourceLocationsAsync(key, type)`가 1 반환하는지 **사전 해소 검증** 후 Play해야 한다.
- **원인 오진 주의**: 처음엔 Play Mode Script(packed) 의심했으나 실제 index는 이미 0(Use Asset Database)였다. 진짜 원인은 엔트리 캐시 미반영. 다음엔 추정 전에 `LoadResourceLocationsAsync`로 먼저 해소 여부를 찍어볼 것.
- **unity-ai-operator 툴셋 한계**: execute_code/execute_menu_item 미노출 → Addressables 정식 등록·EditorPrefs 설정 불가. orchestrator가 execute_code(deferred 툴 로드)로 직접 처리해 우회. (다음엔 Addressables 정식 등록이 필요한 검증은 operator에 맡기기 전에 execute_code 필요성을 미리 인지.)
- execute_code는 여전히 codedom(C#6) 폴백 — FQN·최상단 using 금지 원칙 유효(이번에도 준수).

### 다음 테스트 (다음 실행 입력)
- **game-plan 순서 2번의 나머지: 적 추격 AI**. 스폰된 EnemyContext에 `ChaseDirectionProvider`(자기→플레이어) + 기존 `CharacterMover`/`ActorView` 재사용으로 추격 이동 부여. EnemySpawnManager는 위치만 세팅하므로 스폰 직후 적의 이동 컴포넌트 초기화(타깃 주입) 경로 설계 필요.
- **후속 연계**: Enemy 사망 시 풀 반납(`EnemyContext.Release`) — 체력·피격 시스템(game-plan 순서 3) 구현 시 연결. 현재 스폰만 담당(반납 Out of Scope).
- 리뷰 m2~m4(핸들 캐싱·버퍼 계약 주석·_waveData null 가드)는 규모 커지면 재검토.

### 하네스 자체 개선 메모
- **무증상 실패 감지**: async IAsyncStartable에서 await가 영영 완료 안 되면 예외 없이 기능만 죽는다. 게이트 3에 "핵심 상태 플래그(_ready 등)가 true가 됐는가"를 로그/검증 포인트로 넣으면 조기 발견 가능.
- Addressables 의존 기능의 게이트 B에는 "정식 API 등록 + LoadResourceLocations 해소 검증"을 체크리스트로 표준화.
