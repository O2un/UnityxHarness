using O2un.AI;
using UnityEngine;

namespace O2un.Actors
{
    public sealed class RecoverState : IState, IStateProgress
    {
        private readonly CharacterMover _mover;
        private readonly float _duration;

        private float _elapsed;

        public RecoverState(CharacterMover mover, float duration)
        {
            _mover = mover;
            _duration = duration;
        }

        public bool IsComplete => _elapsed >= _duration;

        public void Enter()
        {
            _elapsed = 0f;
            _mover.SetDirection(Vector3.zero);
        }

        public void Tick(float dt)
        {
            _elapsed += dt;
        }

        public void Exit() { }
    }
}
