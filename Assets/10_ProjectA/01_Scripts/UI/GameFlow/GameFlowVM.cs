using System;
using O2un.Manager;
using R3;

namespace O2un.UI
{
    public sealed class GameFlowVM : IDisposable
    {
        private readonly IGameManager _gameManager;
        private readonly CompositeDisposable _disposables = new();

        private readonly ReactiveProperty<bool> _showStart = new();
        private readonly ReactiveProperty<bool> _showHud = new();
        private readonly ReactiveProperty<bool> _showVictory = new();
        private readonly ReactiveProperty<bool> _showDefeat = new();
        private readonly ReactiveProperty<string> _resultText = new(string.Empty);

        public GameFlowVM(IGameManager gameManager)
        {
            _gameManager = gameManager;

            _gameManager.CurrentState
                    .Subscribe(OnStateChanged)
                    .AddTo(_disposables);
        }

        public ReadOnlyReactiveProperty<bool> ShowStart => _showStart;
        public ReadOnlyReactiveProperty<bool> ShowHud => _showHud;
        public ReadOnlyReactiveProperty<bool> ShowVictory => _showVictory;
        public ReadOnlyReactiveProperty<bool> ShowDefeat => _showDefeat;
        public ReadOnlyReactiveProperty<string> ResultText => _resultText;

        public void OnStartClicked()
        {
            _gameManager.StartGame();
        }

        public void OnRestartClicked()
        {
            _gameManager.Restart();
        }

        private void OnStateChanged(GameState state)
        {
            _showStart.Value = state == GameState.Idle;
            _showHud.Value = state == GameState.Playing || state == GameState.Paused;
            _showVictory.Value = state == GameState.Victory;
            _showDefeat.Value = state == GameState.Defeat;

            if (state == GameState.Victory || state == GameState.Defeat)
            {
                _resultText.Value = BuildResultText();
            }
        }

        private string BuildResultText()
        {
            return $"Wave {_gameManager.ReachedWave}/{_gameManager.TotalWaves}\n"
                    + $"Level {_gameManager.Level}\n"
                    + $"Kills {_gameManager.KillCount.CurrentValue}";
        }

        public void Dispose()
        {
            _disposables.Dispose();
            _showStart.Dispose();
            _showHud.Dispose();
            _showVictory.Dispose();
            _showDefeat.Dispose();
            _resultText.Dispose();
        }
    }
}
