using UnityEngine;

namespace O2un.Combat
{
    [CreateAssetMenu(menuName = "O2un/Combat/Skill Upgrade", fileName = "SkillUpgrade")]
    public sealed class SkillUpgradeSO : ScriptableObject
    {
        [SerializeField] private SkillDefinitionSO _skill;
        [SerializeField] private int _level = 1;
        [SerializeField] private string _displayName;
        [SerializeField] private float _cooldownDelta;
        [SerializeField] private int _damageDelta;
        [SerializeField] private float _lifetimeDelta;
        [SerializeField] private float _speedDelta;
        [SerializeField] private float _rangeDelta;
        [SerializeField] private float _reHitIntervalDelta;

        public SkillDefinitionSO Skill => _skill;
        public string SkillId => null != _skill ? _skill.SkillId : string.Empty;
        public int Level => _level;
        public string DisplayName => string.IsNullOrEmpty(_displayName) ? name : _displayName;

        public SkillUpgradeData Build()
        {
            return new SkillUpgradeData(
                _level,
                _cooldownDelta,
                _damageDelta,
                _lifetimeDelta,
                _speedDelta,
                _rangeDelta,
                _reHitIntervalDelta);
        }
    }
}
