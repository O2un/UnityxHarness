using UnityEngine;

namespace O2un.Feedback
{
    public sealed class HitStopModule
    {
        public const float MAX_DURATION = 0.2f;

        private float _remaining;
        private float _timeScale = 1f;

        public bool IsActive => 0f < _remaining;
        public float TimeScale => _timeScale;

        public void Push(float duration, float timeScale)
        {
            float clamped = Mathf.Clamp(duration, 0f, MAX_DURATION);

            // IsActive 판정이 이전 상태 기준이어야 하므로 _remaining 갱신보다 먼저 합성한다.
            _timeScale = IsActive ? Mathf.Min(_timeScale, timeScale) : timeScale;
            _remaining = Mathf.Max(_remaining, clamped);
        }

        public bool Tick(float unscaledDeltaTime)
        {
            if (false == IsActive)
            {
                return false;
            }

            _remaining -= unscaledDeltaTime;
            if (0f < _remaining)
            {
                return false;
            }

            _remaining = 0f;
            return true;
        }
    }
}
