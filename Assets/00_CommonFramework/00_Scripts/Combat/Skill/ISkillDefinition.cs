namespace O2un.Combat
{
    public interface ISkillDefinition
    {
        float Cooldown { get; }
        void Activate(ISkillContext ctx);
    }
}
