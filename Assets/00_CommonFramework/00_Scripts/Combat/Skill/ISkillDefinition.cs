namespace O2un.Combat
{
    public interface ISkillDefinition
    {
        string SkillId { get; }
        int Level { get; }
        float Cooldown { get; }
        bool ApplyUpgrade(SkillUpgradeData upgrade);
        void Activate(ISkillContext ctx);
    }
}
