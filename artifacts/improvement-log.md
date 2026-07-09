# 개선 기록 (improvement-log)

이 파일은 **가장 최근 실행 1건**만 담는 단일 스냅샷이다. 다음 실행의 Phase 0-A가 그대로 읽어 연속 작업 기준으로 삼는다. 새 실행이 끝나면 통째로 덮어쓴다(이전 내용은 덮어쓰기 전에 chain-log.md로 축약 누적).

---

## 2026-07-08 · 스폰 시스템 확장(도넛 랜덤 + 정규분포 시간분산) + 풀 체력 리셋 버그

### 무엇을 했나
- `artifacts/04-user-feedback.md` 피드백 3건 적용. 배치(게이트 A) = **00_CommonFramework**(스폰 시스템이 이미 공통), 정규분포 σ=(end-start)/6 자동(사용자 확정).
- **R1 버그**: `NpcContext.Build()`의 `GetComponentInParent<EnemyContext>()` → `GetComponentInParent<EnemyContext>(true)`(includeInactive) + null 경고 로그.
- **R2 도넛 스폰**(신규): `WaveEntry`에 `SpawnPlacement`(Fixed/PlayerRadius)+MinRadius/MaxRadius. 좌표는 런타임에 `IActorQuery.Player` 기준 해석. 순수 `AnnulusSampler`(면적균등 √lerp).
- **R3 정규분포 시간분산**(신규): `WaveEntry`에 `SpawnTiming`(Burst/NormalSpread)+EndTime. `WaveModule`이 생성 시 타임라인 사전 전개(개체별 시각=clamp(Gaussian(μ,σ),start,end) 후 정렬), `GetSpawnsAt` 커서 전진. 순수 `GaussianSampler`(Box-Muller, seed 옵션).
- 파일: (신규) `Manager/EnemySpawner/GaussianSampler.cs`·`AnnulusSampler.cs`. (수정) `WaveDataSO.cs`(enum 2종+WaveEntry 필드)·`SpawnRequest.cs`(배치 필드+FromEntry/Fixed 팩토리)·`WaveModule.cs`(타임라인 전개)·`EnemySpawnManager.cs`(IActorQuery 주입+ResolvePosition)·`NpcContext.cs`(R1).

### 사용자 승인 (게이트 A)
- 배치: 00_CommonFramework 확정. 정규분포 퍼짐: 자동 (end-start)/6.

### 4단계 게이트
- ①컴파일 ✅ (초기 CS0104 `Random` 모호성 1건 → `System.Random` 한정 수정 후 error 0).
- ②런타임 ✅ Play 콘솔 error 0.
- ③기능 ✅ **정량 실측**(execute_code):
  - R1: 수정 후 5마리 enemyCtx=ok, 죽으면 active=False(반납 정상화), 재사용시 HP 0/30 dead=True → 30/30 dead=False.
  - R2: 반경 3.009~6.997(3~7 링 내 전량)·y평면.
  - R3: 500개 전량 [10,20]내·중앙 밀집·평균 대칭. 회귀 Burst t5→3 정상.
- ④사용자 ⏳ (viewer).

### R1 근본 원인 (핵심 교훈)
- 증상 "재사용 시 체력 안 참"의 실제 = **죽은 적이 풀 반납조차 못 하고 active=True/HP0으로 방치**.
- 원인: `GetComponentInParent<T>()`/`GetComponentInChildren<T>()`는 **기본적으로 비활성 오브젝트를 건너뜀**. 풀 생성/주입 시점 오브젝트가 비활성이라 EnemyContext 미검출 → `_enemyContext=null` → ResetFull 미배선 + Release() no-op.
  - (원래 `GetComponent<T>()`는 비활성 미스킵이라 self는 찾았을 것이나 계층은 못 봄. 견고성 위해 `GetComponentInParent<T>(true)`로 둘 다 커버.)
- **하네스 교훈: 풀링 대상에서 컴포넌트 조회는 반드시 includeInactive 오버로드**(`GetComponentInParent/Children(true)`). 오브젝트가 비활성 상태로 생성·주입될 수 있음.

### 게이트 B + 추가 버그(도넛이 원점 한 점에 스폰) — 근본 원인 규명·수정
- `WaveData_Test.asset` 2엔트리를 PlayerRadius로 구성(엔트리1 min5/max9·NormalSpread[1~6s]·Count8, 엔트리2 min6/max10·Burst@3s·Count5).
- **사용자 재보고("안 퍼진다, 한 곳(원점)에서 나온다")** — 실측으로 파고들어 진짜 원인 발견:
  - ResolvePosition은 정상(로그 resolved=플레이어 주위 링, afterAssign=링). 그러나 프레임 경과 후 실제 transform은 전부 **원점**.
  - **근본 원인 = CharacterController 텔레포트 버그**: 적 루트에 CC가 있고, CC가 **활성인 채** `transform.position`을 대입하면 Transform만 잠깐 바뀌고 CC 내부 위치는 풀 리셋값(원점) 그대로 → 다음 `Controller.Move()`가 transform을 원점으로 **스냅백**. (기존 Fixed 스폰도 항상 원점이었으나 적이 원점≈플레이어라 무증상이었음.)
- **수정**: `EnemySpawnManager`에 `Teleport(transform, pos)` — CC 있으면 `enabled=false → position 대입 → enabled=true`(CC 내부 위치까지 동기화). 플레이어 이동에서 쓰던 것과 동일 패턴.
- **재검증**: 플레이어를 (50,0,50)으로 이동 후 스폰 11마리 전부 dPlayer 3.5~8.7·**atOrigin 0**, 위치 x43~55/z43~57로 사방 분산 → 링 유지 확증(원점 아님).
- **하네스 교훈**: 풀에서 꺼낸 CharacterController 오브젝트의 스폰 위치는 `transform.position` 직접 대입 금지 — CC를 껐다 켜야 내부 위치가 동기화됨. (안 그러면 첫 Move에서 원점 스냅백.)

### 남은 개선/후속 (비차단)
2. **유닛테스트**: AnnulusSampler(반경 경계)·GaussianSampler·WaveModule(타임라인 전개·회귀) EditMode 테스트로 자동화(현재 execute_code 1회 실측).
3. **RNG 시드**: WaveModule/AnnulusSampler는 무시드(런타임 랜덤). 재현 필요 시 시드 노출 여지.
4. (이전 실행 이월) AttackSystem M1 설계문서 §3.1 정정, HitboxModule 풀 재사용, PlayerActor debt 정리.

### 다음 테스트 (다음 실행 입력)
- game-plan 순서상 다음은 **경험치·레벨업**(처치 시 XP 지급·레벨업 강화). `EnemyHealth.OnDeath`를 XP 훅으로 재사용 가능.
- 코드 리뷰(code-reviewer)는 이번 스폰 확장분(WaveModule 타임라인·샘플러 순수성·EnemySpawnManager if 분기) 미진행 → 후속 점진 검토 여지.
