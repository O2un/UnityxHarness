# Melee Combo — 근접 공격 3단 콤보

## Overview

2D 플랫폼 액션(Project B) 플레이어의 근접 공격 기능. 공격 입력으로 발동하는
기본공격→콤보1→콤보2 3단 근접 콤보 스킬 1종을 구현한다. 데미지 판정 타이밍은
Animation Event가 쥐고, 콤보 전이는 입력 구간(콤보 윈도우)과 입력 버퍼로 잇는다.
판정·피격은 공통 코어의 `HitboxModule`·`IDamageable`·R3 피격 이벤트 계약을 재사용한다.

## Goals

- 공격 입력으로 근접 콤보 스킬을 발동한다 (쿨다운 자동 발동이 아님)
- 3단 콤보(기본공격→콤보1→콤보2)를 콤보 입력 구간 내 입력으로 전이한다
- 구간이 열리기 전 입력은 버퍼에 기록했다가 구간이 열리는 순간 소비한다 (점프 버퍼와 동일 계약)
- 히트박스 On/Off를 Animation Event로 처리해 애니메이션 수정 시 판정 타이밍이 함께 따라오게 한다
- 대상 판정·중복 타격 방지는 공통 `HitboxModule`을 호출해 처리한다
- 피격은 R3로 단방향 발행하고 대상은 `IDamageable` 계약에만 의존한다
- 콤보 단계·입력 구간·버퍼 판단은 Unity API 비의존 순수 Module이 소유한다 (new로 테스트 가능)
- 콤보 입력 구간·버퍼 시간, 단계별 데미지·히트박스 크기를 ScriptableObject로 조절 가능하게 한다

## Out of Scope

- 원거리 투사체 스킬 — [ranged-projectile.md](ranged-projectile.md)로 분리
- 특수 스킬
- 방어·패링
- 넉백·경직 연출
- 공통 코어(`HitboxModule`·피격 이벤트·`IDamageable`) 재작성 — 참조만 한다
- 공통 `SkillModule`의 자동 Tick 발동 경로를 근접 콤보에 그대로 쓰는 것

## Technical Requirements

### 배치

- 신규 코드(콤보 Module, 전용 발동 경로, 공격 View, 데이터 SO)는 `Assets/20_ProjectB` 하위에 둔다
- 공통 계약은 `Assets/00_CommonFramework/00_Scripts/Combat`의 기존 코드를 참조:
  - `O2un.Combat.HitboxModule` — `TryHit(IDamageable)` 대상·팀 판정, `HitPolicy` 중복 타격 방지, `OnHit` (R3 `Observable<DamageEvent>`) 발행
  - `O2un.Combat.IDamageable` — `Team`(ActorType), `ApplyDamage(int)`

### 구성요소

| 구성요소 | 레이어 | 책임 |
|---|---|---|
| 근접 콤보 스킬 정의 (데이터) | ScriptableObject | 단계별 데미지·히트박스 크기·콤보 입력 구간 시간·버퍼 시간 |
| 콤보 판단 Module | Module (순수 C#) | 콤보 단계, 입력 구간 열림/닫힘, 버퍼 상태, 다음 단계 전이 여부 판단 |
| 공격 View | MonoBehaviour (View) | Animator 재생, Animation Event 수신, 히트박스 Collider 소유·On/Off |
| 발동 경로 (어댑터) | Project B 전용 | 공격 입력(Reactive)으로 근접 콤보 발동을 건다 |
| Animation Event 브리지 | MonoBehaviour | 애니메이션 재생 GameObject에 부착, 이벤트를 순수 Module로 전달 |

### 판정·콤보 흐름

- 각 공격 애니메이션 클립에 Animation Event 4종을 단다:
  - 히트박스 켜기 (칼날이 지나기 시작하는 프레임)
  - 히트박스 끄기 (스윙이 끝나는 프레임)
  - 콤보 입력 구간 열림 / 닫힘 (애니메이션 후반)
- Animation Event가 호출하는 함수는 애니메이션 재생 GameObject의 컴포넌트(브리지)에 있어야 하며, 브리지가 순수 콤보 Module로 신호를 전달한다
- 콤보 입력 구간 내 공격 입력 → 다음 단계 전이. 구간을 놓치면 대기 자세 복귀
- 구간 열림 전 입력은 버퍼에 기록, 열리는 순간 소비
- 켜진 히트박스와 겹친 대상 판정·한 스윙 내 중복 타격 방지는 `HitboxModule.TryHit` 호출로 처리
- 히트박스 On/Off는 코드 타이머로 재지 않는다 (공통 풀 기반 히트박스 View의 수명 방식과 별개 경로)
- 공격 입력은 Reactive 값으로 받아 판단 Module/발동 경로가 구독한다
- 데이터 SO 값은 Context가 입력받아 Module·스킬 정의에 넘긴다 (CLAUDE.md의 WithParameter/RegisterInstance 기준 적용)

### 공격 중 다른 행동 정책

- 공격 중에도 이동 입력은 그대로 처리한다 (이동 잠금 없음)
- 점프·대시 입력은 공격을 캔슬한다: 콤보 단계 초기화, 히트박스 즉시 Off, 해당 행동으로 전이
- 데미지 값은 int 기반 (`DamageEvent`·`ApplyDamage(int)`와 동일)

### 의존 방향

- Module은 Unity API 비의존, View→Module 신호 전달만 허용 (Module이 View 참조 금지)
- 공격 로직은 구체 적 타입이 아닌 `IDamageable`에만 의존

## Acceptance Criteria

- [ ] 공격 입력 1회로 기본공격이 발동한다 (자동 쿨다운 발동 아님)
- [ ] 콤보 입력 구간 내 입력 시 기본공격→콤보1→콤보2로 전이한다
- [ ] 콤보 입력 구간을 놓치면 대기 자세로 복귀하고 콤보 단계가 초기화된다
- [ ] 구간 열림 전 입력이 버퍼에 저장되어 구간이 열리는 순간 다음 단계가 발동한다
- [ ] 히트박스가 Animation Event 프레임에 켜지고 꺼진다 (코드 타이머 없음)
- [ ] 한 스윙에 같은 대상이 두 번 타격되지 않는다 (`HitboxModule` 정책)
- [ ] 피격 시 R3 이벤트가 발행되고 구독자가 체력 감소를 처리한다
- [ ] 콤보 판단 Module이 Unity API 없이 new로 생성·테스트 가능하다
- [ ] 단계별 데미지·히트박스 크기·구간/버퍼 시간이 SO에서 조절 가능하다
- [ ] 공격 중 이동 입력이 정상 동작한다 (이동 잠금 없음)
- [ ] 점프·대시 입력 시 공격이 캔슬되고 콤보 단계가 초기화되며 히트박스가 꺼진다
- [ ] 신규 코드가 모두 `Assets/20_ProjectB` 하위에 있고 공통 코어는 수정되지 않았다

## Resolved

- 3단계 공격 애니메이션 클립(기본·콤보1·콤보2)은 이미 준비되어 있음
- 공격 중 이동은 잠그지 않는다; 점프·대시는 공격을 캔슬한다
- 데미지는 int 기반으로 통일
