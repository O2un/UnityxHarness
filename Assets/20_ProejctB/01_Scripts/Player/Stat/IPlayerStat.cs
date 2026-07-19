using R3;

namespace O2un.ProjectB.Platformer
{
    public enum UpgradeStatType
    {
        AttackDamage,
        MaxHealth,
        MoveSpeed,
    }

    public interface IPlayerStatReader
    {
        ReadOnlyReactiveProperty<float> MoveSpeed { get; }
        ReadOnlyReactiveProperty<int> MaxHealth { get; }
        ReadOnlyReactiveProperty<int> AttackBonus { get; }
    }

    public interface IPlayerStatWriter
    {
        void SetBase(UpgradeStatType stat, float baseValue);
        void AddModifier(UpgradeStatType stat, float value);
        void ClearModifiers();
    }
}
