using System;
using System.Collections.Generic;
using O2un.Sound;
using R3;
using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    public sealed class Enemy2DSensorView : MonoBehaviour, IDisposable
    {
        [SerializeField] private EnemySensingData _data;
        [SerializeField] private Transform _eyeOrigin;
        [SerializeField] private Transform _groundProbe;

        private readonly CompositeDisposable _disposables = new();
        private readonly List<Collider2D> _targetBuffer = new();

        private SensingTimerModule _timer;
        private HearingModule _hearing;

        private bool _heardSoundLatched;
        private bool _wasHitLatched;

        private SensorSnapshot _snapshot;

        public SensorSnapshot Snapshot => _snapshot;
        public EnemySensingData Data => _data;
        public Transform EyeOrigin => null != _eyeOrigin ? _eyeOrigin : transform;
        public Transform GroundProbe => null != _groundProbe ? _groundProbe : transform;

        public void Init(ISoundSignalSource soundSource, Observable<int> onDamaged)
        {
            if (null == _data)
            {
                Debug.LogError($"[Enemy2DSensorView] '{name}' EnemySensingData가 비어 있습니다.");
                return;
            }

            _timer = new SensingTimerModule(_data.SensingInterval, UnityEngine.Random.Range(0f, _data.SensingInterval));
            _hearing = new HearingModule(_data.HearingRadius);

            soundSource?.OnSound
                .Subscribe(OnSound)
                .AddTo(_disposables);

            onDamaged?
                .Subscribe(_ => _wasHitLatched = true)
                .AddTo(_disposables);

            Sense();
        }

        public void Tick(float dt)
        {
            if (null == _timer)
            {
                return;
            }

            if (false == _timer.TryConsume(dt, out float _))
            {
                return;
            }

            Sense();
        }

        private void OnSound(SoundSignal signal)
        {
            if (true == _hearing.CanHear(signal, transform.position))
            {
                _heardSoundLatched = true;
            }
        }

        private void Sense()
        {
            Vector2 selfPosition = transform.position;
            float facing = 0f <= transform.localScale.x ? 1f : -1f;
            Vector2 eye = null != _eyeOrigin ? (Vector2)_eyeOrigin.position : selfPosition;

            bool hasTarget = TryFindVisibleTarget(eye, facing, out Vector2 targetPosition, out bool isVisible);

            _snapshot = new SensorSnapshot(
                selfPosition,
                facing,
                hasTarget,
                targetPosition,
                isVisible,
                _heardSoundLatched,
                _wasHitLatched,
                ProbeGroundAhead(selfPosition, facing),
                ProbeWallAhead(eye, facing));

            _heardSoundLatched = false;
            _wasHitLatched = false;
        }

        private bool TryFindVisibleTarget(Vector2 eye, float facing, out Vector2 targetPosition, out bool isVisible)
        {
            targetPosition = Vector2.zero;
            isVisible = false;

            ContactFilter2D filter = new();
            filter.SetLayerMask(_data.TargetMask);
            filter.useTriggers = true;

            int count = Physics2D.OverlapCircle(eye, _data.AggroRange, filter, _targetBuffer);
            if (0 == count)
            {
                return false;
            }

            targetPosition = _targetBuffer[0].bounds.center;

            Vector2 toTarget = targetPosition - eye;
            float distance = toTarget.magnitude;
            if (distance > _data.SightDistance)
            {
                return true;
            }

            Vector2 forward = new(facing, 0f);
            if (Vector2.Angle(forward, toTarget) > _data.SightAngle * 0.5f)
            {
                return true;
            }

            RaycastHit2D blocker = Physics2D.Raycast(eye, toTarget.normalized, distance, _data.ObstacleMask);
            isVisible = null == blocker.collider;
            return true;
        }

        private bool ProbeGroundAhead(Vector2 selfPosition, float facing)
        {
            Vector2 origin = null != _groundProbe ? (Vector2)_groundProbe.position : selfPosition;
            origin.x += facing * _data.WallProbeDistance;

            return null != Physics2D.Raycast(origin, Vector2.down, _data.GroundProbeDistance, _data.GroundMask).collider;
        }

        private bool ProbeWallAhead(Vector2 eye, float facing)
        {
            return null != Physics2D.Raycast(eye, new Vector2(facing, 0f), _data.WallProbeDistance, _data.ObstacleMask).collider;
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        private void OnDestroy()
        {
            Dispose();
        }
    }
}
