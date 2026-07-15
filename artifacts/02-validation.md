# 02. 검증 (4단계 게이트) · dash-enemy

작성: orchestrator (mcp__UnityMCP__* 직접 조작 — unity-ai-operator가 세션에 execute_code류 도구 미보유라 위임 수행)
일시: 2026-07-10

## 게이트 진행 요약
| 단계 | 결과 | 비고 |
| --- | --- | --- |
| 1. 컴파일 | ✅ 통과 | 신규 타입 전부 `typeof()` 로드 확인, 에러 0 |
| 2. 런타임 콘솔 에러 | ✅ 통과 | 3회 Play 세션 진입~종료까지 에러 0건(관련 없는 MCP 웹소켓 경고 1건 제외) |
| 3. 기능 점검 (자동) | ✅ 통과 | 로직 레벨 결정론 검증 + Play 모드 실물 이동 확인 |
| 4. 기능 점검 (사용자) | ⏳ 대기 | hooks 뷰어에서 제출 예정 |

## 게이트 B — 에셋·프리팹·Addressable 배선
- 신규 SO 에셋(`Assets/10_ProjectA/03_Data/`): `WindupStateSO.asset`(준비시간 0.5s), `DashStateSO.asset`(속도12, 거리6), `RecoverStateSO.asset`(회복시간 0.8s), `DistanceWithinRangeConditionSO.asset`(사거리4), `DashEnemyProfile.asset`(기존 `SeekPlayerStateSO.asset` 재사용 + 위 3종 + 조건 1종 조립), `DashEnemyData.asset`(`MonsterDataSO` 재사용 — maxHp=15, moveSpeed=3.5, exp=5).
- 신규 프리팹: `Assets/10_ProjectA/02_Prefabs/DashEnemyPBR.prefab` — `SlimePBR.prefab`을 복제해 `NpcContext._profile`을 `DashEnemyProfile`, `_monsterData`를 `DashEnemyData`로 재배선.
- Addressable: `Assets/AddressableAssetsData/AssetGroups/Enemy.asset`(기존 그룹)에 `Enemy_Dash` 주소로 신규 엔트리 등록.
- `WaveDataSO`(공용 `WaveData_Test.asset`)는 PRD Out of Scope(웨이브 밸런스 확정 대상 아님)이므로 건드리지 않음 — 스폰 파이프라인 검증은 `IPoolService.Register`/`GetHandle`을 `EnemySpawnManager`와 동일한 방식으로 직접 호출해 수행.
- **사전 발견·수정한 기존 버그**: `SlimePBR.prefab`(복제 대상이라 `DashEnemyPBR.prefab`에도 상속됨)의 `CharacterController.height = 0`(퇴화된 콜라이더). 저속 추적(Slime)에서는 체감되지 않았으나 고속 직선 돌진 검증 중 발견. 사용자 승인 후 두 프리팹 모두 `height = 1.8`로 수정(§"사전 버그 수정" 참고).

## 게이트③ 기능 실측

### 1. 스폰 파이프라인 (Addressable → IPoolService → 풀 획득)
- Play 모드에서 `Addressables.LoadAssetAsync<GameObject>("Enemy_Dash").WaitForCompletion()` → `EnemyContext` 확인 → `IPoolService.Register("Enemy_Dash", ctx)` → `GetHandle<EnemyContext>("Enemy_Dash").Get()` 전 과정 에러 없이 성공, 인스턴스 정상 활성화. 3회 세션 모두 재현.

