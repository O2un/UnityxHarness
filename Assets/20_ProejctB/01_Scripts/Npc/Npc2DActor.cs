using O2un.Actors;
using O2un.Combat;

namespace O2un.ProjectB.Platformer
{
    public sealed class Npc2DActor : Actor<NpcView>, IActorTickable
    {
        private readonly EnemyHealth _health;
        private readonly Enemy2DSensorView _sensor;
        private readonly Enemy2DBlackboard _blackboard;
        private readonly PerceptionMemoryModule _memory = new();

        public override ActorType Type => ActorType.Enemy;
        public EnemyHealth Health => _health;
        public IEnemy2DBlackboard Blackboard => _blackboard;

        public Npc2DActor(NpcView view, IActorRegistry registry, EnemyHealth health, Enemy2DSensorView sensor)
            : base(view, registry)
        {
            _health = health;
            _sensor = sensor;
            _blackboard = new Enemy2DBlackboard();
        }

        public override void Tick(float dt)
        {
            if (null == _sensor)
            {
                return;
            }

            _sensor.Tick(dt);
            _memory.Tick(dt);
            UpdateBlackboard();
        }

        private void UpdateBlackboard()
        {
            SensorSnapshot snapshot = _sensor.Snapshot;

            _blackboard.SelfPosition = snapshot.SelfPosition;
            _blackboard.Facing = snapshot.Facing;
            _blackboard.HasTarget = snapshot.HasTarget;
            _blackboard.TargetPosition = snapshot.TargetPosition;
            _blackboard.IsPlayerVisible = snapshot.IsPlayerVisible;
            _blackboard.HeardSoundThisTick = snapshot.HeardSound;
            _blackboard.WasHitThisTick = snapshot.WasHit;
            _blackboard.GroundAhead = snapshot.GroundAhead;
            _blackboard.WallAhead = snapshot.WallAhead;

            if (true == snapshot.IsPlayerVisible)
            {
                _memory.Perceive(snapshot.TargetPosition);
            }
            else if (true == snapshot.HeardSound && true == snapshot.HasTarget)
            {
                _memory.Perceive(snapshot.TargetPosition);
            }

            _blackboard.LastKnownTargetPosition = _memory.LastKnownTargetPosition;
            _blackboard.HasLastKnownTargetPosition = _memory.HasLastKnownTargetPosition;
            _blackboard.TimeSincePerceived = _memory.TimeSincePerceived;
        }

        public override void Dispose()
        {
            _sensor?.Dispose();
            _health.Dispose();
            base.Dispose();
        }
    }
}
