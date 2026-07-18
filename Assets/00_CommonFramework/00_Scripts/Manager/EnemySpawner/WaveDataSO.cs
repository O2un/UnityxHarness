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

    // 기존 에셋에 필드가 없어 0으로 역직렬화되므로 Time이 첫 번째여야 한다.
    public enum SpawnTriggerMode
    {
        Time,
        KillBased,
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
        [SerializeField] private SpawnTriggerMode _triggerMode;
        [SerializeField] private List<WaveEntry> _waves = new();

        public SpawnTriggerMode TriggerMode => _triggerMode;
        public IReadOnlyList<WaveEntry> Waves => _waves;
    }
}
