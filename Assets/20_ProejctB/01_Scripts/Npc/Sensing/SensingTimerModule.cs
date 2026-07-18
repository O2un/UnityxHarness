using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    public sealed class SensingTimerModule
    {
        private readonly float _interval;

        private float _elapsed;

        public SensingTimerModule(float interval, float phaseOffset = 0f)
        {
            _interval = Mathf.Max(0.0001f, interval);
            _elapsed = Mathf.Clamp(phaseOffset, 0f, _interval);
        }

        public float Interval => _interval;

        public bool TryConsume(float dt, out float elapsedSinceLastSensing)
        {
            _elapsed += dt;
            if (_elapsed < _interval)
            {
                elapsedSinceLastSensing = 0f;
                return false;
            }

            elapsedSinceLastSensing = _elapsed;
            _elapsed = 0f;
            return true;
        }
    }
}
