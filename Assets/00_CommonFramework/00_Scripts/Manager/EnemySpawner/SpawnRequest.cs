using UnityEngine;

namespace O2un.Manager
{
    public readonly struct SpawnRequest
    {
        public readonly string Key;
        public readonly Vector3 Position;
        public readonly SpawnPlacement Placement;
        public readonly float MinRadius;
        public readonly float MaxRadius;

        public SpawnRequest(string key, Vector3 position, SpawnPlacement placement, float minRadius, float maxRadius)
        {
            Key = key;
            Position = position;
            Placement = placement;
            MinRadius = minRadius;
            MaxRadius = maxRadius;
        }

        public static SpawnRequest Fixed(string key, Vector3 position)
        {
            return new SpawnRequest(key, position, SpawnPlacement.Fixed, 0f, 0f);
        }

        public static SpawnRequest FromEntry(in WaveEntry entry)
        {
            return new SpawnRequest(
                entry.AddressableKey,
                entry.Position,
                entry.Placement,
                entry.MinRadius,
                entry.MaxRadius);
        }
    }
}
