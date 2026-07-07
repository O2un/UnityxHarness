using System.Collections.Generic;
using System.Linq;

namespace O2un.Manager
{
    public sealed class WaveModule
    {
        private readonly List<WaveEntry> _waves;
        private readonly List<string> _requiredKeys;
        private readonly List<SpawnRequest> _buffer = new();
        private int _nextIndex;

        public WaveModule(IReadOnlyList<WaveEntry> waves)
        {
            _waves = waves.OrderBy(wave => wave.SpawnTime).ToList();
            _requiredKeys = _waves.Select(wave => wave.AddressableKey).Distinct().ToList();
        }

        public IReadOnlyList<string> RequiredKeys => _requiredKeys;

        public IReadOnlyList<SpawnRequest> GetSpawnsAt(float time)
        {
            _buffer.Clear();
            while (_nextIndex < _waves.Count && _waves[_nextIndex].SpawnTime <= time)
            {
                WaveEntry wave = _waves[_nextIndex];
                for (int i = 0; i < wave.Count; i++)
                {
                    _buffer.Add(new SpawnRequest(wave.AddressableKey, wave.Position));
                }

                _nextIndex++;
            }

            return _buffer;
        }
    }
}
