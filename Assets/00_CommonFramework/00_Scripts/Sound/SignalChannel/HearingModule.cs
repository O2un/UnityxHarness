using UnityEngine;

namespace O2un.Sound
{
    public sealed class HearingModule
    {
        private readonly float _hearingRadius;

        public HearingModule(float hearingRadius)
        {
            _hearingRadius = Mathf.Max(0f, hearingRadius);
        }

        public float HearingRadius => _hearingRadius;

        public bool CanHear(in SoundSignal signal, Vector2 listenerPosition)
        {
            float effectiveRadius = _hearingRadius * Mathf.Max(0f, signal.Intensity);
            if (0f >= effectiveRadius)
            {
                return false;
            }

            return (signal.Position - listenerPosition).sqrMagnitude <= effectiveRadius * effectiveRadius;
        }
    }
}
