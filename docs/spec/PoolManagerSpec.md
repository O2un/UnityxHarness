| 질문 | 답변 |
|---|---|
| 무엇을 감싸는가 | `UnityEngine.Pool.ObjectPool<T>`를 감싼 `PoolModule<T>`를 소유·생성 |
| 외부 노출 범위 | 외부 코드는 `IPoolService`와 `IPoolHandle<T>`만 봄. `PoolModule<T>` 구현체는 직접 주입받지 않음 |
| 생성 방식 | `PoolManager`가 `IObjectResolver`를 생성자 주입받아 보유. 풀 인스턴스는 `resolver.Instantiate(prefab)`으로 생성 |
| 풀 구분 기준 | 오브젝트 키. 예: `enemy_goblin`, `enemy_slime`, `projectile_basic`, `effect_hit` |
| 등록 방식 | VContainer LifetimeScope에 Singleton Manager로 등록하고 `IPoolService`로 노출 |
| 적용 대상 | Enemy, Projectile, Effect |