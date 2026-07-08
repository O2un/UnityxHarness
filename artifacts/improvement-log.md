# 개선 기록 (improvement-log)

이 파일은 **가장 최근 실행 1건**만 담는 단일 스냅샷이다. 다음 실행의 Phase 0-A가 그대로 읽어 연속 작업 기준으로 삼는다. 새 실행이 끝나면 통째로 덮어쓴다(이전 내용은 덮어쓰기 전에 chain-log.md로 축약 누적).

---

## 2026-07-08 · AttackSystem(자동공격·히트박스·피격·처치) 구현

### 무엇을 했나
- `docs/requirements/attack-system.md`(PRD) 구현. game-plan 순서상 **체력·피격 + 자동공격**을 하나의 전투 파이프라인(발동→판정→피격→처치)으로 묶음. 설계(unity-architect)→게이트 A→구현(gameplay-engineer)→게이트 1→체크포인트 커밋→게이트 B(unity-ai-operator)→런타임 버그 2건 수정→재검증→리뷰(code-reviewer)까지 풀 파이프라인.
- **신규 공통 골격** `Assets/00_CommonFramework/00_Scripts/Combat/` (ns `O2un.Combat`):
  - Skill/: `ISkillDefinition`·`ISkillContext`·`SkillContext`·`SkillModule`(순수, 슬롯 쿨다운→Activate)·`ITargetStrategy`·`NearestEnemyStrategy`·`SelfTargetStrategy`·`SkillDefinitionSO`(abstract base).
  - Hitbox/: `IDamageable`·`IAttackSpawner`·`AttackSpawner`(Service, IPoolService lazy 등록)·`AttackHitboxView`(MB, IPoolable, 트리거 판정)·`DamageableView`(MB, IDamageable)·`AttackRequest`·`HitboxConfig`·`HitboxModule`(순수, 정책·수명·R3 이벤트)·`HitPolicy`·`DamageEvent`(=IDamageable+int).
  - Health/: `IHealth`·`PlayerHealthAdapter`(IPlayerDataWriter→IHealth, 역의존 없음).
- **신규 프로젝트 콘텐츠** `Assets/10_ProjectA/01_Scripts/Combat/` + Actor/Npc:
  - `MeleeSwingSkill`·`ProjectileSkill`·`AuraFieldSkill`(각 ISkillDefinition) + 각 SO(`[CreateAssetMenu]`).
  - `EnemyHealth`(순수, 인스턴스별 IHealth+OnDeath). `MonsterDataSO`(MaxHp·MoveStats·AttackSkill).
- **수정**: PlayerActor/PlayerContext(SkillModule·자동공격·Player DamageableView 배선), NpcActor/NpcContext(EnemyHealth·처치·MonsterDataSO·적 공격 SkillModule), GameSceneScope(`AttackSpawner` As<IAttackSpawner> 등록), + 버그수정 AttackHitboxView·PoolModule.

### 사용자 승인 (게이트 A + 설계 질문)
- **배치**: "골격 공통 / 콘텐츠 프로젝트". 계약·Module·Service·View·SkillDefinitionSO(abstract)는 00_CommonFramework, 구체 스킬 3종·각 SO·MonsterDataSO·EnemyHealth는 10_ProjectA. → PlayerContext(공통)는 base SO[]만 참조해 역의존 회피.
- **Player 피격 포함**: 적→Player 경로까지 배선. `MonsterDataSO.AttackSkill`에 TargetTeam=Player 오라 SO 지정 → 적이 오라 히트박스로 Player 타격(동일 파이프라인, 데이터만 다름).
- **ITargetStrategy 분리 채택**: NearestEnemy(근접·투사체 공용)/Self(오라).
- 나머지 기본값: 오라 재판정 0.5s·EveryInterval, 근접·투사체 OncePerTarget, 처치=NpcContext가 OnDeath→EnemyContext.Release(Unregister는 ActorView.OnDisable 재사용), 풀 lazy 등록.

### 4단계 게이트
- ①컴파일 ✅ — refresh(scope=all,force) 후 read_console error 0.
- ②런타임 ✅ — **버그 2건 수정 후** 런타임·종료 전 구간 콘솔 에러 0.
- ③기능 🟡 정성 통과 — 히트박스 3종 풀 스폰·순환(Instantiate/Destroy 없음), 근접 히트박스 스폰=NearestEnemyStrategy 타깃 성공 방증, 슬라임 전량 풀 반납=피격·처치 사이클 방증. **정량 수치(HP 감소·처치 카운트·Player 피격) 미포착**(operator에 execute_code 미노출 + 에디터 백그라운드 프레임 스로틀).
- ④사용자 ⏳ (viewer 육안 확인 대기 — 특히 정량 검증 위임).

