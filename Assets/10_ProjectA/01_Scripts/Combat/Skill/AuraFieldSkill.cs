using O2un.Actors;
using UnityEngine;

namespace O2un.Combat
{
    public sealed class AuraFieldSkill : ISkillDefinition
    {
        private readonly float _cooldown;
        private readonly float _lifetime;
        private readonly float _reHitInterval;
        private readonly int _damage;
        private readonly ActorType _targetTeam;
        private readonly AttackHitboxView _prefab;
        private readonly string _poolKey;
        private readonly ITargetStrategy _targeting;

        public AuraFieldSkill(
            float cooldown,
            float lifetime,
            float reHitInterval,
            int damage,
            ActorType targetTeam,
            AttackHitboxView prefab,
            string poolKey,
            ITargetStrategy targeting)
        {
            _cooldown = cooldown;
            _lifetime = lifetime;
            _reHitInterval = reHitInterval;
            _damage = damage;
            _targetTeam = targetTeam;
            _prefab = prefab;
            _poolKey = poolKey;
            _targeting = targeting;
        }

        public float Cooldown => _cooldown;

        public void Activate(ISkillContext ctx)
        {
            // WHY: SelfTargetStrategy는 타깃을 반환하지 않으며, 오라는 오너를 추종하는 지속 히트박스로 발동한다
            _targeting.Select(ctx.Query, ctx.OwnerPosition);

            AttackRequest request = new()
            {
                Prefab = _prefab,
                PoolKey = _poolKey,
                Origin = ctx.OwnerPosition,
                Rotation = ctx.OwnerRotation,
                MoveDirection = Vector3.zero,
                Speed = 0f,
                FollowOwner = ctx.Owner,
                Lifetime = _lifetime,
                ReHitInterval = _reHitInterval,
                ReleaseOnHit = false,
                Policy = HitPolicy.EveryInterval,
                Damage = _damage,
                TargetTeam = _targetTeam,
            };

            ctx.Spawner.Spawn(request);
        }
    }
}
