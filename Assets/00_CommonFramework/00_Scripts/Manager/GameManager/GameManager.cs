using System;
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
        void StartGame();
        void PauseGame();
        void ResumeGame();
        void EndGame(bool isVictory);
    }

    public sealed class GameManager : IGameManager, IInitializable, IDisposable
    {
        private readonly ReactiveProperty<GameState> _currentState = new(GameState.Idle);
        public ReadOnlyReactiveProperty<GameState> CurrentState => _currentState;

        public void Initialize() { }

        public void StartGame()
        {
            if (_currentState.Value != GameState.Idle) return;
            _currentState.Value = GameState.Playing;
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

        public void Dispose()
        {
            _currentState.Dispose();
        }
    }
}
