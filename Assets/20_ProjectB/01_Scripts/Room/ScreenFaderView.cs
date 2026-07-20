using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace O2un.ProjectB.Platformer
{
    [RequireComponent(typeof(CanvasGroup))]
    public sealed class ScreenFaderView : MonoBehaviour, IScreenFader
    {
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private Graphic _overlay;

        private CancellationTokenSource _cts;

        private CanvasGroup Group => _group ??= GetComponent<CanvasGroup>();

        private void Awake()
        {
            Group.blocksRaycasts = false;
            Group.interactable = false;

            if (null != _overlay)
            {
                _overlay.raycastTarget = false;
            }
        }

        public UniTask FadeOutAsync(float duration)
        {
            return FadeToAsync(1f, duration);
        }

        public UniTask FadeInAsync(float duration)
        {
            return FadeToAsync(0f, duration);
        }

        private async UniTask FadeToAsync(float target, float duration)
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
            CancellationToken token = _cts.Token;

            float start = Group.alpha;

            if (0f >= duration)
            {
                Group.alpha = target;
                return;
            }

            float elapsed = 0f;
            try
            {
                while (elapsed < duration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    Group.alpha = Mathf.Lerp(start, target, Mathf.Clamp01(elapsed / duration));
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }

            Group.alpha = target;
        }
    }
}
