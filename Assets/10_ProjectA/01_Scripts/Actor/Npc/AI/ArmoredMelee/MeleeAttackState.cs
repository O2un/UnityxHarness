using O2un.AI;
using UnityEngine;

namespace O2un.Actors
{
    public sealed class MeleeAttackState : IState, IStateProgress
    {
        private readonly CharacterMover _mover;
        private readonly float _attackDuration;
        private float _elapsed;

        public MeleeAttackState(CharacterMover mover, float attackDuration)
        {
            _mover = mover;
            _attackDuration = attackDuration;
        }

        public bool IsComplete => _elapsed >= _attackDuration;

        public void Enter()
        {
            _elapsed = 0f;
            _mover.SetDirection(Vector3.zero);
        }

        public void Tick(float deltaTime)
        {
            _elapsed += deltaTime;
            _mover.SetDirection(Vector3.zero);
        }

        public void Exit()
        {
            _mover.SetDirection(Vector3.zero);
        }
    }
}
