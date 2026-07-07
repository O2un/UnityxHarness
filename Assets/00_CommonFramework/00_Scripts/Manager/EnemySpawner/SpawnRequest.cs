using UnityEngine;

namespace O2un.Manager
{
    public readonly struct SpawnRequest
    {
        public readonly string Key;
        public readonly Vector3 Position;

        public SpawnRequest(string key, Vector3 position)
        {
            Key = key;
            Position = position;
        }
    }
}
