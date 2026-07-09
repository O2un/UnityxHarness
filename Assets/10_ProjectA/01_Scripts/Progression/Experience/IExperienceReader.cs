using R3;

namespace O2un.Progression
{
    public interface IExperienceReader
    {
        ReadOnlyReactiveProperty<int> CurrentExp { get; }
        ReadOnlyReactiveProperty<int> CurrentLevel { get; }
        Observable<LevelUpEvent> OnLevelUp { get; }
    }
}
