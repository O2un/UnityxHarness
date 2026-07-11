using System;
using O2un.Actors;
using O2un.Combat;
using O2un.DataStore;
using O2un.Progression;
using R3;
using VContainer.Unity;

namespace O2un.Manager
{
    public enum GameState
    {
        Idle,
        Playing,
        Paused,
        Victory,
        Defeat,
    }

    public interface IGameManager
    {
        ReadOnlyReactiveProperty<GameState> CurrentState { get; }
        ReadOnlyReactiveProperty<int> KillCount { get; }
        int ReachedWave { get; }
        int TotalWaves { get; }
        int Level { get; }
        void StartGame();
        void PauseGame();
        void ResumeGame();
        void EndGame(bool isVictory);
        void Restart();
    }

    public sealed class GameManager : IGameManager, IInitializable, IDisposable
    {
        private readonly EnemySpawnManager _spawnManager;
        private readonly PlayerHealthAdapter _playerHealth;
        private readonly IEnemyKillEvent _killEvent;
        private readonly IExperienceReader _experienceReader;
        private readonly IExperienceWriter _experienceWriter;
        private readonly IPlayerDataReader _playerReader;
        private readonly IPlayerDataWriter _playerWriter;

        private readonly ReactiveProperty<GameState> _currentState = new(GameState.Idle);
        private readonly ReactiveProperty<int> _killCount = new(0);
        private readonly CompositeDisposable _disposables = new();

        public GameManager(
            EnemySpawnManager spawnManager,
            PlayerHealthAdapter playerHealth,
            IEnemyKillEvent killEvent,
            IExperienceReader experienceReader,
            IExperienceWriter experienceWriter,
            IPlayerDataReader playerReader,
            IPlayerDataWriter playerWriter)
        {
            _spawnManager = spawnManager;
            _playerHealth = playerHealth;
            _killEvent = killEvent;
            _experienceReader = experienceReader;
            _experienceWriter = experienceWriter;
            _playerReader = playerReader;
            _playerWriter = playerWriter;
        }

        public ReadOnlyReactiveProperty<GameState> CurrentState => _currentState;
        public ReadOnlyReactiveProperty<int> KillCount => _killCount;
        public int ReachedWave => _spawnManager.ReachedWave;
        public int TotalWaves => _spawnManager.TotalWaves;
        public int Level => _experienceReader.CurrentLevel.CurrentValue;

        public void Initialize()
        {
            _spawnManager.OnCleared
                    .Subscribe(_ => EndGame(true))
                    .AddTo(_disposables);

            _playerHealth.OnDeath
                    .Subscribe(_ => EndGame(false))
                    .AddTo(_disposables);

            _killEvent.OnKilled
                    .Subscribe(_ => OnEnemyKilled())
                    .AddTo(_disposables);

            _currentState
                    .Subscribe(state => UnityEngine.Time.timeScale = state == GameState.Playing ? 1f : 0f)
                    .AddTo(_disposables);
        }

        public void StartGame()
        {
            if (_currentState.Value != GameState.Idle) return;
            _currentState.Value = GameState.Playing;
            _spawnManager.Begin();
        }

        public void PauseGame()
        {
            if (_currentState.Value != GameState.Playing) return;
            _currentState.Value = GameState.Paused;
        }

        public void ResumeGame()
        {
            if (_currentState.Value != GameState.Paused) return;
            _currentState.Value = GameState.Playing;
        }

        public void EndGame(bool isVictory)
        {
            if (_currentState.Value != GameState.Playing) return;
            _currentState.Value = isVictory ? GameState.Victory : GameState.Defeat;
        }

        public void Restart()
        {
            _spawnManager.Reset();
            _experienceWriter.Reset();
            _playerWriter.SetCurrentHP(_playerReader.MaxHP.CurrentValue);
            _killCount.Value = 0;
            _currentState.Value = GameState.Idle;
        }

        private void OnEnemyKilled()
        {
            if (_currentState.Value != GameState.Playing) return;
            _killCount.Value++;
        }

        public void Dispose()
        {
            UnityEngine.Time.timeScale = 1f;
            _disposables.Dispose();
            _currentState.Dispose();
            _killCount.Dispose();
        }
    }
}
