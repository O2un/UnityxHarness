using O2un.Manager;
using UnityEngine;

namespace O2un.Combat
{
    public sealed class AttackSpawner : IAttackSpawner
    {
        private readonly IPoolService _pool;

        public AttackSpawner(IPoolService pool)
        {
            _pool = pool;
        }

        public void Spawn(in AttackRequest request)
        {
            if (null == request.Prefab)
            {
                Debug.LogError("[AttackSpawner] AttackRequest.Prefab is null.");
                return;
            }

            if (false == _pool.IsRegistered(request.PoolKey))
            {
                _pool.Register(request.PoolKey, request.Prefab);
            }

            IPoolHandle<AttackHitboxView> handle = _pool.GetHandle<AttackHitboxView>(request.PoolKey);
            if (null == handle)
            {
                return;
            }

            AttackHitboxView view = handle.Get();

            HitboxConfig config = new(
                request.Damage,
                request.TargetTeam,
                request.Policy,
                request.ReHitInterval,
                request.Lifetime);

            HitboxMotion motion = new()
            {
                Origin = request.Origin,
                Rotation = request.Rotation,
                MoveDirection = request.MoveDirection,
                Speed = request.Speed,
                FollowOwner = request.FollowOwner,
                ReleaseOnHit = request.ReleaseOnHit,
            };

            view.Configure(new HitboxModule(config), motion);
        }
    }
}
