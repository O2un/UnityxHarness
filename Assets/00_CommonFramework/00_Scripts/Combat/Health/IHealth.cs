using R3;

namespace O2un.Combat
{
    public interface IHealth
    {
        ReadOnlyReactiveProperty<int> CurrentHP { get; }
        int MaxHP { get; }
        void VaryHP(int delta);
    }
}
