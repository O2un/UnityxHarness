# 개선 기록 (improvement-log)

이 파일은 **가장 최근 실행 1건**만 담는 단일 스냅샷이다. 다음 실행의 Phase 0-A가 그대로 읽어 연속 작업 기준으로 삼는다. 새 실행이 끝나면 통째로 덮어쓴다(이전 내용은 덮어쓰기 전에 chain-log.md로 축약 누적).

---

## 2026-07-07 · NpcActor 추격 AI(소비측) 구현

### 무엇을 했나
- `docs/requirements/npc-actor.md`(PRD) 구현. 선행 의존(`AIBase` FSM, `ActorManager`)이 이미 구현돼 있어 그 위에 "Player 추적" 상태 1개를 얹는 소비측. PRD가 Resolved Decisions 5건으로 사실상 확정본이라 설계 단계 skip → 게이트 A(배치·질문) → 구현 → 게이트 1(컴파일) 통과.
- 신규 6파일 (`10_ProjectA/01_Scripts/Actor/Npc/`, namespace `O2un.Actors`):
  - `EnemyBlackboard.cs` — 순수 C# 값 컨테이너(`SelfPosition`/`TargetPosition`/`HasTarget`). Transform 미참조(Vector3만).
  - `SeekPlayerState.cs`(`IState`) — Enter/Exit 빈함수, Tick에서 blackboard 방향 계산→`dir.y=0`→`mover.SetDirection(dir.normalized)`. `HasTarget==false`면 SetDirection(zero). 자기 전이 없음.
  - `ChaseEnemyAI.cs`(`BaseEnemyAI` 구체 서브클래스) — 생성자에서 `SetInitial(initial)`만. protected 그래프 API를 소유하는 최소 서브클래스(profile이 직접 SetInitial 불가하므로 필요).
  - `ChaseAIProfile.cs`(`ScriptableObject`, `[CreateAssetMenu]`) — `Build(blackboard, mover)`가 SeekPlayerState 생성→`new ChaseEnemyAI(seek)` 반환. 그래프 조립 경로.
  - `NpcActor.cs`(순수 C#, `IActor(Enemy)`+`IDisposable`) — Tick: UpdateTarget(query.Player→blackboard, null이면 HasTarget=false)→ai.Tick→ApplyMovement(mover.Velocity→view.Move/RotateTo). Register/Unregister 자체 소유(중복 안전). Transform=>_view.transform.
  - `NpcContext.cs`(MonoBehaviour) — `[Inject] Construct(ChaseAIProfile, IActorRegistry, IActorQuery)`에서 blackboard·mover·actor 조립+Register. Update→Tick(deltaTime), OnDisable→Unregister, OnDestroy→Dispose. **PlayerContext와 동일 패턴**.
- `GameSceneScope.cs`(공통) 편집: `[SerializeField] ChaseAIProfile _chaseProfile` + `if(null!=_chaseProfile) RegisterInstance(_chaseProfile)`.

### 사용자 승인 (게이트 A + 설계 질문 + 2차 지시)
- **배치**: `10_ProjectA/01_Scripts/Actor/Npc` 확정(PRD대로 Project A 전용, ProjectA 스크립트 관례 `01_Scripts`). 사용자 선택.
- **이동 실행**: PlayerActor와 동일하게 NpcActor가 `ActorView`를 소유해 velocity를 view.Move/RotateTo로 적용. → PRD 생성자 목록에 없던 `ActorView`를 생성자 인자로 추가(승인된 편차). SeekPlayerState는 방향만 산출.
- **MoveStats**: PlayerContext 선례(serialize) 채택 → NpcContext에 `[SerializeField] MoveStats`.
- **2차 지시(사용자)**: (1) `PlayerActor`를 `IActor(Player)`로 만들고 `PlayerContext`에서 `IActorRegistry` 주입·Register/Unregister. (2) 몬스터는 기존 Slime(`SlimePBR`=어드레서블 `Enemy_Slime`) 사용. (3) `ChaseAIProfile`은 `GameSceneScope` DI가 아니라 `NpcContext` 인스펙터(`[SerializeField] _profile`)로 주입 → GameSceneScope 배선 **원복**.

### 4단계 게이트 (전부 통과)
- ①컴파일 ✅ — MCP `refresh_unity(force,all)` 후 `read_console(error)` 0건. (초기 `scripts`-scope 컴파일은 신규 .cs 미임포트로 CS0246 → `scope=all force`로 자산 임포트 후 해소.)
- ②런타임 ✅ — GameScene Play, 콘솔 에러 0. 5마리 스폰(`SlimePBR(Clone)`), DI 주입·NpcContext.Construct 정상.
- ③기능 ✅ — 라이브 스모크(execute_code 2회 샘플): Player 이동에도 슬라임 전원이 Player로 수렴, 거리 3.33→1.33로 감소·군집. **Player 등록 배선 검증됨**(미등록이면 HasTarget=false로 정지했을 것). 자동 유닛테스트는 여전히 미작성(아래).
- ④사용자 ⏳ (viewer 육안 피드백 대기)

### 게이트 B 실행 내역 (완료)
- 에셋: `Assets/10_ProjectA/02_Prefabs/ChaseAIProfile_Slime.asset`(guid 9a735bcf…) 생성.
- 프리팹 `SlimePBR.prefab`: `CharacterController`+`ActorView`+`NpcContext` 추가. `NpcContext._view`→ActorView, `_profile`→위 에셋, `_stats`(MoveSpeed 3/RotationSpeed 720). `ActorView._animator`는 미할당(런타임 `GetComponentInChildren`로 자동 해소).
- 주의: `_stats` 같은 **중첩 struct는 set_property의 nested-dict로 안 먹혀** 0으로 남음 → 도트 경로(`_stats._moveSpeed`)로 재설정해 해결.

### 3차 지시(사용자) 반영 — 완료·재검증
- **(1) 풀 Get/Release로 등록·해제 (OnEnable 금지 → IPoolable 사용)**: `IPoolable`에 `OnSpawned()`/`OnDespawned()` 추가 → `PoolModule.OnGet/OnRelease`에서 `is IPoolable` 캐스팅 호출. `EnemyContext`(common)에 `SetLifecycleCallbacks(onSpawned, onDespawned)` + 두 훅 구현(SetReleaseCallback 패턴 미러). `NpcContext`(project)는 Build에서 `GetComponent<EnemyContext>().SetLifecycleCallbacks(_actor.Register, _actor.Unregister)`만 연결, **OnEnable/OnDisable 제거**. 레이어 준수: project→common 참조만(EnemyContext는 Npc 무참조). 검증: registeredEnemies=5(풀 Get시 IPoolable.OnSpawned→Register 동작 확인).
- **(2) 접지 = 높이 0 고정 (중력 방식 폐기)**: ActorView 중력 변경 **원복**. `NpcActor.ApplyMovement`에서 view.Move 후 `transform.position.y = 0` 고정. 검증: 슬라임 전원 y=0.000.

### 남은 개선/후속 (비차단)
1. **Unregister(OnDespawned) 런타임 미검증**: 현재 씬에 슬라임을 풀로 반납하는 despawn 로직이 없어(수명/사망 미구현) OnDespawned→Unregister 경로는 코드상 배선만 확인, 런타임 미실측. 사망/수명 클립에서 반납 붙일 때 함께 검증.
2. **CharacterController 치수** 기본값 사용 — 슬라임 스케일에 맞춰 튜닝 여지(콜라이더 시각/충돌 정확도).
3. **분리/충돌회피 없음**: 정지한 Player로 5마리가 dist=0까지 겹쳐 수렴(MVP 허용). 군집 분리는 후속.
4. **FSM/NpcActor 유닛테스트** 미작성: 등록순 1전이·프레임당 1회·Exit→Enter·초기 Enter, HasTarget=false 정지.

### 하네스 교훈(추가)
- **에디터 비포커스 + Run In Background 꺼짐 → 플레이 프레임 미진행**: MCP로 play 후 Time.time이 0.02에 멈춰 스폰/이동 무증상. `Application.runInBackground = true`(런타임 set)로 프레임 진행시켜 검증. 이후 MCP 라이브 검증 전 runInBackground 보장 필요.

### 아쉬웠던 점 / 원인 → 반영
- **Player 등록 경로 공백**(1차에서 의심 → 2차에서 해결): `PlayerActor`가 IActor 미구현이라 Player가 ActorManager에 안 실렸음. 사용자 지시로 `PlayerActor:IActor`+`PlayerContext` 주입 등록 추가 → 런타임 슬라임 수렴으로 배선 검증 완료.
- **refresh 스코프 함정 재확인**: 신규 .cs는 `scope=scripts` 컴파일 전에 자산 임포트가 안 돼 "타입 없음"(CS0246)으로 뜸. 신규 파일 생성 직후엔 `scope=all force`로 임포트→컴파일해야 함.
- **중첩 struct set_property 함정**: MCP `set_property`에 `{"_stats":{...}}` nested-dict를 주면 조용히 무시(0 유지). 도트 경로 `_stats._moveSpeed`로 개별 지정해야 먹음. 프리팹/컴포넌트의 Serializable struct 필드 세팅 시 주의.

### 다음 테스트 (다음 실행 입력)
- **FSM/NpcActor 유닛테스트** 추가: 등록순 1전이·프레임당 1회·Exit→Enter·초기 Enter, HasTarget=false 정지. (게이트3 자동화 공백 메우기)
- **풀 재사용 재등록 보강**(위 후속 1) + 필요 시 **중력/접지**(후속 2)·CharacterController 치수 튜닝(후속 3).
- game-plan 순서: 이동→(스폰+추격AI ✅ 이번 완료)→**체력·피격**이 다음. NpcActor/PlayerActor에 체력·피격 상태(Attack/Death)를 같은 FSM 그래프에 얹는 클립으로 연결.

### 하네스 자체 개선 메모
- prd 스킬: (a) 파일배치 예시 경로를 실제 `00_Scripts/`·`01_Scripts/` 관례로 교정, (b) "PlayerContext와 동일 패턴" 같은 선례참조와 "[Inject] X 배선" 같은 구체지시를 동시에 넣어 모순이 반복됨 → 택1로 통일.
- 순수 C# 소비측이라도 DI/씬 배선이 붙는 순간 게이트 2~4는 게이트 B(프리팹·에셋) 승인 이후에만 유효 → 코드-only 완료와 런타임-완료를 분리 보고.
