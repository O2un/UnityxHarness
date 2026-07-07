using O2un.AI;
using UnityEngine;

namespace O2un.Actors
{
    public sealed class NpcActor : Actor
    {
        private readonly BaseEnemyAI _ai;
        private readonly EnemyBlackboard _blackboard;
        private readonly CharacterMover _mover;
        private readonly IActorQuery _query;

        public override ActorType Type => ActorType.Enemy;

        public NpcActor(
            BaseEnemyAI ai,
            EnemyBlackboard blackboard,
            CharacterMover mover,
            ActorView view,
            IActorRegistry registry,
            IActorQuery query)
            : base(view, registry)
        {
            _ai = ai;
            _blackboard = blackboard;
            _mover = mover;
            _query = query;
        }

        public override void Tick(float dt)
        {
            UpdateTarget();
            _ai.Tick(dt);
            ApplyMovement();
        }

        private void UpdateTarget()
        {
            _blackboard.SelfPosition = View.transform.position;

            IActor player = _query.Player;
            if (null == player)
            {
                _blackboard.HasTarget = false;
                return;
            }

            _blackboard.HasTarget = true;
            _blackboard.TargetPosition = player.Transform.position;
        }

        private void ApplyMovement()
        {
            Vector3 velocity = _mover.Velocity.CurrentValue;
            View.Move(velocity);

            Transform t = View.transform;
            Vector3 pos = t.position;
            pos.y = 0f;
            t.position = pos;

            if (velocity.sqrMagnitude > 0f)
            {
                View.RotateTo(_mover.TargetRotation, _mover.RotationSpeed);
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            _mover.Dispose();
        }
    }
}
