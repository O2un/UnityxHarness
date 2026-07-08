using O2un.Actors;

namespace O2un.Combat
{
    public interface IDamageable
    {
        ActorType Team { get; }
        void ApplyDamage(int amount);
    }
}
