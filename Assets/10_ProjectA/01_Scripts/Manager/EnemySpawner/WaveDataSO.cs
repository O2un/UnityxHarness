using System;
using System.Collections.Generic;
using UnityEngine;

namespace O2un.Manager
{
    public enum SpawnPlacement
    {
        Fixed,
        PlayerRadius,
    }

    public enum SpawnTiming
    {
        Burst,
        NormalSpread,
    }

    [Serializable]
    public struct WaveEntry
    {
        public int WaveNumber;
        public string AddressableKey;
        public float SpawnTime;
        public int Count;
        public Vector3 Position;

        public SpawnPlacement Placement;
        public float MinRadius;
        public float MaxRadius;

        public SpawnTiming Timing;
        public float EndTime;
    }

    [CreateAssetMenu(fileName = "WaveData", menuName = "O2un/Enemy/WaveData")]
    public sealed class WaveDataSO : ScriptableObject
    {
        [SerializeField] private List<WaveEntry> _waves = new();

        public IReadOnlyList<WaveEntry> Waves => _waves;
    }
}
