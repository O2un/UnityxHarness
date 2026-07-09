using O2un.Actors;
using UnityEngine;

namespace O2un.Combat
{
    public sealed class ProjectileSkill : ISkillDefinition
    {
        private readonly string _skillId;
        private readonly SkillStats _stats;
        private readonly ITargetStrategy _targeting;

        public ProjectileSkill(
            string skillId,
            SkillStats stats,
            ITargetStrategy targeting)
        {
            _skillId = skillId;
            _stats = stats;
            _targeting = targeting;
        }

        public string SkillId => _skillId;
        public int Level => _stats.Level;
        public float Cooldown => _stats.Cooldown;

        public bool ApplyUpgrade(SkillUpgradeData upgrade)
        {
            return _stats.ApplyUpgrade(upgrade);
        }

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
                Prefab = _stats.HitboxPrefab,
                PoolKey = _stats.PoolKey,
                Origin = origin,
                Rotation = Quaternion.LookRotation(direction, Vector3.up),
                MoveDirection = direction,
                Speed = _stats.Speed,
                FollowOwner = null,
                Lifetime = _stats.Lifetime,
                ReHitInterval = 0f,
                ReleaseOnHit = true,
                Policy = HitPolicy.OncePerTarget,
                Damage = _stats.Damage,
                TargetTeam = _stats.TargetTeam,
            };

            ctx.Spawner.Spawn(request);
        }
    }
}
