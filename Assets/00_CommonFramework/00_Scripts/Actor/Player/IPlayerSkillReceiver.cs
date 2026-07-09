using O2un.Combat;

namespace O2un.Actors
{
    public interface IPlayerSkillReceiver
    {
        bool AcquireOrUpgradeSkill(ISkillDefinition definition);
        bool ApplySkillUpgrade(string skillId, SkillUpgradeData upgrade);
        int GetSkillLevel(string skillId);
        bool HasSkill(string skillId);
    }
}
