using O2un.Actors;
using UnityEngine;

namespace O2un.Combat
{
    public sealed class ProjectileSkill : ISkillDefinition
    {
        private readonly float _cooldown;
        private readonly float _speed;
        private readonly float _lifetime;
        private readonly int _damage;
        private readonly ActorType _targetTeam;
        private readonly AttackHitboxView _prefab;
        private readonly string _poolKey;
        private readonly ITargetStrategy _targeting;

        public ProjectileSkill(
            float cooldown,
            float speed,
            float lifetime,
            int damage,
            ActorType targetTeam,
            AttackHitboxView prefab,
            string poolKey,
            ITargetStrategy targeting)
        {
            _cooldown = cooldown;
            _speed = speed;
            _lifetime = lifetime;
            _damage = damage;
            _targetTeam = targetTeam;
            _prefab = prefab;
            _poolKey = poolKey;
            _targeting = targeting;
        }

        public float Cooldown => _cooldown;

        public void Activate(ISkillContext ctx)
        {
            IActor target = _targeting.Select(ctx.Query, ctx.OwnerPosition);
            if (null == target)
            {
                return;
            }

            Vector3 origin = ctx.OwnerPosition;
            Vector3 direction = target.Transform.position - origin;
            direction.y = 0f;

            if (direction.sqrMagnitude <= 0f)
            {
                return;
            }

            direction.Normalize();

            AttackRequest request = new()
            {
                Prefab = _prefab,
                PoolKey = _poolKey,
                Origin = origin,
                Rotation = Quaternion.LookRotation(direction, Vector3.up),
                MoveDirection = direction,
                Speed = _speed,
                FollowOwner = null,
                Lifetime = _lifetime,
                ReHitInterval = 0f,
                ReleaseOnHit = true,
                Policy = HitPolicy.OncePerTarget,
                Damage = _damage,
                TargetTeam = _targetTeam,
            };

            ctx.Spawner.Spawn(request);
        }
    }
}
