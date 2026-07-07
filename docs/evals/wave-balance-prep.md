# Wave Balance Evaluation — 데이터 & 계산식 초안

> 목적: monster ActorData를 기준으로 웨이브 난이도를 정량 평가하기 위한
> (1) 입력 데이터 스키마, (2) 파생 지표 계산식, (3) 판정 임계값 초안을 잡는다.
> 아직 구현 안 된 값은 `[미구현]`으로 표시한다.

---

## 1. 입력 데이터 (현재 코드 기준)

### 1-1. Wave — 실데이터 있음
`WaveDataSO.WaveEntry` 기준.

| 필드 | 타입 | 비고 |
|---|---|---|
| AddressableKey | string | 몬스터 식별자 = 풀 키 겸용 |
| SpawnTime | float(초) | **이산 스폰 시각** (연속 rate 아님) |
| Count | int | 해당 시각에 한 번에 뜨는 수 |
| Position | Vector3 | 스폰 위치 |

> prep 원안의 `spawnRate`는 현재 데이터에 없다. 스폰은 "시각 t에 Count개"인 이산 이벤트라
> rate는 **파생 지표**(§3-1)로 계산한다. `start/end`도 별도 필드가 아니라
> `min/max(SpawnTime)`으로 유도한다.

### 1-2. Monster ActorData — `[미구현]`, 이번에 스키마 확정 필요
현재 `EnemyContext`는 풀링만 하고 스탯이 없다. 평가의 전제이므로 아래 필드를 ActorData(SO)로 신설 제안.

| 필드 | 타입 | 평가에서 쓰는 곳 | 필수도 |
|---|---|---|---|
| addressKey | string | Wave·Pool 조인 키 | 필수 |
| poolKey | string | 풀 매핑(현재 addressKey와 동일 취급) | 필수 |
| hp | int | HP 유입 압력(§3-3), TTK(§3-2) | 필수 |
| moveSpeed | float | 플레이어 도달 시간, 회피 난이도 | 필수 |
| contactDamage | int | 생존 압력(§3-4) | 평가 2차 |
| bodyRadius | float | 스웜 가독성/밀도(§3-5) | 평가 2차 |
| xpValue | int | 성장 곡선 매칭(나중) | 나중 |

> `moveSpeed`는 이미 `O2un.Actors.MoveStats`에 있으니, ActorData가 MoveStats를 품거나
> 참조하는 형태로 재사용한다(중복 정의 금지).

### 1-3. Pool — `[미구현]`
현재 `PoolModule`은 Unity `ObjectPool` 기본값이라 prewarm/최대치 설정이 없다.
평가로 **역산한 값을 채우는 대상**이다.

| 필드 | 타입 | 산출 근거 |
|---|---|---|
| poolKey | string | = addressKey |
| prewarm | int | ceil(peakAlive × 1.2) (§3-2 결과로 채움) |
| expectedMaxAlive | int | = peakAlive (§3-2) |

### 1-4. Player — 나중 (비교 대상)
`PlayerDataStore`에 MaxHP(=100)만 있음. 평가 2차에서 붙임.

| 필드 | 현재 | 필요 |
|---|---|---|
| maxHP | 100 (고정) | level curve |
| dps(t) | 없음 | 레벨별 dps |
| attackRange | 없음 | 유효 사거리 |
| maxTargets | 없음 | 동시 타격 수 |

---

## 2. 표기

- `t` : 경과 시간(초)
- `W` : 슬라이딩 윈도우 폭(초). 기본 `W = 5`
- `n_i, t_i, m_i` : i번째 WaveEntry의 Count / SpawnTime / 몬스터종
- `hp(m)`, `spd(m)` : 몬스터 m의 hp, moveSpeed
- `D(t)` : 플레이어 순간 DPS · `K` : maxTargets · 없으면 상수로 가정해 스윕

---

## 3. 파생 지표 & 계산식

### 3-1. 스폰율 (spawns/sec)
이산 이벤트를 윈도우로 평활화.

