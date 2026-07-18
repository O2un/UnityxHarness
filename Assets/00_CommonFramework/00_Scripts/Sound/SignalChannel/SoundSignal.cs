using UnityEngine;

namespace O2un.Sound
{
    public readonly struct SoundSignal
    {
        public readonly Vector2 Position;
        public readonly SoundKind Kind;
        public readonly float Intensity;

        public SoundSignal(Vector2 position, SoundKind kind, float intensity = 1f)
        {
            Position = position;
            Kind = kind;
            Intensity = intensity;
        }
    }
}
