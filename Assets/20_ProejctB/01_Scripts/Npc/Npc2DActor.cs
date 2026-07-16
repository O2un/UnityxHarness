using O2un.Actors;
using O2un.Combat;

namespace O2un.ProjectB.Platformer
{
    public sealed class Npc2DActor : Actor<NpcView>
    {
        private readonly EnemyHealth _health;

        public override ActorType Type => ActorType.Enemy;
        public EnemyHealth Health => _health;

        public Npc2DActor(NpcView view, IActorRegistry registry, EnemyHealth health)
            : base(view, registry)
        {
            _health = health;
        }

        public override void Tick(float dt)
        {
        }

        public override void Dispose()
        {
            _health.Dispose();
            base.Dispose();
        }
    }
}