```
spawnRate(t) = ( Σ  n_i  where  t-W ≤ t_i ≤ t ) / W
누적 스폰수  N_spawn(t) = Σ n_i  where  t_i ≤ t
```

### 3-2. 동시 생존 수(peakAlive) — 풀 크기의 근거
사망이 없으면 상한 `alive_max(t) = N_spawn(t)`.
플레이어 처치를 넣은 흐름 모델(Δt 스텝):

```
killCap(t)  = D(t) × K / hp_avg(t)         # 초당 처치 가능 마릿수
alive(t)    = max(0, alive(t-Δt) + spawn(Δt) - min(alive(t-Δt), killCap(t)·Δt))
peakAlive   = max_t alive(t)
```

- `hp_avg(t)` : 그 시점 생존 몬스터 hp 가중평균(단일종이면 hp)
- 플레이어 스탯이 없으면 `D·K`를 스윕(예: 낮음/중간/높음 3케이스)해 peakAlive 밴드를 낸다.
- **산출물**: `expectedMaxAlive = peakAlive`, `prewarm = ceil(peakAlive × 1.2)`

### 3-3. HP 유입 압력 (핵심 지표)
윈도우 동안 "도착하는 총 체력" vs "플레이어가 낼 수 있는 총 데미지".

```
HPflux(t)  = Σ ( n_i × hp(m_i) )   where  t-W ≤ t_i ≤ t
pressure(t) = HPflux(t) / ( D(t) × W )
```

- `pressure < 1` : 플레이어가 유입을 따라잡음(안정)
- `pressure ≈ 1` : 균형 경계
- `pressure > 1` : 처치 속도 부족 → alive 증가 → 압도. 지속되면 스파이크 구간.

### 3-4. 생존 압력 (contactDamage 확보 후)
플레이어에 닿는 몬스터의 초당 데미지 vs 플레이어 HP.

```
incomingDPS(t) = alive_contact(t) × contactDamage_avg
survivalTime  = maxHP / max(ε, incomingDPS(t) - regen)
```

- `alive_contact(t)` : alive 중 플레이어 사거리/접촉권에 든 수(1차 근사로 alive 전체 사용)
- `survivalTime`이 웨이브 잔여시간보다 짧으면 그 구간은 위험.

### 3-5. 스웜 가독성 / 밀도
화면 위 몬스터가 시각적으로 구분되는지.

```
density(t)  = alive_onScreen(t) / screenArea
spacing(t)  = sqrt( screenArea / max(1, alive_onScreen(t)) )   # 평균 최근접 간격
readable    = spacing(t) > bodyRadius × 2 × k          # k: 가독 여유계수(초안 1.5)
```

- `spacing < bodyRadius×2` 근처면 겹쳐 보임 → 가독성 경고.

---

## 4. 판정 임계값 초안 (튜닝 전 기본값)

| 지표 | 안정 | 경고 | 위험 |
|---|---|---|---|
| pressure(t) (§3-3) | < 0.8 | 0.8–1.2 | > 1.2 지속 |
| peakAlive (§3-2) | ≤ expectedMaxAlive | — | 초과(풀 부족) |
| survivalTime (§3-4) | > 잔여시간 | ~잔여시간 | < 잔여시간 |
| spacing (§3-5) | > radius×3 | radius×2~3 | < radius×2 |

윈도우 `W`, 여유계수 `k`, 처치 스윕 밴드는 실측 후 조정한다.

---

## 5. 다음 단계

1. `MonsterActorData` SO 스키마 확정 (§1-2) — 위치는 공통/프로젝트 승인 필요.
2. `WaveDataSO` + ActorData를 읽어 §3 지표를 뽑는 오프라인 평가기(순수 C# Module) 작성.
3. Player 스탯(dps/range/maxTargets) 붙으면 §3-2·3-3의 스윕을 실값으로 교체.
4. §4 임계값을 실측 플레이 데이터로 캘리브레이션.
