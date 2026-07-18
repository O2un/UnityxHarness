using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    public interface IAttackState
    {
        bool IsAttackFinished { get; }
    }

    public sealed class AttackState : TimedState, IAttackState
    {
        private readonly IEnemy2DBlackboard _blackboard;
        private readonly Npc2DMover _mover;
        private readonly IAttackHook _hook;
        private readonly float _attackRange;

        public AttackState(IEnemy2DBlackboard blackboard, Npc2DMover mover, IAttackHook hook, float attackRange)
        {
            _blackboard = blackboard;
            _mover = mover;
            _hook = hook;
            _attackRange = attackRange;
        }

        public bool IsAttackFinished => _hook.IsFinished;

        protected override void OnEnter()
        {
            _mover.Stop();
            FaceTarget();
            _hook.Begin();
        }

        protected override void OnTick(float dt)
        {
            _mover.Stop();
            _hook.Tick(dt);

            if (false == _hook.IsFinished)
            {
                return;
            }

            if (false == IsTargetInRange())
            {
                return;
            }

            FaceTarget();
            _hook.Begin();
        }

        private void FaceTarget()
        {
            if (false == _blackboard.HasTarget)
            {
                return;
            }

            _mover.SetFacing(_blackboard.TargetPosition.x - _blackboard.SelfPosition.x);
        }

        private bool IsTargetInRange()
        {
            if (false == _blackboard.HasTarget)
            {
                return false;
            }

            Vector2 delta = _blackboard.TargetPosition - _blackboard.SelfPosition;
            return delta.sqrMagnitude <= _attackRange * _attackRange;
        }
    }
}
