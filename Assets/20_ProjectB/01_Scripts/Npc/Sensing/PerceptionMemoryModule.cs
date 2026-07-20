using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    public sealed class PerceptionMemoryModule
    {
        private Vector2 _lastKnownTargetPosition;
        private bool _hasLastKnown;
        private float _timeSincePerceived;

        public Vector2 LastKnownTargetPosition => _lastKnownTargetPosition;
        public bool HasLastKnownTargetPosition => _hasLastKnown;
        public float TimeSincePerceived => _timeSincePerceived;

        public void Tick(float dt)
        {
            _timeSincePerceived += dt;
        }

        public void Perceive(Vector2 targetPosition)
        {
            _lastKnownTargetPosition = targetPosition;
            _hasLastKnown = true;
            _timeSincePerceived = 0f;
        }

        public void Reset()
        {
            _lastKnownTargetPosition = Vector2.zero;
            _hasLastKnown = false;
            _timeSincePerceived = 0f;
        }
    }
}
