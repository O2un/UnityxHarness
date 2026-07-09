using System;
using System.Collections.Generic;
using O2un.Combat;
using UnityEngine;
using UnityEngine.Serialization;

namespace O2un.Progression
{
    [CreateAssetMenu(menuName = "O2un/Progression/Level Up Skill Pool", fileName = "LevelUpSkillPool")]
    public sealed class LevelUpSkillPoolSO : ScriptableObject
    {
        [FormerlySerializedAs("_candidates")]
        [SerializeField] private SkillDefinitionSO[] _learnCandidates;
        [SerializeField] private SkillUpgradeSO[] _upgradeCandidates;

        public IReadOnlyList<SkillDefinitionSO> LearnCandidates => _learnCandidates ?? Array.Empty<SkillDefinitionSO>();
        public IReadOnlyList<SkillUpgradeSO> UpgradeCandidates => _upgradeCandidates ?? Array.Empty<SkillUpgradeSO>();
    }
}