### 게이트 B 실행 내역 (완료)
- 히트박스 프리팹 3종 `Assets/10_ProjectA/02_Prefabs/Hitbox/`(Hitbox_MeleeSwing/Projectile/Aura): 트리거 SphereCollider + kinematic Rigidbody + AttackHitboxView.
- SO 5종 `Assets/10_ProjectA/02_Prefabs/Combat/`: MeleeSwingSkill/ProjectileSkill/AuraFieldSkill(Player용, targetTeam Enemy) + AuraFieldSkill_EnemyToPlayer(targetTeam Player, poolKey 팀별 분리) + MonsterData_Slime(maxHp 30, move 3/720, attackSkill=EnemyToPlayer).
- 프리팹 배선: SlimePBR에 DamageableView + NpcContext(_monsterData/_damageable). PlayerActor에 Rigidbody(kinematic)+DamageableView + PlayerContext(_skills 3종/_damageable).
- 물리: 별도 Layer 미생성 → HitboxModule.TargetTeam 팀 필터로 자해 방지. 트리거 이벤트는 가해측 kinematic Rigidbody로 보장.
- 부수: ProjectSettings runInBackground 0→1(operator, 프레임 진행 목적).

### 런타임 버그 2건 (발견·수정·재검증 완료)
1. **AttackHitboxView.cs NRE 폭주**: ReleaseOnHit 명중 시 OnHit→ReleaseSelf→OnDespawned가 `_hitbox`를 동기 null화하는데 같은 물리 스텝 잔여 트리거 콜백이 released 히트박스에 도달. → `TryHit`에 `null == _hitbox` 가드 + 캐시 Collider를 OnDespawned에서 enabled=false/Configure에서 enabled=true. 재검증 0건.
2. **PoolModule.cs teardown MissingReferenceException**: 씬/스코프 파기 시 Unity가 pooled 오브젝트를 먼저 destroy → OnDestroy가 파기된 obj.gameObject 접근. → `if (null != obj)` Unity-null 가드. 종료 후 0건.

### 리뷰 (code-reviewer): blocker 0 / major 1 / minor 8
- **M1(코드 결함 아님)**: `ISkillContext.Owner:Transform`가 설계 §3.1("Vector3만")과 상충하나 §3.5(`AttackRequest.FollowOwner:Transform`)와는 정합 → **설계 문서 내부 모순**. 아키텍트가 §3.1을 구현(오라 Transform 추종 허용)에 맞춰 정정 권장.
- m1(PoolModule `if (obj != null)` 평가값 앞) **수정 완료** → `null != obj`. 나머지 m2~m8은 기존 코드 debt·설계 준수·정보성 → 후속(아래).

### 남은 개선/후속 (비차단)
1. **게이트 3 정량 검증**: EnemyHealth.CurrentHP 감소·처치 카운트·적→Player 오라의 Player CurrentHP 감소를 execute_code 가능 환경/포커스 보장에서 실측. (자동 gate3-test.json은 이번 .cs 신규라도 갱신 안 됨 — 전투 기능 실측 스크립트 추가 여지.)
2. **M1 설계 문서 정정**(unity-architect): §3.1 순수성 서술 vs Transform 추종 정합화.
3. **m5 HitboxModule**: 스폰마다 new 대신 풀 재사용(Reset 활용) 또는 IDisposable 정리 일원화. 현재 Subject 미Dispose(구독은 Clear로 끊김, GC 대상이라 무해).
4. **m2/m3/m6 PlayerActor 기존 debt**: 빈 Init()·미사용 _disposables·하드코딩 시작 HP(SetCurrentHP(100)) — 스탯 SO화 시 함께 정리.
5. **밸런싱**: SO 수치는 예시값(데미지·쿨다운·HP). 실제 밸런싱 후속.
6. **전투/체력 유닛테스트** 미작성(HitboxModule 정책 dedupe·재판정, SkillModule 쿨다운, EnemyHealth 0 도달 OnDeath).

### 하네스 교훈
- **가해/피격 히트박스 수명 경합**: 풀 재사용 View에서 트리거 콜백이 released 인스턴스에 도달할 수 있음. `_active` 플래그만으론 부족 — 핵심 참조(`_hitbox`) null 가드 + Collider.enabled 토글로 물리 이벤트 자체 차단이 견고.
- **풀 teardown 순서**: 씬/스코프 Dispose가 Unity 오브젝트 파기 이후 실행 → PoolModule.OnDestroy는 Unity-null 가드 필수. (공통 인프라라 다른 풀 대상에도 적용됨.)
- **operator 도구 한계**: unity-ai-operator에 execute_code 미노출 → 순수 Module 런타임 값 정량 계측 불가. 정량 게이트 3은 execute_code 보유 주체 또는 Stop-hook gate3 자동 러너로.
- **에디터 비포커스 프레임 스로틀** 재확인: runInBackground=1로도 MCP 표본 호출만으론 충분한 프레임 미유발. 초기 스폰 버스트로 정성 검증은 가능.

### 다음 테스트 (다음 실행 입력)
- game-plan 순서: 이동→스폰+추격AI ✅→**체력·피격+자동공격 ✅ 이번 완료**→다음은 **경험치·레벨업**(처치 시 XP 지급·레벨업 강화). AttackSystem의 `EnemyHealth.OnDeath`를 XP 지급 훅으로 재사용 가능.
- 그 전에 (선택) 게이트 3 정량 검증 스크립트 + 전투 유닛테스트로 자동화 공백 메우기.
