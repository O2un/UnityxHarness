namespace O2un.ProjectB.Platformer
{
    public enum UpgradeCardKind
    {
        StatModifier,
        PassiveSkill,
    }

    public enum PassiveSkillType
    {
        CriticalOnHit,
        HomingMissile,
    }

    public interface IUpgradeCardData : O2un.Manager.IItemData
    {
        UpgradeCardKind Kind { get; }
        UpgradeStatType TargetStat { get; }
        float ModifierValue { get; }
        PassiveSkillType PassiveSkill { get; }
        string DisplayName { get; }
    }
}
