# Project A 웨이브 밸런스 Prep — 기댓값 산정

> 실제 구현된 코드·애셋 값으로 **계산식을 돌려 기댓값을 뽑고**, 임계값과 대조해 난이도 흐름을 평가한다.
> 이번 평가는 애셋에 설정된 값이 원본인 **기댓값(expected) 기준**이다.
> 동적 플레이 실측(실제 조작·회피·명중률이 섞인 값)은 이 문서 범위 밖이며, 여기서는 계산 가정으로만 다룬다.
> 수치를 임의로 바꾸지 않는다.

---

## 1. 몬스터 기준 (`MonsterDataSO`)

`Assets/10_ProjectA/03_Data/Actor/Npc/**` 애셋 기준. HP는 `EnemyHealth(maxHp)`로 주입, 이동은 `MoveStats`, 공격은 `SkillDefinitionSO`.

| addressKey | 애셋 | maxHp | moveSpeed | 공격 | exp |
|---|---|---:|---:|---|---:|
| `Enemy_Slime` | `MonsterData_Slime` | 30 | 3.0 | 근접 스킬 | 1 |
| `Enemy_Dash` | `DashEnemyData` | 15 | 3.5 | 없음(돌진 접촉) | 5 |
| `Enemy_ArmoredMelee` | `ArmoredMeleeEnemyData` | 60 | 1.5 | `EnemyMeleeSwing` | 20 |

## 2. 웨이브 기준 (`WaveDataSO` — `WaveData_Test`)

스폰은 `spawnRate`가 아니라 **이산 이벤트**다. 각 `WaveEntry`는 `SpawnTime`에 `Count`마리를, `Timing=NormalSpread`이면 `[SpawnTime, EndTime]` 구간에 정규분포로 흩뿌린다(`WaveModule.ResolveSpawnTime`). `Placement=PlayerRadius`는 플레이어 반경 `MinRadius~MaxRadius`에 배치.

| Wave | 구간(초) | 구성 (key × Count) | 도착 HP 합 |
|---|---|---|---:|
| 1 | 1–12 | Slime ×8 | 240 |
| 2 | 14–26 | Slime ×6 · Dash ×4 | 240 |
| 3 | 28–40 | Slime ×5 · Dash ×4 · Armored ×3 | 390 |
| 4 | 42–54 | Slime ×7 · Dash ×5 · Armored ×4 | 525 |
| 5 | 56–70 | Slime ×10 · Dash ×7 · Armored ×6 | 765 |

> 도착 HP 합 = Σ(Count × maxHp). 구간 길이(대개 12초)로 나누면 초당 유입 HP가 나온다(§4 pressure 입력).

## 3. 플레이어 기대 DPS (`SkillStats` + `SkillUpgradeData`)

`Assets/10_ProjectA/03_Data/Combat/Skills/Player/**`. 기준 DPS는 `Damage / Cooldown`으로 유도한다. 레벨업은 `SkillUpgradeData` 델타를 누적 적용(`ApplyUpgrade`)한 값으로 계산한다.

| 스킬 | Lv1 (dmg/cd → DPS) | Lv2 누적 (dmg/cd → DPS) | 비고 |
|---|---|---|---|
| MeleeSwing | 10 / 1.0 → 10 | 26 / 0.6 → 43 | 단발, 히트박스 범위 내 다수 타격 가능 |
| Projectile | 8 / 0.8 → 10 | 23 / 0.3 → 77 | 단발, 명중률은 계산 가정(§4) |
| AuraField | 5 / reHit 0.5 → 10/s | dmg 8 / reHit 0.4 → 20/s | 지속 판정, 범위 내 유지 가정 |

> 단발 스킬의 기대 DPS는 대미지÷쿨다운. 지속 판정(AuraField)은 대미지÷reHit 간격. 동시 타격 수·명중률·범위 유지는 조작에 따라 달라지므로 **계산 가정**으로 명시하고 실측으로 확정하지 않는다.

## 4. 파생 지표 (계산식)

애셋 값으로 오프라인 계산한다. 플레이어 DPS는 §3의 기대 DPS를, 필요하면 조작 계수 `k`(명중·범위 유지율, 기본 가정 0.6~1.0)를 곱해 스윕한다.

- **HP 유입 압력** `pressure(wave) = (도착 HP 합 / 구간 길이) / (기대 DPS × 동시 타격 수)`
  - `< 0.8` 안정 · `0.8–1.2` 경계 · `> 1.2` 처치 속도 부족(위험)
- **예상 동시 생존 수** `expectedPeakAlive` — 유입과 처치율(기대 DPS ÷ 평균 HP)의 차로 구간별 누적 추정. 플레이어 성능을 낮음/중간/높음으로 스윕해 밴드로 낸다.
- **역산 pool prewarm** `prewarm(poolKey) = ceil(해당 종 expectedPeakAlive × 여유계수)` — 여유계수 기본 1.2.

## 5. 기댓값 표 (계산 결과를 채움)

| Wave | 초당 유입 HP | 기대 DPS 밴드(낮음~높음) | pressure 밴드 | expectedPeakAlive | 판정 |
|---|---:|---|---|---:|---|
| 1 | 계산 |  |  |  |  |
| 2 | 계산 |  |  |  |  |
| 3 | 계산 |  |  |  |  |
| 4 | 계산 |  |  |  |  |
| 5 | 계산 |  |  |  |  |

| poolKey | expectedPeakAlive | 역산 prewarm(×1.2) |
|---|---:|---:|
| `Enemy_Slime` |  |  |
| `Enemy_Dash` |  |  |
| `Enemy_ArmoredMelee` |  |  |

## 6. 판정 기준

- 각 구간에서 기대 DPS가 초당 유입 HP를 감당하는지(pressure), 초반→후반으로 pressure가 의도한 대로 상승하는지, 업그레이드 경로별 pressure 차이, 역산 prewarm이 expectedPeakAlive를 덮는지를 본다.
- 계산 가정(명중률·범위 유지)이 결과를 좌우하는 항목은 그 가정을 함께 적는다. 가정이 불확실하면 밴드로 남기고 단일값으로 단정하지 않는다.
- 동적 플레이 실측은 이 평가 범위 밖이다. 실측이 필요하면 별도 작업으로 분리한다.
