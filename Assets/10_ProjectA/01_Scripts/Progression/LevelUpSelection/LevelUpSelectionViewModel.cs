using System;
using System.Collections.Generic;
using R3;
using UnityEngine;

namespace O2un.Progression
{
    public sealed class LevelUpSelectionViewModel : IDisposable
    {
        private readonly LevelUpSelectionModule _module;
        private readonly CompositeDisposable _disposables = new();
        private List<LevelUpSkillCandidate> _currentCandidates;
        private int _pendingCount;

        private readonly ReactiveProperty<bool> _isVisible = new(false);
        public ReadOnlyReactiveProperty<bool> IsVisible => _isVisible;

        private readonly ReactiveProperty<IReadOnlyList<string>> _candidateLabels = new(Array.Empty<string>());
        public ReadOnlyReactiveProperty<IReadOnlyList<string>> CandidateLabels => _candidateLabels;

        private readonly ReactiveProperty<IReadOnlyList<Sprite>> _candidateIcons = new(Array.Empty<Sprite>());
        public ReadOnlyReactiveProperty<IReadOnlyList<Sprite>> CandidateIcons => _candidateIcons;

        public LevelUpSelectionViewModel(LevelUpSelectionModule module, IExperienceReader experienceReader)
        {
            _module = module;

            experienceReader.OnLevelUp.Subscribe(_ => OnLevelUp()).AddTo(_disposables);
        }

        private void OnLevelUp()
        {
            _pendingCount++;
            if (1 == _pendingCount)
            {
                SetPaused(true);
                ShowNextSelection();
            }
        }

        private void ShowNextSelection()
        {
            _currentCandidates = _module.ExtractRandomCandidates();
            if (0 == _currentCandidates.Count)
            {
                CompleteCurrent();
                return;
            }

            string[] labels = new string[_currentCandidates.Count];
            Sprite[] icons = new Sprite[_currentCandidates.Count];
            for (int i = 0; i < _currentCandidates.Count; i++)
            {
                labels[i] = _currentCandidates[i].Label;
                icons[i] = _currentCandidates[i].Icon;
            }

            _candidateLabels.Value = labels;
            _candidateIcons.Value = icons;
            _isVisible.Value = true;
        }

        private void Hide()
        {
            _isVisible.Value = false;
            _currentCandidates = null;
        }

        public void ChooseCandidate(int index)
        {
            if (true == _module.ApplyCandidate(_currentCandidates, index))
            {
                CompleteCurrent();
            }
        }

        private void CompleteCurrent()
        {
            _pendingCount--;
            if (_pendingCount > 0)
            {
                ShowNextSelection();
                return;
            }

            Hide();
            SetPaused(false);
        }

        private void SetPaused(bool isPaused)
        {
            Time.timeScale = true == isPaused ? 0f : 1f;
        }

        public void Dispose()
        {
            _disposables.Dispose();
            _isVisible.Dispose();
            _candidateLabels.Dispose();
            _candidateIcons.Dispose();
        }
    }
}
