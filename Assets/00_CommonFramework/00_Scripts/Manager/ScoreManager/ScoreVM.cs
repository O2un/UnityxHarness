using System;
using R3;

namespace O2un.Manager
{
    public sealed class ScoreVM : IDisposable
    {
        public ReadOnlyReactiveProperty<int> Score { get; }

        public ScoreVM(IScoreManager scoreManager)
        {
            Score = scoreManager.Score;
        }

        public void Dispose() { }
    }
}
