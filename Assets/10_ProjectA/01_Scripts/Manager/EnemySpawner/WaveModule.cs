using System;
using System.Collections.Generic;
using System.Linq;

namespace O2un.Manager
{
    public sealed class WaveModule
    {
        private readonly struct ScheduledSpawn
        {
            public readonly float Time;
            public readonly int WaveIndex;
            public readonly SpawnRequest Request;

            public ScheduledSpawn(float time, int waveIndex, SpawnRequest request)
            {
                Time = time;
                WaveIndex = waveIndex;
                Request = request;
            }
        }

        private readonly List<ScheduledSpawn> _timeline;
        private readonly List<string> _requiredKeys;
        private readonly List<SpawnRequest> _buffer = new();
        private int _nextIndex;

        public WaveModule(IReadOnlyList<WaveEntry> waves, int? seed = null)
        {
            Random rng = seed.HasValue ? new Random(seed.Value) : new Random();
            _timeline = BuildTimeline(waves, rng);
            _requiredKeys = waves.Select(wave => wave.AddressableKey).Distinct().ToList();
            TotalWaves = waves.Count;
        }

        public IReadOnlyList<string> RequiredKeys => _requiredKeys;
        public bool IsExhausted => _nextIndex >= _timeline.Count;
        public int TotalWaves { get; }
        public int ReachedWave { get; private set; }

        public IReadOnlyList<SpawnRequest> GetSpawnsAt(float time)
        {
            _buffer.Clear();
            while (_nextIndex < _timeline.Count && _timeline[_nextIndex].Time <= time)
            {
                ScheduledSpawn spawn = _timeline[_nextIndex];
                _buffer.Add(spawn.Request);
                if (spawn.WaveIndex + 1 > ReachedWave)
                {
                    ReachedWave = spawn.WaveIndex + 1;
                }
                _nextIndex++;
            }

            return _buffer;
        }

        public void Reset()
        {
            _nextIndex = 0;
            ReachedWave = 0;
        }

        private static List<ScheduledSpawn> BuildTimeline(IReadOnlyList<WaveEntry> waves, Random rng)
        {
            List<ScheduledSpawn> timeline = new();

            for (int w = 0; w < waves.Count; w++)
            {
                WaveEntry wave = waves[w];
                SpawnRequest request = SpawnRequest.FromEntry(wave);

                for (int i = 0; i < wave.Count; i++)
                {
                    float time = ResolveSpawnTime(wave, rng);
                    timeline.Add(new ScheduledSpawn(time, w, request));
                }
            }

            timeline.Sort((a, b) => a.Time.CompareTo(b.Time));
            return timeline;
        }

        private static float ResolveSpawnTime(in WaveEntry wave, Random rng)
        {
            if (SpawnTiming.NormalSpread != wave.Timing)
            {
                return wave.SpawnTime;
            }

            float start = wave.SpawnTime;
            float end = wave.EndTime > start ? wave.EndTime : start;
            float mean = (start + end) * 0.5f;
            float stdDev = (end - start) / 6f;
            float sample = GaussianSampler.Sample(rng, mean, stdDev);
            return Math.Clamp(sample, start, end);
        }
    }
}
