using System;
using UnityEngine;

namespace O2un.Feedback
{
    [Serializable]
    public sealed class HitFeedbackProfile
    {
        [SerializeField] private float _hitStopDuration = 0.06f;
        [SerializeField] private float _hitStopTimeScale = 0.05f;
        [SerializeField] private float _shakeForce = 0.4f;
        [SerializeField] private AnimationCurve _damageToIntensity = AnimationCurve.Linear(0f, 1f, 50f, 2f);

        public HitFeedbackProfile()
        {
        }

        public HitFeedbackProfile(float hitStopDuration, float shakeForce)
        {
            _hitStopDuration = hitStopDuration;
            _shakeForce = shakeForce;
        }

        public float HitStopDuration => _hitStopDuration;
        public float HitStopTimeScale => _hitStopTimeScale;
        public float ShakeForce => _shakeForce;

        public float EvaluateIntensity(int damage)
        {
            if (null == _damageToIntensity || 0 == _damageToIntensity.length)
            {
                return 1f;
            }

            return _damageToIntensity.Evaluate(damage);
        }
    }
}
