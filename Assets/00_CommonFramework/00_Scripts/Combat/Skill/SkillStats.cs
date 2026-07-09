using O2un.Actors;
using UnityEngine;

namespace O2un.Combat
{
    public sealed class SkillStats
    {
        public SkillStats(
            int level,
            float cooldown,
            int damage,
            float lifetime,
            AttackHitboxView hitboxPrefab,
            string poolKey,
            ActorType targetTeam,
            float speed = 0f,
            float range = 0f,
            float reHitInterval = 0f)
        {
            Level = level;
            Cooldown = cooldown;
            Damage = damage;
            Lifetime = lifetime;
            HitboxPrefab = hitboxPrefab;
            PoolKey = poolKey;
            TargetTeam = targetTeam;
            Speed = speed;
            Range = range;
            ReHitInterval = reHitInterval;
        }

        public int Level { get; private set; }
        public float Cooldown { get; private set; }
        public int Damage { get; private set; }
        public float Lifetime { get; private set; }
        public AttackHitboxView HitboxPrefab { get; }
        public string PoolKey { get; }
        public ActorType TargetTeam { get; }
        public float Speed { get; private set; }
        public float Range { get; private set; }
        public float ReHitInterval { get; private set; }

        public bool ApplyUpgrade(SkillUpgradeData upgrade)
        {
            if (upgrade.Level <= Level)
            {
                return false;
            }

            Level = upgrade.Level;
            Cooldown = Mathf.Max(0f, Cooldown + upgrade.CooldownDelta);
            Damage = Mathf.Max(0, Damage + upgrade.DamageDelta);
            Lifetime = Mathf.Max(0f, Lifetime + upgrade.LifetimeDelta);
            Speed = Mathf.Max(0f, Speed + upgrade.SpeedDelta);
            Range = Mathf.Max(0f, Range + upgrade.RangeDelta);
            ReHitInterval = Mathf.Max(0f, ReHitInterval + upgrade.ReHitIntervalDelta);
            return true;
        }
    }
}
