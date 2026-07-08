using O2un.Actors;

namespace O2un.Combat
{
    public readonly struct HitboxConfig
    {
        public readonly int Damage;
        public readonly ActorType TargetTeam;
        public readonly HitPolicy Policy;
        public readonly float ReHitInterval;
        public readonly float Lifetime;

        public HitboxConfig(int damage, ActorType targetTeam, HitPolicy policy, float reHitInterval, float lifetime)
        {
            Damage = damage;
            TargetTeam = targetTeam;
            Policy = policy;
            ReHitInterval = reHitInterval;
            Lifetime = lifetime;
        }
    }
}
