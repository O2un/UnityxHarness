using System.Collections.Generic;
using O2un.Actors;
using O2un.Combat;
using UnityEngine;

namespace O2un.Progression
{
    public sealed class LevelUpSelectionModule
    {
        private const int CandidateCount = 3;

        private readonly IActorQuery _actorQuery;
        private readonly LevelUpSkillPoolSO _pool;

        public LevelUpSelectionModule(IActorQuery actorQuery, LevelUpSkillPoolSO pool)
        {
            _actorQuery = actorQuery;
            _pool = pool;
        }

        internal List<LevelUpSkillCandidate> ExtractRandomCandidates()
        {
            List<LevelUpSkillCandidate> working = CollectAvailableCandidates();
            int count = Mathf.Min(CandidateCount, working.Count);
            List<LevelUpSkillCandidate> result = new(count);

            for (int i = 0; i < count; i++)
            {
                int index = Random.Range(0, working.Count);
                result.Add(working[index]);
                working.RemoveAt(index);
            }

            return result;
        }

        internal bool ApplyCandidate(IReadOnlyList<LevelUpSkillCandidate> candidates, int index)
        {
            if (null == candidates || index < 0 || index >= candidates.Count)
            {
                return false;
            }

            if (_actorQuery.Player is not IPlayerSkillReceiver receiver)
            {
                return true;
            }

            candidates[index].ApplyTo(receiver);
            return true;
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
    }
}
