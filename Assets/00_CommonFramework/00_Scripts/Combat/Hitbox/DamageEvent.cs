namespace O2un.Combat
{
    public readonly struct DamageEvent
    {
        public readonly IDamageable Target;
        public readonly int Damage;

        public DamageEvent(IDamageable target, int damage)
        {
            Target = target;
            Damage = damage;
        }
    }
}
