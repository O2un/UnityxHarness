# 패시브 스킬 카드 기능 (PassiveSkillCard)

> 분해 4/4 — `item-upgrade-system.md`의 "패시브 스킬 효과" 축.
> **선행 의존: `upgrade-card-data-inventory.md`(2/4)의 해금 집합, `room-reward-card-spawn.md`(3/4)의 획득 진입점.**
> 이 PRD는 이미 데이터로만 존재하던 패시브 2종에 실제 전투 효과를 붙인다.

## Overview

패시브 스킬 카드는 액티브 스킬이 아니라 **공격 적중 시 상시 발동하는 효과**다. 이번 범위의 2종은 크리티컬(`CriticalOnHit`)과 유도 미사일(`HomingMissile`)이며, 둘 다 근접 콤보의 적중 이벤트를 트리거로 삼는다. 분해 2/4에서 카드는 이미 인벤토리에 담기고 해금 집합에 반영되지만 아무 일도 하지 않는 상태다. 이 PRD는 그 해금 집합을 게이트로 삼아 실제 전투 동작을 붙인다.

## Goals

- 근접 콤보 적중 시점에 크리티컬 판정을 넣고, 성공 시 피해 배수를 적용한다.
- 근접 콤보 적중 시점에 유도 미사일을 추가 발사한다.
- 두 효과 모두 해금 집합 조회로 게이트한다. 미해금 상태의 전투 동작은 기존과 완전히 동일하다.
- 유도 미사일이 가장 가까운 적을 향해 궤도를 수정한다.
- 크리티컬 확률·배수, 미사일 피해·속도·유도 강도를 데이터로 조정 가능하게 한다.

## Out of Scope

- 액티브 스킬 도입. 공통 `SkillModule`/`SkillDefinitionSO`는 이번에도 도입하지 않는다.
- 패시브 3종 이상 확장. 이번은 `CriticalOnHit`·`HomingMissile` 2종이다.
- 원거리 스킬(`RangedSkillModule` 입력 발사) 적중에서의 패시브 발동. 이번 트리거는 **근접 콤보 적중**만이다.
- 크리티컬 전용 연출(피격 이펙트·데미지 숫자·화면 흔들림).
- 유도 미사일 전용 아트. 기존 `RangedSkillData.ProjectilePrefab`을 재사용하거나 플레이스홀더로 진행한다.
- 패시브 중복 획득 시의 강화(확률 누적 등). 해금은 on/off다.
- 곱연산 스탯 modifier. 크리티컬 배수는 이 패시브 내부의 피해 계산이지 스탯 modifier가 아니다.
- 자동화 테스트 검증 — 테스트 어셈블리(asmdef) 도입 전까지 검증하지 않는다.

## Technical Requirements

### 배치

- 신규 코드는 `Assets/20_ProejctB/01_Scripts/Player/Combat/` 아래에 둔다 (근접·원거리 전투 코드가 이미 있는 곳).
- 공통 `00_CommonFramework`의 `HitboxModule`·`HitboxConfig`·`DamageEvent`는 **수정하지 않는다.** `HitboxConfig`는 `readonly struct`이며 피해량이 생성 시 굳는다.

### 트리거 지점

- 근접 적중은 `Player2DActor.CreateStageHitboxes`가 만든 각 `HitboxModule.OnHit` 구독에서 처리된다. 분해 1/4이 이 핸들러를 "적중 시점 피해량 해석"으로 이미 바꿔 놓았으므로, 패시브 판정도 **같은 핸들러 안**에서 이어 붙인다.
- `HitboxModule`은 `HitPolicy.OncePerTarget`으로 동작하므로 한 스테이지에서 같은 적에게 한 번만 `OnHit`이 발행된다. 패시브도 자연히 타격당 1회 판정된다.
- 최종 피해 계산 순서: `스테이지 base Damage + AttackBonus` → 크리티컬 배수 적용 → 최소 1로 하한 → `ApplyDamage`.

### 크리티컬 (CriticalOnHit)

- 해금되지 않았으면 판정 자체를 하지 않는다(난수도 소비하지 않는다).
- 확률 판정에 성공하면 피해량에 배수를 적용한다. 배수 적용 후 정수로 내림 처리하고 최소 1을 보장한다.
- 확률·배수는 SO 필드로 노출한다. 기존 `MeleeComboData`를 늘리지 말고 패시브 전용 데이터 SO(`PassiveSkillData`)를 새로 만든다 — 패시브는 근접 콤보와 수명이 다르고 유도 미사일 설정도 함께 들어가야 한다.
- 판정 로직은 **Unity API 비의존 순수 C#**으로 분리한다. `UnityEngine.Random`/`Mathf` 같은 정적 유틸을 값으로 쓰는 것은 무방하다.

### 유도 미사일 (HomingMissile)

- 해금 시, 근접 적중마다 유도 미사일 1발을 추가 발사한다.
- 발사 경로는 `Player2DActor.FireProjectile`의 흐름을 **재사용**한다: `IPoolService.IsRegistered` 확인 → 필요 시 `Register` → `GetHandle<Projectile2DView>` → `Get()` → `HitboxConfig` 구성 → `Configure(...)`. 입력이 아니라 적중 이벤트가 트리거라는 점만 다르다.
  - `RangedSkillModule`의 쿨다운·캐스팅 상태는 **거치지 않는다.** 이 발사는 애니메이션 캐스팅이 없는 즉시 발사다.
  - `Player2DActor`의 `rangedRefs` 생성 조건(`RangedSkillRefs` 미할당 시 원거리 스킬 비활성)은 **건드리지 않는다.** 다만 유도 미사일은 프리팹과 `IPoolService`가 있어야 발사되므로, 없으면 경고 로그를 남기고 조용히 건너뛴다.
