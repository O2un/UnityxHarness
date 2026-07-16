# Ranged Projectile — 원거리 투사체 스킬

## Overview

2D 플랫폼 액션(Project B) 플레이어의 원거리 투사체 스킬 1종. 스킬 입력으로 발동하며
쿨다운으로 연사를 제한한다. 투사체는 공통 풀 기반 히트박스 View(`AttackHitboxView`)를
재사용하고, 판정·피격은 기존 `HitboxModule`·`IDamageable`·R3 `DamageEvent` 계약을
그대로 따른다. 근접 콤보(melee-combo)와 독립적으로 동작한다.

## Goals

- 스킬 입력으로 원거리 투사체 스킬 1종을 발동한다 (자동 발동 아님)
- 쿨다운이 남아 있으면 입력을 무시한다 (연사 제한)
- 투사체 이동·수명·피격을 공통 `AttackHitboxView` + `AttackSpawner` 풀 경로로 처리한다
- 스킬 데이터(데미지·쿨다운·투사체 속도·수명)를 SO로 조절 가능하게 한다
- 피격은 기존 R3 `DamageEvent` 발행 계약을 그대로 사용하고 대상은 `IDamageable`에만 의존한다
- 투사체 발사 방향은 플레이어가 바라보는 방향(2D 좌/우)을 따른다

## Out of Scope

- 근접 콤보 스킬 (melee-combo.md에서 별도로 다룬다)
- 공통 코어(`SkillModule`·`AttackSpawner`·`AttackHitboxView`·`HitboxModule`) 수정 — 참조만 한다
- 조준·차징·관통 등 투사체 변형
- 스킬 획득·업그레이드 UI

## Technical Requirements

### 배치

- 신규 코드(투사체 스킬 정의 SO, 입력 발동 경로)는 `Assets/20_ProjectB` 하위에 둔다
- 공통 계약은 `Assets/00_CommonFramework/00_Scripts/Combat`의 기존 코드를 참조:
  - `O2un.Combat.SkillDefinitionSO` / `ISkillDefinition` — 스킬 데이터·발동 빌드
  - `O2un.Combat.AttackSpawner` + `AttackRequest` — 풀 기반 히트박스 스폰
  - `O2un.Combat.AttackHitboxView` — `HitboxMotion` 이동(MoveDirection·Speed), 수명, `ReleaseOnHit`
  - `O2un.Combat.HitboxModule` / `IDamageable` — 판정·피격 계약

### 구성요소

| 구성요소 | 레이어 | 책임 |
|---|---|---|
| 투사체 스킬 정의 (데이터) | ScriptableObject | 데미지·쿨다운·속도·수명·히트박스 프리팹·풀 키 |
| 투사체 히트박스 프리팹 | Prefab (`AttackHitboxView`) | Collider·이동·수명, 명중 시 풀 반환(`ReleaseOnHit`) |
| 입력 발동 경로 (어댑터) | Project B 전용 | 스킬 입력(Reactive) 구독 → 쿨다운 체크 → 발사 |
| 쿨다운 판단 | Module (순수 C#) 또는 공통 SkillModule 슬롯 재사용 | 쿨다운 잔여 판단 — 공통 재사용 가능 여부는 설계 단계에서 결정 |

### 발동·판정 흐름

- 스킬 입력(Reactive) 수신 → 쿨다운 잔여 확인 → 남아 있으면 무시, 아니면 발사 + 쿨다운 시작
- 발사 시 `AttackRequest`로 투사체 히트박스를 스폰: `MoveDirection` = 플레이어 바라보는 방향, `Speed` = SO 값, `Lifetime` 만료 또는 명중 시 풀 반환
- 대상 판정·중복 타격 방지·`DamageEvent` 발행은 `HitboxModule` 기존 경로 그대로
- 근접 콤보 공격 중에도 스킬 입력은 독립적으로 처리한다 (상호 잠금 없음)
- 데이터 SO는 CLAUDE.md의 WithParameter/RegisterInstance 기준에 따라 Context가 입력받아 넘긴다

### 의존 방향

- Project B 코드는 공통 코어의 인터페이스·공개 계약만 참조하고 수정하지 않는다
- 공격 로직은 구체 적 타입이 아닌 `IDamageable`에만 의존

## Acceptance Criteria

- [ ] 스킬 입력 1회로 투사체가 발사된다 (자동 발동 아님)
- [ ] 쿨다운이 남아 있는 동안의 입력은 무시된다
- [ ] 투사체가 플레이어가 바라보는 방향으로 SO에 지정된 속도로 이동한다
- [ ] 명중 또는 수명 만료 시 투사체가 풀로 반환된다
- [ ] 피격 시 R3 `DamageEvent`가 발행되고 대상 체력이 감소한다
- [ ] 데미지·쿨다운·속도·수명이 SO에서 조절 가능하다
- [ ] 근접 콤보 중에도 투사체 발동이 독립적으로 동작한다
- [ ] 신규 코드가 모두 `Assets/20_ProjectB` 하위에 있고 공통 코어는 수정되지 않았다

## Open Questions

- 데이터(데미지·쿨다운·투사체 속도·수명) 값 미정 — 구현 시 플레이스홀더 값으로 시작
- 스킬 입력 키/버튼 바인딩 미정
- 쿨다운 판단을 공통 SkillModule 슬롯 재사용으로 할지 전용 Module로 할지 — 설계 단계에서 결정

## Resolved

- 발동 방식: 입력 발동 + 쿨다운 연사 제한 (자동 Tick 발동 아님)
- 발사 방향: 플레이어가 바라보는 좌/우 방향
- 데미지는 int 기반
