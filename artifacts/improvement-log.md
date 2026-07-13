# 개선 기록 (improvement-log)

## 2026-07-13 · balance-probe 구현

### 구현 내용
에디터 전용 밸런스 실측 계측 도구. 사람이 한 판 플레이하는 동안 웨이브 구간별 실측 지표를 기존 이벤트 구독으로 모아 판 종료 시 CSV 출력. 전부 `Assets/10_ProjectA/01_Scripts/`, CommonFramework 무변경.

- **데미지 채널** (`Combat/Damage/`): `EnemyDamageChannel`(= `IEnemyDamageSource`+`IEnemyDamagePublisher`), ExpGainedChannel 패턴 그대로. `EnemyHealth.VaryHP`가 HP 감소분을 publish → 생존자 포함 정확한 총 데미지로 실측 DPS 산출.
- **주입 배선**: `NpcContext.Construct`에 `IEnemyDamagePublisher` 추가 → `new EnemyHealth(maxHp, _damagePublisher)`.
- **BalanceProbeManager** (`BalanceProbe/`, 파일 전체 `#if UNITY_EDITOR`): IInitializable/ITickable/IDisposable 엔트리포인트. 5개 웨이브 윈도(1–12/14–26/28–40/42–54/56–70초) 하드코딩. 구간별 집계: measuredDps(=Σ데미지/구간길이)·kills·peakAlive(`IActorQuery.GetActive(Enemy).Count` 폴링)·level·wasHit·visited. Playing 전이 시 계측 시작, Victory/Defeat에서 `BalanceLogs/balance-probe_<ts>.csv` 출력, Idle 전이 시 ResetRun(재시작 대비).
- **DI** (`GameSceneScope`): `EnemyDamageChannel` 무조건 등록(빌드에서도 NpcContext가 IEnemyDamagePublisher 필요) + 프로브 엔트리포인트는 `#if UNITY_EDITOR`.
- `.gitignore`에 `/BalanceLogs/` 추가.

### 결정 (사용자 승인)
- DPS 소스: EnemyHealth 이벤트 채널(정확도 우선) — PRD의 '비침범' AC를 정확도 위해 완화. 게임 로직 수정은 HP 감소 1지점 publish로 최소화.
- 출력 경로: 프로젝트 루트 `BalanceLogs/`. 종료 훅: `IGameManager.CurrentState`(클리어·게임오버 둘 다).

### 검증
- Gate 1 컴파일: 통과(에러 0). 신규 파일은 `refresh scope=all`로 임포트해야 인식됨(scripts-only 리프레시로는 새 .cs 미컴파일 → 최초 CS0234/CS0246 발생, full refresh로 해소).
- Gate 2 Play: 진입 에러 0, DI 그래프 정상(EnemyDamageChannel이 NpcContext 충족, 프로브 엔트리포인트 생성).
- Gate 3·4: 미완 — CSV는 UI Start→판 종료까지 실제 플레이해야 생성. MCP UI 클릭 불가로 자동화 불가.

### 다음 실행 규칙 (미완 항목)
- **Gate 3·4 수동 확인 필요**: GameScene Play → GameSelect Start → 한 판 완주 → 콘솔 `[BalanceProbe] ... 저장` 로그 + 루트 `BalanceLogs/*.csv` 생성 확인. CSV 축(웨이브·DPS·peakAlive)이 `docs/evals/wave-balance-prep.md`와 대조 가능한지 확인.
- 동시 타격 수 지표는 미구현(PRD Open Question, 히트박스 이벤트 부재로 Needs Assumption). 필요 시 스킬 히트 채널 추가로 확장.
- `WINDOWS`는 `WaveData_Test` 기준 하드코딩. 웨이브 구성이 바뀌면 이 상수도 갱신 필요(WaveDataSO에서 동적 산출로 확장 여지).
