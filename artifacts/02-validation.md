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
