using O2un.Actors;
using UnityEngine;

namespace O2un.Combat
{
    public sealed class AuraFieldSkill : ISkillDefinition
    {
        private readonly string _skillId;
        private readonly SkillStats _stats;
        private readonly ITargetStrategy _targeting;

        public AuraFieldSkill(
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
            // WHY: SelfTargetStrategy는 타깃을 반환하지 않으며, 오라는 오너를 추종하는 지속 히트박스로 발동한다
            _targeting.Select(ctx.Query, ctx.OwnerPosition);

            AttackRequest request = new()
            {
                Prefab = _stats.HitboxPrefab,
                PoolKey = _stats.PoolKey,
                Origin = ctx.OwnerPosition,
                Rotation = ctx.OwnerRotation,
                MoveDirection = Vector3.zero,
                Speed = 0f,
                FollowOwner = ctx.Owner,
                Lifetime = _stats.Lifetime,
                ReHitInterval = _stats.ReHitInterval,
                ReleaseOnHit = false,
                Policy = HitPolicy.EveryInterval,
                Damage = _stats.Damage,
                TargetTeam = _stats.TargetTeam,
            };

            ctx.Spawner.Spawn(request);
        }
    }
}
