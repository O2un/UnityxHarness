# PoolManager — PRD

## Overview
`UnityEngine.Pool.ObjectPool<T>`를 감싼 오브젝트 풀링 매니저. Enemy·Projectile·Effect처럼 빈번히 생성·파괴되는 오브젝트를 재사용해 GC/Instantiate 비용을 줄인다. 외부 코드는 `IPoolService`와 `IPoolHandle<T>`만 통해 풀을 사용한다.

## Goals
- 문자열 키 단위로 풀을 구분·관리한다 (`enemy_goblin`, `projectile_basic`, `effect_hit` 등).
- 외부에 `IPoolService`(풀 획득/반납 진입점)와 `IPoolHandle<T>`(개별 풀 핸들)만 노출한다.
- 풀 인스턴스를 `IObjectResolver.Instantiate(prefab)`로 생성해 VContainer DI가 유지되도록 한다.
- VContainer LifetimeScope에 Singleton Manager로 등록하고 `IPoolService`로 노출한다.
- `PoolModule<T>` 구현체는 외부에 직접 주입/노출하지 않는다 (내부 소유).

## Out of Scope
- Addressables 기반 프리팹 로딩 (프리팹은 등록 시점에 주입/전달된다고 가정).
- 씬 전환 시 풀 자동 정리·영속 정책.
- 풀 크기 자동 튜닝/프리워밍 전략 (기본 ObjectPool 파라미터만 사용).
- Enemy/Projectile/Effect 각 시스템의 스폰 로직 자체 (풀은 인프라만 제공).

## Technical Requirements
- **레이어**: `PoolManager`(Manager, 순수 C#) → `PoolModule<T>`(Module, 순수 C#, `UnityEngine.Pool.ObjectPool<T>` 래핑).
- **생성자 주입**: `PoolManager(IObjectResolver resolver)`. 풀 인스턴스는 `resolver.Instantiate(prefab)`로 생성.
- **풀 구분**: `string` 키 기반 딕셔너리로 키→`PoolModule<T>` 매핑.
- **제네릭 T**: `Component` 제약. `IPoolHandle<T>`의 `T`는 컴포넌트 타입이다.
- **인터페이스**:
  - `IPoolService` — 키로 풀 핸들 획득, 등록 진입점.
  - `IPoolHandle<T>` where `T : Component` — 개별 풀에서 `Get()` / `Release(obj)`.
- **반납 API**: `handle.Release(obj)` 형태를 표준으로 한다.
- **등록**: VContainer LifetimeScope에서 Singleton, `As<IPoolService>()` 노출.
- **배치 위치**: `00_CommonFramework` (공통 인프라).

```
00_CommonFramework/00_Scripts/Manager/PoolManager/
├── PoolManager.cs
├── IPoolService.cs
├── IPoolHandle.cs
└── PoolModule.cs
```

## Acceptance Criteria
- [ ] `IPoolService`로 문자열 키를 통해 오브젝트를 Get/Release 할 수 있다.
- [ ] 반납된 오브젝트가 재사용되어 새 Instantiate 없이 재획득된다.
- [ ] `resolver.Instantiate(prefab)`로 생성되어 풀 오브젝트에 DI가 주입된다.
- [ ] `IPoolHandle<T>`의 `T`는 `Component` 제약을 가지며, `Release(obj)`로 반납한다.
- [ ] 외부 코드에서 `PoolModule<T>`를 직접 참조/주입하지 않는다.
- [ ] VContainer에 Singleton으로 등록되고 `IPoolService`로 resolve된다.
- [ ] Enemy·Projectile·Effect 중 최소 1개 대상에 실제 적용해 동작을 검증한다.

## Open Questions
1. **풀 등록 시점** — 프리팹+키를 언제 어떻게 등록하나? (부트 시 일괄 등록 vs 최초 Get 시 lazy 생성) — 나중에 처리.
