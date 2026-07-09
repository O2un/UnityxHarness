using O2un.Actors;
using O2un.Combat;

namespace O2un.Progression
{
    internal readonly struct LevelUpSkillCandidate
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
}
