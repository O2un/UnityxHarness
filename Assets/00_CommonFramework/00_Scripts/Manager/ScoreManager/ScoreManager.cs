using System;
using R3;
using VContainer.Unity;

namespace O2un.Manager
{
    public interface IScoreManager
    {
        ReadOnlyReactiveProperty<int> Score { get; }
        void AddScore(int basePoint);
    }

    public sealed class ScoreManager : IScoreManager, IInitializable, IDisposable
    {
        private readonly IScoreCalculator _calculator;
        private readonly ReactiveProperty<int> _score = new(0);
        private readonly CompositeDisposable _disposables = new();

        public ReadOnlyReactiveProperty<int> Score => _score;

        public ScoreManager(IScoreCalculator calculator)
        {
            _calculator = calculator;
        }

        public void Initialize() { }

        public void AddScore(int basePoint)
        {
            _score.Value += _calculator.Calculate(basePoint);
        }

        public void Dispose()
        {
            _disposables.Dispose();
            _score.Dispose();
        }
    }
}
