using System;
using System.Collections.Generic;
using UnityEngine;

namespace O2un.Combat
{
    [Serializable]
    public struct SkillUpgradeEntry
    {
        [SerializeField] private string _displayName;
        [SerializeField] private float _cooldownDelta;
        [SerializeField] private int _damageDelta;
        [SerializeField] private float _lifetimeDelta;
        [SerializeField] private float _speedDelta;
        [SerializeField] private float _rangeDelta;
        [SerializeField] private float _reHitIntervalDelta;

        public string DisplayName => _displayName;

        public SkillUpgradeData Build(int level)
        {
            return new SkillUpgradeData(
                level,
                _cooldownDelta,
                _damageDelta,
                _lifetimeDelta,
                _speedDelta,
                _rangeDelta,
                _reHitIntervalDelta);
        }
    }

    [Serializable]
    public struct SkillUpgradeLevel
    {
        [SerializeField] private int _level;
        [SerializeField] private SkillUpgradeEntry[] _list;

        public int Level => _level;
        public IReadOnlyList<SkillUpgradeEntry> List => _list ?? Array.Empty<SkillUpgradeEntry>();
    }

    [CreateAssetMenu(menuName = "O2un/Combat/Skill Upgrade", fileName = "SkillUpgrade")]
    public sealed class SkillUpgradeSO : ScriptableObject
    {
        [SerializeField] private SkillDefinitionSO _skill;
        [SerializeField] private SkillUpgradeLevel[] _list;

        public SkillDefinitionSO Skill => _skill;
        public string SkillId => null != _skill ? _skill.SkillId : string.Empty;
        public IReadOnlyList<SkillUpgradeLevel> List => _list ?? Array.Empty<SkillUpgradeLevel>();
    }
}
