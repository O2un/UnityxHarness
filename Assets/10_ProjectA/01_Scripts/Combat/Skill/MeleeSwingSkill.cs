using O2un.Actors;
using UnityEngine;

namespace O2un.Combat
{
    public sealed class MeleeSwingSkill : ISkillDefinition
    {
        private readonly string _skillId;
        private readonly SkillStats _stats;
        private readonly ITargetStrategy _targeting;

        public MeleeSwingSkill(
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
            Vector3 toTarget = target.Transform.position - origin;
            toTarget.y = 0f;

            Quaternion rotation = toTarget.sqrMagnitude > 0f
                ? Quaternion.LookRotation(toTarget, Vector3.up)
                : ctx.OwnerRotation;

            AttackRequest request = new()
            {
                Prefab = _stats.HitboxPrefab,
                PoolKey = _stats.PoolKey,
                Origin = origin + rotation * Vector3.forward * (_stats.Range * 0.5f),
                Rotation = rotation,
                MoveDirection = Vector3.zero,
                Speed = 0f,
                FollowOwner = null,
                Lifetime = _stats.Lifetime,
                ReHitInterval = 0f,
                ReleaseOnHit = false,
                Policy = HitPolicy.OncePerTarget,
                Damage = _stats.Damage,
                TargetTeam = _stats.TargetTeam,
            };

            ctx.Spawner.Spawn(request);
        }
    }
}
