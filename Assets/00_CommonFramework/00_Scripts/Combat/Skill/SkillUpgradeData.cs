namespace O2un.Combat
{
    public readonly struct SkillUpgradeData
    {
        public SkillUpgradeData(
            int level,
            float cooldownDelta,
            int damageDelta,
            float lifetimeDelta,
            float speedDelta,
            float rangeDelta,
            float reHitIntervalDelta)
        {
            Level = level;
            CooldownDelta = cooldownDelta;
            DamageDelta = damageDelta;
            LifetimeDelta = lifetimeDelta;
            SpeedDelta = speedDelta;
            RangeDelta = rangeDelta;
            ReHitIntervalDelta = reHitIntervalDelta;
        }

        public int Level { get; }
        public float CooldownDelta { get; }
        public int DamageDelta { get; }
        public float LifetimeDelta { get; }
        public float SpeedDelta { get; }
        public float RangeDelta { get; }
        public float ReHitIntervalDelta { get; }
    }
}