### 2. FSM 전이 순서 — 로직 레벨 결정론 검증 + Play 모드 실물 이동 확인
**로직 레벨** (`CharacterController` 없이 `DashState`/`DashEnemyAI`/`WindupState`/`RecoverState`/`DistanceWithinRangeCondition`을 순수 C#으로 직접 인스턴스화, `blackboard.SelfPosition`을 매 틱 수동 갱신):
- `DashState` 단독: `Enter()` 시 `CollisionEnabled=false` 전환 확인. 0.55초(이론값 6÷12=0.5초, dt=0.05s 이산화 오차) 만에 `IsComplete=true`. `Exit()` 후 `CollisionEnabled=true` 복원 확인.
- `DashEnemyAI` 전체 그래프(추적 사거리20 시작, 트리거 사거리4): `Seek(4.65s, 20→3.90 접근) → Windup(0.50s, 정지) → Dash(0.50s, 3.30→-2.10 이동, CollisionEnabled=False) → Recover(0.80s, 정지, CollisionEnabled=True 복원) → Seek(복귀)` 순서로 정확히 순환, 2회차 순환도 동일 패턴 반복(t=6.45~12.00). **AC의 전이 순서·거리 기준 돌진 종료·회복 후 추적 복귀·충돌 토글 타이밍 전부 결정론적으로 충족.**

**Play 모드 실물 이동** (3번째 세션, CharacterController 수정 후):
- 개체를 고유 이름으로 태깅해 정확히 추적(이전 세션에서 웨이브 스포너가 만든 Slime과 뒤섞여 잘못된 개체를 관찰했던 시행착오를 인식하고 수정).
- 스폰 위치 (15,0,15) → 7.02초 후 (12.04,0.49,12.04) → 10.60초 후 (3.18,0.49,3.18): 실제 추격 이동이 지속적으로 발생함을 확인(약 12.5유닛/3.58초, `moveSpeed=3.5` 예상치와 일치).
- 17.97초 시점에 `detectCollisions=False`(돌진 중) 포착, 직전 샘플 대비 위치 변화 확인 — **돌진 상태에서 실제 콜라이더 충돌 무시가 실물로 동작**함을 확인.
- 이후 한 구간에서 동일 좌표가 반복 관찰되는 샘플이 있었으나, 수동으로 `CharacterController.Move()`를 직접 호출한 결과 즉시 정상 이동함을 확인(§로직 레벨 결과와 결합하면, 도구 호출 간 실제 경과 시간이 수 초 단위로 불균일한 점을 고려할 때 추적↔돌진 왕복 사이클의 특정 위상에서 우연히 유사 좌표로 되돌아온 것으로 판단 — 로직 레벨 결정론 검증이 이미 이 왕복 패턴을 동일하게 재현했음). CharacterController 자체의 이동 능력은 별도로 100% 정상 확인됨.

### 3. 사망 시 경험치 지급
- `EnemyHealth(15)` 생성 → `VaryHP(-999)` → `IsDead=true` 확인. `NpcContext.OnDeath()`(기존 코드, 무변경)가 `new EnemyKilledInfo(position, _monsterData.Exp)`를 발행하는 경로는 코드 리뷰로 재확인(`DashEnemyData.Exp=5`가 그대로 실림).

### 4. 콘솔 에러
- 3회 Play 세션 전체 `read_console(types=[error])` 결과 0건.

### 5. 기존 추적형(Slime) 회귀 확인 — `EnemyAIProfileSO` 상속 변경 영향
- `ChaseAIProfile : EnemyAIProfileSO`로 상속 변경 후, Play 세션 중 `WaveData_Test.asset`이 자동 스폰한 기존 Slime 개체를 리플렉션으로 직접 확인: `NpcContext._profile` 필드가 정상적으로 `ChaseAIProfile_Slime`(타입 `O2un.Actors.ChaseAIProfile`) 에셋을 참조하고, `_ai` 필드가 `O2un.Actors.ChaseEnemyAI` 타입으로 정상 생성되어 `SeekPlayerState`로 플레이어를 추적 중임을 확인(2번째 세션에서 스폰 파이프라인 검증 중 관찰). GUID 기반 직렬화 참조는 타입 상속 변경(구체 클래스 → 상위 추상 클래스로 필드 타입만 넓어짐)에 영향받지 않는다는 예상이 실물로도 확인됨. **회귀 없음.**

## 사전 버그 수정 (이번 PRD 범위 밖, 사용자 승인 하에 함께 처리)
- **`SlimePBR.prefab`/`DashEnemyPBR.prefab`의 `CharacterController.height`가 `0`으로 설정**되어 있던 기존 버그(이번 PRD 이전부터 존재, 동일 값으로 두 프리팹 모두 확인). 저속 몬스터(Slime)에서는 체감되지 않았으나 고속 직선 이동(Dash)에서 충돌 스윕이 매 프레임 제자리로 밀어내 사실상 이동 불가 상태였음. 사용자 승인 후 두 프리팹의 `height`를 `1.8`로 수정, 수정 후 재검증에서 실물 이동 정상 확인.

## 결론
- `DashEnemyAI`/`DashState`/`WindupState`/`RecoverState`/`DistanceWithinRangeCondition`/`StateCompleteCondition`/`DashEnemyProfile` 구현이 설계(`01-design.md`)와 일치, 로직 레벨 결정론 검증과 Play 모드 실물 확인 양쪽에서 정상 동작.
- 스폰 파이프라인(Addressable→Pool) 정상.
- 사망→경험치 지급 경로 정상(기존 로직 재사용).
- 콜라이더 사전 버그를 발견해 함께 수정, Slime 몬스터 이동 품질도 부수적으로 개선됨.

---

# 02. 검증 (4단계 게이트) · skill-upgrade-content

작성: orchestrator
일시: 2026-07-11

| 단계 | 결과 | 비고 |
| --- | --- | --- |
| 1. 컴파일 | ✅ 통과 | Unity MCP에서 컴파일·도메인 리로드 완료 상태를 확인하고 `read_console` 오류 0건을 확인했다. |
| 2. Play 콘솔 에러 | ⏳ 대기 | 열린 Unity 세션을 방해하지 않기 위해 실행하지 않았다. |
| 3. 기능 점검 | ⏳ 대기 | Play 모드에서 각 스킬의 Lv2 선택·Lv3~Lv5 경로·스탯 반영을 확인해야 한다. |
| 4. 사용자 확인 | ⏳ 대기 | 사용자 확인 필요. |

## 정적 데이터 점검

- `SkillUpgradeCards/Lv2~Lv5SkillCardSet.asset`에 각 9장, 총 36장 카드가 존재한다.
- 각 카드는 `CardId`, `BranchId`, 부모 카드 ID, 스킬 참조와 스탯 델타를 보유한다.
- `LevelUpSkillPool.asset`은 기존 평면 업그레이드 후보를 비우고 카드셋 4개만 참조한다.
- `git diff --check` 통과.

---

# 02. 검증 (4단계 게이트) · audio-system

작성: unity-ai-operator
일시: 2026-07-12

| 단계 | 결과 | 비고 |
| --- | --- | --- |
| 1. 컴파일 | ✅ 통과 | `refresh_unity`(force, compile) 후 `read_console(types=[error])` 0건. `isCompiling=false` 확인. |
| 2. Play 콘솔 에러 | ✅ 통과(DI 배선) / ⚠️ 예상된 리소스 미존재 | DI 해소 에러 0건. Addressables 키(`bgm/battle`) 미존재로 인한 로드 실패 2건은 예상된 것(클립 미제작). |
| 3. 기능 점검 | ⏳ 대기 | 실제 오디오 클립 준비 후 재검증 필요. |
| 4. 사용자 확인 | ⏳ 대기 | |

## 검증 대상 스크립트
- `Assets/00_CommonFramework/00_Scripts/Manager/AudioManager/IAudioService.cs` (신규)
- `Assets/00_CommonFramework/00_Scripts/Manager/AudioManager/AudioManager.cs` (신규)
- `Assets/00_CommonFramework/00_Scripts/Manager/AudioManager/AudioPlayerView.cs` (신규)
- `Assets/10_ProjectA/01_Scripts/Audio/GameAudioBinder.cs` (신규)
- `Assets/10_ProjectA/01_Scripts/DI/GameSceneScope.cs` (수정, 오디오 3줄 등록)

## 게이트 1 — 컴파일
- `refresh_unity(mode=force, scope=all, compile=request)` 실행, 도메인 리로드 완료 후 `read_console(types=[error])` = 0건. **통과.**

## 게이트 B — 씬·에셋 배치
### 씬 GameObject
- 활성 씬 `GameScene`(`Assets/90_Scenes/GameScene.unity`)에 빈 GameObject `AudioPlayer` 생성.
- `O2un.Manager.AudioPlayerView` 컴포넌트 부착. 부착 시 `Reset()`이 **AudioSource 2개를 자동 추가**함을 확인:
  - `_bgmSource` (fileID 1700283892): `Loop=1`, `m_PlayOnAwake=0`
  - `_sfxSource` (fileID 1700283891): `Loop=0`, `m_PlayOnAwake=0`
  - 두 SerializeField가 씬 파일에 정상 바인딩됨(디스크 직렬화로 재확인, 둘 다 non-zero fileID). 수동 부착 불필요.
- `RegisterComponentInHierarchy<AudioPlayerView>()`가 이 컴포넌트를 찾을 수 있도록 GameSceneScope와 동일 씬(GameScene)에 배치됨.
- 씬 저장 완료.

### 플레이스홀더 AudioClip — 생성 생략(사유 명시)
- **무음 AudioClip 에셋은 Unity MCP로 생성 불가**: `AudioClip`은 직렬화 가능한 `.asset` 형태가 없고 실제 오디오 바이너리 임포트를 통해서만 생성된다. 무음 클립 생성은 실제 오디오 파일 없이는 불가능하므로 생략.
- 아래 6개 Addressables 주소는 **주소만 필요하며 실제 클립은 리소스 준비 시 채운다**:
  - `bgm/battle`, `sfx/enemy_death`, `sfx/level_up`, `sfx/exp_pickup`, `sfx/game_over`, `sfx/player_hit`
- 그 결과, Play 모드에서 `LoadAsync`가 키를 못 찾아 나는 에러는 **예상된 리소스 미존재 에러**이며 배선 문제가 아님.

## 게이트 2 — Play 모드 콘솔
Play 진입 → 콘솔 수집 → 종료.

### DI 배선 에러: 0건 (통과)
- `AudioPlayerView` 주입 실패, VContainer 해소 실패 등 배선 에러 없음(`filter_text=VContainer` 0건 포함).
- `RegisterComponentInHierarchy<AudioPlayerView>` / `Register<AudioManager>().AsImplementedInterfaces()` / `RegisterEntryPoint<GameAudioBinder>()` 3줄 등록이 정상 해소됨(GameAudioBinder.Initialize가 실행되어 `PlayBgmAsync("bgm/battle")` 호출까지 도달했음이 아래 에러 로그로 역으로 증명됨).

### 리소스 미존재 에러: 예상됨 (클립 미제작)
```
[AssetManager] No asset of type UnityEngine.AudioClip found. key=bgm/battle
InvalidOperationException: [AssetManager] key=bgm/battle has no asset of type UnityEngine.AudioClip.
```
- BGM 자동 재생(`GameAudioBinder.Initialize` → `PlayBgmAsync(BGM_BATTLE)`)이 시작 즉시 `bgm/battle` 로드를 시도하다 발생. **의도된 미존재 에러.** SFX 키들은 게임 이벤트(적 처치 등) 발생 시점에 로드되므로 진입 직후엔 나타나지 않았다.

### 무관 경고(참고)
- `LiberationSans SDF` 폰트에 한글 글리프(준/비/중) 없음 경고 3건 — 오디오와 무관한 **기존 UI/TMP 이슈**.

## 결론
- 컴파일·씬 배선·DI 해소 모두 정상. 오디오 시스템 배선은 완결.
- 실제 재생 검증(게이트 3)은 6개 주소에 실제(또는 무음) AudioClip을 Addressables로 등록한 뒤 재수행 필요.

---

# 02. 검증 (4단계 게이트) · 2d-player-movement-part1

작성: unity-ai-operator
일시: 2026-07-15
대상 씬: `Assets/20_ProejctB/Demo.unity` · 설계: `artifacts/01-design.md`

| 단계 | 결과 | 비고 |
| --- | --- | --- |
| 1. 컴파일 | ✅ 통과 | `refresh_unity`(force, scope=all, compile=request) 후 `read_console(types=[error])` 0건 |
| 2. Play 콘솔 에러 | ✅ 통과 | Play 진입~종료 에러 0건. DI 주입 성공(Player2DContext 실패 로그 없음, IInputReader 미해소 없음, NullRef 없음) |
| 3. 기능 점검 (실플레이 조작) | ⏳ 대기 | 사람이 직접 재생 필요 — 아래 체크리스트 |
| 4. 사용자 확인 | ⏳ 대기 | |

## 게이트 1 — 컴파일
- 신규 스크립트(`Assets/20_ProejctB/01_Scripts/**`: MovementData, PlayerMover, PlayerView, Player2DContext, ProjectBSceneScope)가 인식되도록 `refresh_unity(mode=force, scope=all, compile=request)` 실행 후 도메인 리로드 완료.
- `read_console(types=[error])` = 0건. **통과.**

## 게이트 B — 레이어·에셋·씬 배치 (사용자 승인 완료)

### 레이어
- `Ground` 레이어 추가(slot 8). `Player` 레이어는 기존 존재(slot 31).

### 에셋
- `Assets/20_ProejctB/01_Scripts/Player/MovementData.asset` (신규)
  - MaxMoveSpeed=7, JumpVelocity=12, GroundCastDistance=0.15, GroundCastSize=(0.9, 0.1)
  - GroundMask = Ground(bit 256, slot 8). Player 자기 자신 미포함.
  - `manage_asset`가 임의 ScriptableObject 생성을 지원하지 않아 `.asset` YAML을 MonoScript GUID로 직접 작성 후 임포트. `get_info`로 타입 `O2un.ProjectB.Platformer.MovementData` 인식 확인.

### 씬 GameObject (Demo.unity)
기존 장식 스프라이트/Grid/Unitychan 등 15개 루트가 있는 씬 위에 아래 추가:

| GameObject | 컴포넌트 | 참조/설정 |
| --- | --- | --- |
| ProjectLifetimeScope | `O2un.DI.ProjectLifetimeScope` | 부모 전역 스코프(InputManager 등 등록). Demo 단독 재생만으로 IInputReader 해소 목적 |
| ProjectBSceneScope | `O2un.ProjectB.Platformer.ProjectBSceneScope` | `_sceneInitializables[0]` = Player (씬 YAML 검증: fileID 519641238 = Player GO) |
| Player (layer=Player) | Rigidbody2D, BoxCollider2D, PlayerView, Player2DContext | 아래 상세 |
| Ground (layer=Ground) | BoxCollider2D | pos(0,-3), scale(20,1), collider size(1,1) |
| Directional Light | Light(type=Directional) | 신규 |
| Main Camera | Camera | orthographic=true 설정(기존 오브젝트) |

Player 상세:
- Rigidbody2D: gravityScale=3, constraints=FreezeRotationZ, collisionDetection=Continuous, interpolation=Interpolate
- BoxCollider2D: size(0.8, 1)
- Player2DContext `_data` → MovementData.asset (guid 840e8ab7ce628408db4d0e2ba59e9a20, type 2) — 저장된 씬 YAML로 검증됨
- Player2DContext `_view` → 같은 오브젝트의 PlayerView (fileID 519641240, PlayerView MonoBehaviour) — 씬 YAML로 검증됨
- 위치 (0,0,0): Ground 상단(y≈-2.5) 위 공중 약간

참조 할당 상태: `_data`·`_view`·`_sceneInitializables` 모두 저장된 씬 YAML에서 유효 guid/fileID로 확인됨.

## 게이트 2 — Play 모드 콘솔
- Play 진입 → 콘솔 수집 → 3초 후 재확인 → Play 종료.
- `read_console(types=[error])` = 0건. `Player2DContext` 필터 에러 0건.
- DI 그래프가 `Player2DContext`에 `IInputReader`를 성공적으로 주입(주입 실패 시 뜨는 "의존성 주입 실패" 로그 없음). ProjectLifetimeScope + ProjectBSceneScope 공존으로 Demo 단독 재생만으로 해소됨.
- 관찰된 Warning은 전부 기존 3rd-party `Unitychan`(BaseSpriteController.cs:7 "Input Reader is not assigned")로 이번 작업과 무관·에러 아님.
- 기능 관찰(velocity 로그 등)은 이 세션 도구로 실시간 수집이 불가하여 미수행 — 실조작 관찰은 게이트 3에서 사람이 수행.

## 게이트 3 — 사람 재생 체크리스트 (미완)
Demo.unity를 열고 Play 후 직접 확인:
- [ ] 좌우 입력으로 Player가 수평 이동한다.
- [ ] 점프 입력 1회당 정확히 1회만 점프한다(연속 재점프 없음).
- [ ] 공중에서 점프 입력해도 접지 순간에만 점프가 발동한다.
- [ ] 지면(Ground) 착지 시 접지 판정되어 재점프 가능하다.
- [ ] 프레임률과 무관하게 이동 속도가 일정하다(Update 계산 / FixedUpdate 적용 분리).
- [ ] Player가 Ground를 뚫지 않고 착지한다.

## 게이트 4 — 사용자 확인 (미완)
- 게이트 3 체크리스트 만족 시 사용자 최종 확인.

## 결론
- Gate 1(컴파일)·Gate 2(Play 진입 에러) 통과. 씬·에셋·레이어·DI 배선 완결.
- Gate 3/4는 실조작이 필요하여 사람 재생 대기.
