# Project A 밸런스 Prep 정리 프롬프트

완성된 Project A의 실제 코드와 애셋을 읽고, 밸런스 Eval 입력으로 쓸 `@docs/evals/wave-balance-prep.md`를 실제 값 기준으로 정리해 주세요. 추측값을 넣지 말고, 애셋에 설정된 값과 코드에서 유도되는 값만 씁니다.

먼저 다음을 확인합니다.

- `@Assets/10_ProjectA/03_Data/Actor/Npc/**` — `MonsterDataSO`(maxHp, MoveStats, 공격 스킬, exp)
- `@Assets/10_ProjectA/03_Data/Manager/EnemySpawner/**` — `WaveDataSO`의 `WaveEntry`(SpawnTime, Count, Placement, Timing, EndTime)
- `@Assets/10_ProjectA/03_Data/Combat/Skills/Player/**` — `SkillStats`(Damage, Cooldown, Range/Speed, 업그레이드)
- `@Assets/10_ProjectA/01_Scripts/Manager/EnemySpawner/WaveModule.cs` — 이산 스폰 타임라인 규칙
- `@Assets/00_CommonFramework/00_Scripts/Manager/PoolManager/PoolModule.cs` — 풀 설정 현황

## 정리할 내용

1. **몬스터 기준**: 각 `MonsterDataSO` 애셋의 addressKey·maxHp·moveSpeed·공격·exp를 표로.
2. **웨이브 기준**: `WaveDataSO`의 이산 스폰을 Wave별 구간(SpawnTime~EndTime)과 구성(key × Count)으로. `spawnRate`는 별도 필드가 아니라 파생 지표임을 명시.
3. **플레이어 기준**: 단발 스킬은 damage·cooldown·range에서 기준 DPS를 유도(Damage / Cooldown). 지속 판정 스킬은 `reHit`과 명중 유지 조건을 함께 기록해 Play 측정 대상으로 둔다. 레벨 업그레이드 애셋이 있으면 함께.
4. **풀**: `PoolModule`이 prewarm·최대치 설정이 없음을 밝히고, prewarm은 각 poolKey의 실측 peak active에서 **역산할 대상**으로 표시. 전체 peak alive는 난이도·화면 부하 지표로 별도 기록.
5. **Play 실측 필드**: 레벨·업그레이드 경로별 실측 DPS·동시 타격 수·생존 지표·전체 peak alive와 poolKey별 peak active를 채울 빈 표.

## 규칙

- 애셋에 없는 수치를 사실처럼 쓰지 않습니다. 확인 안 된 값은 `Needs Measurement`로 남깁니다.
- 실제로 있는 몬스터·웨이브·스킬만 기록합니다(존재하지 않는 종류를 지어내지 않습니다).
- 이후 `/balance-eval`이 이 문서를 입력으로 실측값과 대조할 수 있도록 표 구조를 유지합니다.
