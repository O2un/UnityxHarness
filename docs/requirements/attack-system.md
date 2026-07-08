# AttackSystem PRD

> 근거: `docs/spec/attack-system-spec.md`

## Overview

가까운 적을 쿨다운마다 자동으로 공격하고, 맞은 적이 피격되는 전투 루프를 구현한다. 뱀서 MVP의 핵심 전투 시스템으로, 공격 방식은 근접 스윙 / 투사체 / 오라·장판 3종이지만 발동 → 판정 → 피격의 흐름은 하나의 파이프라인으로 공유한다.

## Goals

- 스킬 쿨다운·발동 흐름을 `SkillModule`(순수 C#)로 구현한다
- 피격 판정·이벤트 발행을 `HitboxModule`(순수 C#)로 구현한다
- 타깃 선정 전략 + 공격 방식을 `ISkillDefinition` 계약으로 추상화하고 구현 3종(근접 스윙 / 투사체 / 오라·장판)을 제공한다
- 가해 히트박스(`AttackHitboxView`)와 피격 히트박스(`DamageableView` + `IDamageable`)를 View 계층에 배치한다
- 스킬 3종의 수치(쿨다운·사거리·히트박스 모양·데미지·지속시간)와 몬스터 데이터(체력·이동속도)를 `ScriptableObject`로 데이터화한다
- 피격 이벤트를 구독해 체력 감소와 처치 판정까지 연결한다

## Out of Scope

- 경험치 지급
- 스킬 선택 UI
- 레벨업 강화

## Technical Requirements

### 구성요소

| 구성요소 | 계층 | 책임 |
|---|---|---|
| `SkillModule` | Module (순수 C#) | 쿨다운 관리·발동 시점 결정. "언제 발동할지"만 안다 |
| `HitboxModule` | Module (순수 C#) | 피격 판정 처리·R3 이벤트 발행. Unity API 비의존 |
| `ISkillDefinition` | 계약 + 데이터 | 타깃 선정 전략 + 공격 방식 정의. 구현 3종 |
| `AttackHitboxView` | View (MonoBehaviour) | 가해 히트박스. 스킬이 켜는 트리거 Collider 판정 영역 |
| `DamageableView` | View (MonoBehaviour) | 피격 히트박스. 몸체 Collider + `IDamageable` |

### 역할 분리

- `SkillModule`은 타깃 선정도 공격 방식도 직접 결정하지 않는다 — 둘 다 스킬 정의(`ISkillDefinition`)가 정한다
- 타깃 후보는 `ActorManager`(`IActorQuery`)를 발동 컨텍스트로 스킬 정의에 넘겨줄 뿐, `SkillModule`이 직접 고르지 않는다
- 공격 방식별·타깃별 `if`/`switch` 분기 금지 — 다형성으로 처리한다

### 히트박스 판정

- 판정은 가해 히트박스(`AttackHitboxView`, 트리거 Collider)와 피격 히트박스(`DamageableView`, 몸체 Collider)가 겹칠 때 발생한다
- Collider는 둘 다 View 계층에만 존재하고, `HitboxModule`은 어느 쪽 Unity API에도 의존하지 않는다

### 스킬 3종

| 스킬 | 판정 방식 |
|---|---|
| 근접 스윙 | 전방 히트박스 즉시 판정 |
| 투사체 발사 | 투사체 스폰은 `PoolModule`에 위임 (`Get`/`Release` 경유, `Instantiate`/`Destroy` 직접 호출 금지) |
| 오라·장판 | 지속 히트박스, 주기마다 재판정 |

- 쿨다운·사거리·히트박스 모양·데미지·지속시간은 코드 하드코딩이 아니라 스킬 정의 값으로 관리한다

### 데이터 (ScriptableObject)

- 몬스터 데이터(체력·이동속도 등): `ScriptableObject` → `NpcContext`가 입력받는다
- 스킬 3종 정의(타깃 선정 전략·공격 방식·데미지·쿨타임 등): 각각 `ScriptableObject` → `PlayerContext`가 입력받아 조립한다

### 계약 (피격 이벤트)

- 피격 이벤트 payload는 `IDamageable` 대상 + 데미지만 담는다
- 구체 적·Player 타입 참조 금지 — `HitboxModule`은 상대가 적인지 Player인지 모른다
- R3 구독은 `AddTo`로 lifecycle에 바인딩한다

## Acceptance Criteria

- [ ] 플레이어가 입력 없이 쿨다운마다 가까운 적을 자동 공격한다
- [ ] 근접 스윙·투사체·오라/장판 3종이 모두 동작하며, 발동·판정·피격 흐름은 공통 파이프라인을 공유한다
- [ ] `SkillModule`·`HitboxModule`이 Unity API 비의존 순수 C#이며 `new`로 생성 가능하다
- [ ] 공격 방식별·타깃별 `if`/`switch` 분기가 없다 — `ISkillDefinition` 구현 교체만으로 방식이 바뀐다
- [ ] 투사체가 `PoolModule.Get`/`Release`로 스폰·회수되고 `Instantiate`/`Destroy` 직접 호출이 없다
- [ ] 스킬 수치(쿨다운·사거리·데미지·지속시간 등)와 몬스터 데이터가 `ScriptableObject`로 분리되어 코드 수정 없이 조정 가능하다
- [ ] 피격 이벤트 payload에 구체 적·Player 타입 참조가 없다 (`IDamageable` + 데미지만)
- [ ] R3 구독이 모두 `AddTo`로 lifecycle에 바인딩된다
- [ ] 피격 이벤트 구독으로 체력이 감소하고, 체력 0 도달 시 처치 판정이 일어난다

## Open Questions

- 코드 배치 위치: 각 구성요소를 `00_CommonFramework`(공통)로 둘지 `10_ProjectA`(프로젝트 전용)로 둘지 구현 직전 사용자 승인 필요 (하네스 규칙상 오픈 퀘스천)
- 오라·장판의 재판정 주기 값과 동일 대상 중복 타격 허용 정책(틱당 1회 등) 미정 — 스킬 정의 값으로 둘지 확인 필요
- 처치 판정 이후 처리(오브젝트 회수·`ActorManager` Unregister 주체)의 소유 위치 미정
