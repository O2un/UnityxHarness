using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace O2un.Manager
{
    public sealed class KillWaveModule
    {
        private sealed class WaveGroup
        {
            public int Number;
            public readonly List<SpawnRequest> Requests = new();
        }

        private static readonly IReadOnlyList<SpawnRequest> EMPTY = new List<SpawnRequest>();

        private readonly List<WaveGroup> _groups;
        private readonly List<string> _requiredKeys;
        private int _currentIndex = -1;
        private int _remaining;

        public KillWaveModule(IReadOnlyList<WaveEntry> waves)
        {
            _groups = BuildGroups(waves);
            _requiredKeys = waves.Select(wave => wave.AddressableKey).Distinct().ToList();
            TotalWaves = _groups.Count > 0 ? _groups[^1].Number : 0;
        }

        public IReadOnlyList<string> RequiredKeys => _requiredKeys;
        public int TotalWaves { get; }
        public int ReachedWave { get; private set; }
        public bool IsCurrentWaveCleared => _remaining <= 0;

        public bool TryAdvance(out IReadOnlyList<SpawnRequest> spawns)
        {
            if (_currentIndex >= _groups.Count - 1)
            {
                spawns = EMPTY;
                return false;
            }

            _currentIndex++;
            WaveGroup group = _groups[_currentIndex];
            _remaining = 0;
            ReachedWave = group.Number;
            spawns = group.Requests;
            return true;
        }

        // 스폰 실패분만큼 잔존 수가 부풀어 진행이 멎는 것을 막기 위해 실제 스폰 수를 받는다.
        public void NotifySpawned(int count)
        {
            if (0 < count)
            {
                _remaining += count;
            }
        }

        public void NotifyKilled()
        {
            if (0 < _remaining)
            {
                _remaining--;
            }
        }

        public void Reset()
        {
            _currentIndex = -1;
            _remaining = 0;
            ReachedWave = 0;
        }

        private static List<WaveGroup> BuildGroups(IReadOnlyList<WaveEntry> waves)
        {
            Dictionary<int, WaveGroup> lookup = new();

            for (int w = 0; w < waves.Count; w++)
            {
                WaveEntry wave = waves[w];
                int waveNumber = wave.WaveNumber > 0 ? wave.WaveNumber : w + 1;

                if (SpawnPlacement.PlayerRadius == wave.Placement)
                {
                    Debug.LogError(
                            $"[KillWaveModule] 웨이브 {waveNumber}의 '{wave.AddressableKey}'가 PlayerRadius 배치입니다. " +
                            "KillBased 모드는 고정 스폰 지점만 지원하므로 Fixed로 처리합니다.");
                }

                SpawnRequest request = SpawnRequest.Fixed(wave.AddressableKey, wave.Position);

                if (false == lookup.TryGetValue(waveNumber, out WaveGroup group))
                {
                    group = new WaveGroup { Number = waveNumber };
                    lookup.Add(waveNumber, group);
                }

                for (int i = 0; i < wave.Count; i++)
                {
                    group.Requests.Add(request);
                }
            }

            return lookup.Values.OrderBy(group => group.Number).ToList();
        }
    }
}
