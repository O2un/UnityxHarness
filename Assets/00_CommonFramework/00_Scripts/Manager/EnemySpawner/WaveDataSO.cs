using System;
using System.Collections.Generic;
using UnityEngine;

namespace O2un.Manager
{
    [Serializable]
    public struct WaveEntry
    {
        public string AddressableKey;
        public float SpawnTime;
        public int Count;
        public Vector3 Position;
    }

    [CreateAssetMenu(fileName = "WaveData", menuName = "O2un/Enemy/WaveData")]
    public sealed class WaveDataSO : ScriptableObject
    {
        [SerializeField] private List<WaveEntry> _waves = new();

        public IReadOnlyList<WaveEntry> Waves => _waves;
    }
}
