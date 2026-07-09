using System.Collections.Generic;
using O2un.Actors;
using O2un.Combat;
using R3;
using UnityEngine;
using VContainer;

namespace O2un.Progression
{
    public sealed class LevelUpSelectionContext : MonoBehaviour
    {
        [SerializeField] private LevelUpSelectionView _view;

        private IActorQuery _actorQuery;
        private LevelUpSkillPoolSO _pool;
        private LevelUpSelectionViewModel _vm;
        private readonly CompositeDisposable _disposables = new();

        private int _pendingCount;
        private List<LevelUpSkillCandidate> _currentCandidates;

        private readonly struct LevelUpSkillCandidate
        {
            private readonly SkillDefinitionSO _learnSkill;
            private readonly SkillUpgradeSO _upgrade;

            public LevelUpSkillCandidate(SkillDefinitionSO learnSkill)
            {
                _learnSkill = learnSkill;
                _upgrade = null;
            }

            public LevelUpSkillCandidate(SkillUpgradeSO upgrade)
            {
                _learnSkill = null;
                _upgrade = upgrade;
            }

            public string Label => null != _upgrade ? _upgrade.DisplayName : _learnSkill.name;

            public bool ApplyTo(IPlayerSkillReceiver receiver)
            {
                if (null != _upgrade)
                {
                    return receiver.ApplySkillUpgrade(_upgrade.SkillId, _upgrade.Build());
                }

                return receiver.AcquireOrUpgradeSkill(_learnSkill.Build());
            }
        }

        [Inject]
        public void Init(IExperienceReader experienceReader, IActorQuery actorQuery, LevelUpSkillPoolSO pool)
        {
            _actorQuery = actorQuery;
            _pool = pool;

            _vm = new LevelUpSelectionViewModel();
            _view.Bind(_vm);

            experienceReader.OnLevelUp.Subscribe(_ => OnLevelUp()).AddTo(_disposables);
            _vm.OnCandidateChosen.Subscribe(OnCandidateChosen).AddTo(_disposables);
        }

        private void OnLevelUp()
        {
            _pendingCount++;
            if (1 == _pendingCount)
            {
                Time.timeScale = 0f;
                ShowNext();
            }
        }

        private void ShowNext()
        {
            _currentCandidates = ExtractRandomCandidates();
            if (0 == _currentCandidates.Count)
            {
                CompleteCurrent();
                return;
            }

            string[] labels = new string[_currentCandidates.Count];
            for (int i = 0; i < _currentCandidates.Count; i++)
            {
                labels[i] = _currentCandidates[i].Label;
            }

            _vm.ShowCandidates(labels);
        }

        private void OnCandidateChosen(int index)
        {
            if (null == _currentCandidates || index < 0 || index >= _currentCandidates.Count)
            {
                return;
            }

            if (_actorQuery.Player is IPlayerSkillReceiver receiver)
            {
                _currentCandidates[index].ApplyTo(receiver);
            }

            CompleteCurrent();
        }

        private void CompleteCurrent()
        {
            _pendingCount--;
            if (_pendingCount > 0)
            {
                ShowNext();
                return;
            }

            _vm.Hide();
            Time.timeScale = 1f;
        }

        private List<LevelUpSkillCandidate> ExtractRandomCandidates()
        {
            List<LevelUpSkillCandidate> working = CollectAvailableCandidates();
            int count = Mathf.Min(3, working.Count);
            List<LevelUpSkillCandidate> result = new(count);

            for (int i = 0; i < count; i++)
            {
                int index = Random.Range(0, working.Count);
                result.Add(working[index]);
                working.RemoveAt(index);
            }

            return result;
        }

        private List<LevelUpSkillCandidate> CollectAvailableCandidates()
        {
            List<LevelUpSkillCandidate> result = new();
            if (_actorQuery.Player is not IPlayerSkillReceiver receiver)
            {
                return result;
            }

            IReadOnlyList<SkillDefinitionSO> learnCandidates = _pool.LearnCandidates;
            for (int i = 0; i < learnCandidates.Count; i++)
            {
                SkillDefinitionSO skill = learnCandidates[i];
                if (null == skill)
                {
                    continue;
                }

                if (true == string.IsNullOrEmpty(skill.SkillId) || false == receiver.HasSkill(skill.SkillId))
                {
                    result.Add(new LevelUpSkillCandidate(skill));
                }
            }

            IReadOnlyList<SkillUpgradeSO> upgradeCandidates = _pool.UpgradeCandidates;
            for (int i = 0; i < upgradeCandidates.Count; i++)
            {
                SkillUpgradeSO upgrade = upgradeCandidates[i];
                if (null == upgrade || true == string.IsNullOrEmpty(upgrade.SkillId))
                {
                    continue;
                }

                if (true == receiver.HasSkill(upgrade.SkillId) && upgrade.Level > receiver.GetSkillLevel(upgrade.SkillId))
                {
                    result.Add(new LevelUpSkillCandidate(upgrade));
                }
            }

            return result;
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
            _vm?.Dispose();
        }
    }
}
