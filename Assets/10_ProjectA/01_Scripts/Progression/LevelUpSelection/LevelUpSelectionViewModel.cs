using System;
using System.Collections.Generic;
using R3;

namespace O2un.Progression
{
    public sealed class LevelUpSelectionViewModel : IDisposable
    {
        private readonly ReactiveProperty<bool> _isVisible = new(false);
        public ReadOnlyReactiveProperty<bool> IsVisible => _isVisible;

        private readonly ReactiveProperty<IReadOnlyList<string>> _candidateLabels = new(Array.Empty<string>());
        public ReadOnlyReactiveProperty<IReadOnlyList<string>> CandidateLabels => _candidateLabels;

        private readonly Subject<int> _onCandidateChosen = new();
        public Observable<int> OnCandidateChosen => _onCandidateChosen;

        public void ShowCandidates(IReadOnlyList<string> labels)
        {
            _candidateLabels.Value = labels;
            _isVisible.Value = true;
        }

        public void Hide()
        {
            _isVisible.Value = false;
        }

        public void ChooseCandidate(int index)
        {
            _onCandidateChosen.OnNext(index);
        }

        public void Dispose()
        {
            _isVisible.Dispose();
            _candidateLabels.Dispose();
            _onCandidateChosen.Dispose();
        }
    }
}
