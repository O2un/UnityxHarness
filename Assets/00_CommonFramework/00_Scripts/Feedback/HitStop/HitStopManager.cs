using System;
using R3;
using UnityEngine;
using VContainer.Unity;

namespace O2un.Feedback
{
    public sealed class HitStopManager : IInitializable, ITickable, IDisposable
    {
        private readonly IHitFeedbackSignal _signal;
        private readonly HitFeedbackDataSO _data;

        public HitStopManager(IHitFeedbackSignal signal, HitFeedbackDataSO data)
        {
            _signal = signal;
            _data = data;
        }

        private readonly CompositeDisposable _disposables = new();
        private readonly HitStopModule _module = new();

        private float _restoreTimeScale = 1f;

        public void Initialize()
        {
            if (null == _signal || null == _data)
            {
                Debug.LogError($"[HitStopManager] 의존성 누락 — signal={null != _signal}, data={null != _data}");
                return;
            }

            _signal.OnHit.Subscribe(OnHit).AddTo(_disposables);
        }

        public void Tick()
        {
            if (false == _module.IsActive)
            {
                return;
            }

            if (_module.Tick(Time.unscaledDeltaTime))
            {
                Time.timeScale = _restoreTimeScale;
                return;
            }

            Time.timeScale = _module.TimeScale;
        }

        public void Dispose()
        {
            if (_module.IsActive)
            {
                Time.timeScale = _restoreTimeScale;
            }

            _disposables.Dispose();
        }

        private void OnHit(HitFeedbackEvent hit)
        {
            HitFeedbackProfile profile = _data.GetProfile(hit.Team);
            if (null == profile)
            {
                return;
            }

            // 활성 중 재캡처하면 감속된 값을 원본으로 저장해 복원 불능이 되므로 비활성 → 활성 전이에서만 캡처한다.
            if (false == _module.IsActive)
            {
                _restoreTimeScale = Time.timeScale;
            }

            _module.Push(profile.HitStopDuration * profile.EvaluateIntensity(hit.Damage), profile.HitStopTimeScale);

            if (false == _module.IsActive)
            {
                return;
            }

            Time.timeScale = _module.TimeScale;
        }
    }
}
