using System;
using R3;
using UnityEngine;

namespace O2un.Progression
{
    public sealed class ExperienceModule : IExperienceReader, IExperienceWriter, IDisposable
    {
        private readonly AnimationCurve _requiredExpCurve;

        public ExperienceModule(AnimationCurve requiredExpCurve)
        {
            _requiredExpCurve = requiredExpCurve;
        }

        private readonly ReactiveProperty<int> _currentExp = new(0);
        private readonly ReactiveProperty<int> _currentLevel = new(1);
        private readonly Subject<LevelUpEvent> _onLevelUp = new();

        public ReadOnlyReactiveProperty<int> CurrentExp => _currentExp;
        public ReadOnlyReactiveProperty<int> CurrentLevel => _currentLevel;
        public Observable<LevelUpEvent> OnLevelUp => _onLevelUp;

        public void Gain(int amount)
        {
            if (0 >= amount)
            {
                return;
            }

            _currentExp.Value += amount;

            while (_currentExp.Value >= RequiredExp(_currentLevel.Value))
            {
                int required = RequiredExp(_currentLevel.Value);
                _currentExp.Value -= required;
                _currentLevel.Value += 1;
                _onLevelUp.OnNext(new LevelUpEvent(_currentLevel.Value));
            }
        }

        public void Reset()
        {
            _currentExp.Value = 0;
            _currentLevel.Value = 1;
        }

        private int RequiredExp(int level)
        {
            return Math.Max(1, Mathf.RoundToInt(_requiredExpCurve.Evaluate(level)));
        }

        public void Dispose()
        {
            _currentExp.Dispose();
            _currentLevel.Dispose();
            _onLevelUp.Dispose();
        }
    }
}
