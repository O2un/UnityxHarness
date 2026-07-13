#if UNITY_EDITOR
using System;
using System.Globalization;
using System.IO;
using System.Text;
using O2un.Actors;
using O2un.Combat;
using O2un.Manager;
using O2un.Progression;
using R3;
using UnityEngine;
using VContainer.Unity;

namespace O2un.BalanceProbe
{
    public sealed class BalanceProbeManager : IInitializable, ITickable, IDisposable
    {
        private readonly struct WaveWindow
        {
            public readonly int Wave;
            public readonly float Start;
            public readonly float End;

            public WaveWindow(int wave, float start, float end)
            {
                Wave = wave;
                Start = start;
                End = end;
            }
        }

        private static readonly WaveWindow[] WINDOWS =
        {
            new(1, 1f, 12f),
            new(2, 14f, 26f),
            new(3, 28f, 40f),
            new(4, 42f, 54f),
            new(5, 56f, 70f),
        };

        private sealed class WaveSample
        {
            public long DamageDealt;
            public int Kills;
            public int PeakAlive;
            public int Level = 1;
            public bool WasHit;
            public bool Visited;
        }

        private readonly IGameManager _gameManager;
        private readonly IEnemyKillEvent _enemyKill;
        private readonly IEnemyDamageSource _enemyDamage;
        private readonly IExperienceReader _experience;
        private readonly IActorQuery _actorQuery;
        private readonly PlayerHealthAdapter _playerHealth;

        private readonly WaveSample[] _samples;
        private readonly CompositeDisposable _disposables = new();

        private float _elapsed;
        private bool _running;
        private bool _exported;
        private int _lastPlayerHp;

        public BalanceProbeManager(
            IGameManager gameManager,
            IEnemyKillEvent enemyKill,
            IEnemyDamageSource enemyDamage,
            IExperienceReader experience,
            IActorQuery actorQuery,
            PlayerHealthAdapter playerHealth)
        {
            _gameManager = gameManager;
            _enemyKill = enemyKill;
            _enemyDamage = enemyDamage;
            _experience = experience;
            _actorQuery = actorQuery;
            _playerHealth = playerHealth;

            _samples = new WaveSample[WINDOWS.Length];
            for (int i = 0; i < _samples.Length; i++)
            {
                _samples[i] = new WaveSample();
            }
        }

        public void Initialize()
        {
            _lastPlayerHp = _playerHealth.CurrentHP.CurrentValue;

            _enemyKill.OnKilled.Subscribe(_ => OnKill()).AddTo(_disposables);
            _enemyDamage.OnEnemyDamaged.Subscribe(OnDamage).AddTo(_disposables);
            _playerHealth.CurrentHP.Subscribe(OnPlayerHp).AddTo(_disposables);
            _gameManager.CurrentState.Subscribe(OnStateChanged).AddTo(_disposables);
        }

        private void OnStateChanged(GameState state)
        {
            if (GameState.Idle == state)
            {
                ResetRun();
                return;
            }

            if (GameState.Playing == state)
            {
                _running = true;
                return;
            }

            if (GameState.Victory == state || GameState.Defeat == state)
            {
                _running = false;
                Export(state);
            }
        }

        private void ResetRun()
        {
            _elapsed = 0f;
            _running = false;
            _exported = false;
            _lastPlayerHp = _playerHealth.CurrentHP.CurrentValue;

            for (int i = 0; i < _samples.Length; i++)
            {
                _samples[i] = new WaveSample();
            }
        }

        public void Tick()
        {
            if (false == _running)
            {
                return;
            }

            _elapsed += Time.deltaTime;

            int index = CurrentIndex();
            if (0 > index)
            {
                return;
            }

            WaveSample sample = _samples[index];
            sample.Visited = true;
            sample.Level = _experience.CurrentLevel.CurrentValue;

            int alive = _actorQuery.GetActive(ActorType.Enemy).Count;
            if (alive > sample.PeakAlive)
            {
                sample.PeakAlive = alive;
            }
        }

        private void OnKill()
        {
            int index = CurrentIndex();
            if (0 <= index)
            {
                _samples[index].Kills++;
            }
        }

        private void OnDamage(int amount)
        {
            int index = CurrentIndex();
            if (0 <= index)
            {
                _samples[index].DamageDealt += amount;
            }
        }

        private void OnPlayerHp(int hp)
        {
            if (hp < _lastPlayerHp)
            {
                int index = CurrentIndex();
                if (0 <= index)
                {
                    _samples[index].WasHit = true;
                }
            }

            _lastPlayerHp = hp;
        }

        private int CurrentIndex()
        {
            for (int i = 0; i < WINDOWS.Length; i++)
            {
                WaveWindow window = WINDOWS[i];
                if (_elapsed >= window.Start && _elapsed <= window.End)
                {
                    return i;
                }
            }

            return -1;
        }

        private void Export(GameState endState)
        {
            if (true == _exported)
            {
                return;
            }

            _exported = true;

            string directory = Path.Combine(Directory.GetCurrentDirectory(), "BalanceLogs");
            Directory.CreateDirectory(directory);

            string fileName = $"balance-probe_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            string path = Path.Combine(directory, fileName);

            StringBuilder sb = new();
            sb.AppendLine("wave,seconds,measuredDps,kills,peakAlive,level,wasHit,visited");

            for (int i = 0; i < WINDOWS.Length; i++)
            {
                WaveWindow window = WINDOWS[i];
                WaveSample sample = _samples[i];
                float duration = window.End - window.Start;
                float dps = 0f < duration ? sample.DamageDealt / duration : 0f;

                sb.Append(window.Wave.ToString(CultureInfo.InvariantCulture)).Append(',');
                sb.Append($"{window.Start:0}-{window.End:0}").Append(',');
                sb.Append(dps.ToString("0.00", CultureInfo.InvariantCulture)).Append(',');
                sb.Append(sample.Kills.ToString(CultureInfo.InvariantCulture)).Append(',');
                sb.Append(sample.PeakAlive.ToString(CultureInfo.InvariantCulture)).Append(',');
                sb.Append(sample.Level.ToString(CultureInfo.InvariantCulture)).Append(',');
                sb.Append(sample.WasHit ? "1" : "0").Append(',');
                sb.Append(sample.Visited ? "1" : "0").Append('\n');
            }

            sb.AppendLine();
            sb.Append("# endState,").Append(endState.ToString()).Append('\n');
            sb.Append("# totalElapsedSeconds,").Append(_elapsed.ToString("0.00", CultureInfo.InvariantCulture)).Append('\n');

            File.WriteAllText(path, sb.ToString());
            Debug.Log($"[BalanceProbe] 밸런스 실측 파일 저장: {path}");
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }
    }
}
#endif