- **현재 `Projectile2DView`는 직선 이동만 한다** (`transform.position += _direction * _speed * dt`, `_direction`은 `Configure` 시점에 고정). 유도를 위해 다음 중 하나를 택한다:
  - (권장) `Projectile2DView`에 유도 대상과 회전 속도를 받는 오버로드/추가 설정을 넣어, `Update`에서 목표 방향으로 `_direction`을 점진 회전시킨다. 기존 직선 발사 경로는 유도 미설정 시 지금과 동일하게 동작해야 한다.
  - 별도 `HomingProjectile2DView`를 만들고 풀 키를 분리한다.
- 대상 탐색은 씬 탐색(`GameObject.Find`/`FindObjectsOfType`) 금지. 공통 `IActorQuery`(`ActorManager`)로 적을 조회하고, 가장 가까운 적을 고른다. `NearestEnemyStrategy`가 이미 공통에 있으므로 재사용 가능 여부를 먼저 확인한다.
- 대상이 없으면 현재 바라보는 방향으로 직선 발사한다. 대상이 도중에 사라지면 마지막 방향을 유지한다.
- 미사일 피해·속도·수명·유도 강도는 `PassiveSkillData` SO 필드로 노출한다.
- 미사일은 `HitPolicy.OncePerTarget`, `releaseOnHit = true`로 구성해 적중 시 회수한다.

### 해금 조회

- 분해 2/4의 집계 Module이 노출한 해금 조회(`IsUnlocked(PassiveSkillType)` 또는 읽기 전용 집합)를 사용한다. 패시브 해금 상태를 이 PRD에서 다시 저장하거나 캐시하지 않는다.
- `Player2DActor`가 조회 인터페이스를 생성자 주입으로 받는다. 인터페이스로만 의존하고 구체 클래스를 참조하지 않는다.
- `Player2DContext`가 주입 경로를 잇는다. 주입 실패 시 기존 널 검사 패턴(경고 로그 + 해당 기능만 비활성, 이동·공격은 유지)을 따른다.

### DI

- `PassiveSkillData` SO는 `Player2DContext`의 직렬화 필드로 두고 `Player2DActor` 생성자로 넘긴다(`MeleeComboRefs`/`RangedSkillRefs`와 같은 방식). 소비처가 하나뿐이므로 전역 `RegisterInstance`하지 않는다.

## Acceptance Criteria

- [ ] 크리티컬 패시브 획득 전에는 같은 적에게 주는 근접 피해량이 항상 일정하다.
- [ ] 크리티컬 패시브 획득 후에는 일부 타격의 피해량이 눈에 띄게 커진다.
- [ ] 크리티컬이 터진 타격도 피해량이 정수이고 0 이하가 되지 않는다.
- [ ] 유도 미사일 패시브 획득 전에는 근접 적중 시 추가 투사체가 발사되지 않는다.
- [ ] 유도 미사일 패시브 획득 후에는 근접 적중마다 투사체가 1발 발사된다.
- [ ] 발사된 미사일이 근처의 다른 적을 향해 궤도를 꺾는다.
- [ ] 화면에 적이 하나뿐이라 유도 대상이 없어도 미사일이 직선으로 날아가고 콘솔 에러가 없다.
- [ ] 미사일이 적에게 맞으면 피해를 주고 사라진다.
- [ ] 미사일이 아무것도 맞히지 않으면 수명 후 회수된다 (씬에 누적되지 않는다).
- [ ] 기존 원거리 스킬(입력 발사)의 쿨다운·캐스팅 동작이 패시브 도입 전과 동일하다.
- [ ] 두 패시브를 모두 획득해도 콤보 진행과 애니메이션이 끊기지 않는다.
- [ ] 패시브를 하나도 획득하지 않은 상태의 전투 감각이 기존 플레이와 동일하다.
- [ ] 룸을 전환해도 해금된 패시브가 계속 발동한다.
- [ ] Play 모드 진입부터 2개 룸 클리어까지 콘솔 에러가 없다.

## Open Questions

- 크리티컬 확률·배수 초기값 미정 (예: 20% / 2배). 체감 가능한 값으로 시작해 플레이 후 조정한다.
- 유도 미사일이 매 적중마다 발사되면 다수 적 구간에서 투사체가 과다해질 수 있다. 발사 쿨다운(예: 0.3초)을 둘지 결정 필요.
- 유도 미사일 프리팹을 기존 원거리 스킬 프리팹과 공유할지, 별도 풀 키로 분리할지 미정. 유도 로직을 `Projectile2DView`에 넣을지 별도 View로 뺄지와 함께 결정한다.
- 유도 대상 선정에 `NearestEnemyStrategy`(공통 `ITargetStrategy`)를 그대로 쓸 수 있는지, 2D 좌표계와 맞는지 구현 전 확인 필요.
- 미사일 프리팹·`PassiveSkillData` 에셋 생성은 **에셋 변경이므로 별도 승인이 필요하다.**
