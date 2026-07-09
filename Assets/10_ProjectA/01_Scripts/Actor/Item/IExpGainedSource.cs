using R3;

namespace O2un.Actors
{
    public interface IExpGainedSource
    {
        Observable<int> OnGained { get; }
    }
}
