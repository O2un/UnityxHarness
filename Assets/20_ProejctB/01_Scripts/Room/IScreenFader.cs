using Cysharp.Threading.Tasks;

namespace O2un.ProjectB.Platformer
{
    public interface IScreenFader
    {
        UniTask FadeOutAsync(float duration);
        UniTask FadeInAsync(float duration);
    }
}
