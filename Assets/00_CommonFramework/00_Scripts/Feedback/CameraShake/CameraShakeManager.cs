using System;
using R3;
using UnityEngine;
using VContainer.Unity;

namespace O2un.Feedback
{
    public sealed class CameraShakeManager : IInitializable, IDisposable
    {
        private readonly IHitFeedbackSignal _signal;
        private readonly HitFeedbackDataSO _data;
        private readonly IImpulseEmitter _emitter;

        public CameraShakeManager(IHitFeedbackSignal signal, HitFeedbackDataSO data, IImpulseEmitter emitter)
        {
            _signal = signal;
            _data = data;
            _emitter = emitter;
        }

        private readonly CompositeDisposable _disposables = new();

        public void Initialize()
        {
            if (null == _signal || null == _data || null == _emitter)
            {
                Debug.LogError($"[CameraShakeManager] 의존성 누락 — signal={null != _signal}, data={null != _data}, emitter={null != _emitter}");
                return;
            }

            _signal.OnHit.Subscribe(OnHit).AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        private void OnHit(HitFeedbackEvent hit)
        {
            HitFeedbackProfile profile = _data.GetProfile(hit.Team);
            if (null == profile)
            {
                return;
            }

            float force = profile.ShakeForce * profile.EvaluateIntensity(hit.Damage);
            if (0f >= force)
            {
                return;
            }

            _emitter.Emit(force);
        }
    }
}
