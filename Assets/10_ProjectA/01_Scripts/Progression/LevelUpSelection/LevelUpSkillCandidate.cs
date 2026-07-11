using O2un.Actors;
using O2un.Combat;

namespace O2un.Progression
{
    internal readonly struct LevelUpSkillCandidate
    {
        private readonly SkillDefinitionSO _learnSkill;
        private readonly SkillUpgradeSO _upgrade;
        private readonly SkillUpgradeEntry _upgradeEntry;
        private readonly int _upgradeLevel;

        public LevelUpSkillCandidate(SkillDefinitionSO learnSkill)
        {
            _learnSkill = learnSkill;
            _upgrade = null;
            _upgradeEntry = default;
            _upgradeLevel = 0;
        }

        public LevelUpSkillCandidate(SkillUpgradeSO upgrade, SkillUpgradeEntry upgradeEntry, int upgradeLevel)
        {
            _learnSkill = null;
            _upgrade = upgrade;
            _upgradeEntry = upgradeEntry;
            _upgradeLevel = upgradeLevel;
        }

        public string Label => null != _upgrade && false == string.IsNullOrEmpty(_upgradeEntry.DisplayName)
            ? _upgradeEntry.DisplayName
            : null != _upgrade ? _upgrade.name : _learnSkill.name;

        public bool ApplyTo(IPlayerSkillReceiver receiver)
        {
            if (null != _upgrade)
            {
                return receiver.ApplySkillUpgrade(_upgrade.SkillId, _upgradeEntry.Build(_upgradeLevel));
            }

            return receiver.AcquireOrUpgradeSkill(_learnSkill.Build());
        }
    }
}
