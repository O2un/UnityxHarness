using O2un.Actors;
using UnityEngine;

namespace O2un.Combat
{
    public sealed class MeleeSwingSkill : ISkillDefinition
    {
        private readonly float _cooldown;
        private readonly float _range;
        private readonly float _lifetime;
        private readonly int _damage;
        private readonly ActorType _targetTeam;
        private readonly AttackHitboxView _prefab;
        private readonly string _poolKey;
        private readonly ITargetStrategy _targeting;

        public MeleeSwingSkill(
            float cooldown,
            float range,
            float lifetime,
            int damage,
            ActorType targetTeam,
            AttackHitboxView prefab,
            string poolKey,
            ITargetStrategy targeting)
        {
            _cooldown = cooldown;
            _range = range;
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
            Vector3 toTarget = target.Transform.position - origin;
            toTarget.y = 0f;

            Quaternion rotation = toTarget.sqrMagnitude > 0f
                ? Quaternion.LookRotation(toTarget, Vector3.up)
                : ctx.OwnerRotation;

            AttackRequest request = new()
            {
                Prefab = _prefab,
                PoolKey = _poolKey,
                Origin = origin + rotation * Vector3.forward * (_range * 0.5f),
                Rotation = rotation,
                MoveDirection = Vector3.zero,
                Speed = 0f,
                FollowOwner = null,
                Lifetime = _lifetime,
                ReHitInterval = 0f,
                ReleaseOnHit = false,
                Policy = HitPolicy.OncePerTarget,
                Damage = _damage,
                TargetTeam = _targetTeam,
            };

            ctx.Spawner.Spawn(request);
        }
    }
}
