using System;
using O2un.Manager;
using R3;
using UnityEngine;

namespace O2un.Combat
{
    public struct HitboxMotion
    {
        public Vector3 Origin;
        public Quaternion Rotation;
        public Vector3 MoveDirection;
        public float Speed;
        public Transform FollowOwner;
        public bool ReleaseOnHit;
    }

    [RequireComponent(typeof(Collider))]
    public sealed class AttackHitboxView : MonoBehaviour, IPoolable
    {
        private readonly CompositeDisposable _despawnBag = new();

        private HitboxModule _hitbox;
        private HitboxMotion _motion;
        private Action _release;
        private bool _active;

        public void SetReleaseCallback(Action release)
        {
            _release = release;
        }

        public void Configure(HitboxModule hitbox, HitboxMotion motion)
        {
            _hitbox = hitbox;
            _motion = motion;

            Transform self = transform;
            self.position = motion.Origin;
            self.rotation = motion.Rotation;

            _hitbox.OnHit.Subscribe(OnHit).AddTo(_despawnBag);
            _active = true;
        }

        public void OnSpawned()
        {
            // NULL — 스폰 직후 AttackSpawner가 Configure로 상태를 채운다
        }

        public void OnDespawned()
        {
            _active = false;
            _despawnBag.Clear();
            _hitbox = null;
        }

        private void Update()
        {
            if (false == _active)
            {
                return;
            }

            float dt = Time.deltaTime;
            _hitbox.Tick(dt);

            if (null != _motion.FollowOwner)
            {
                transform.position = _motion.FollowOwner.position;
            }
            else if (_motion.Speed > 0f)
            {
                transform.position += _motion.MoveDirection * (_motion.Speed * dt);
            }

            if (true == _hitbox.IsExpired)
            {
                ReleaseSelf();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            TryHit(other);
        }

        private void OnTriggerStay(Collider other)
        {
            TryHit(other);
        }

        private void TryHit(Collider other)
        {
            if (false == _active)
            {
                return;
            }

            DamageableView target = other.GetComponentInParent<DamageableView>();
            if (null == target)
            {
                return;
            }

            _hitbox.TryHit(target);
        }

        private void OnHit(DamageEvent e)
        {
            e.Target.ApplyDamage(e.Damage);

            if (true == _motion.ReleaseOnHit)
            {
                ReleaseSelf();
            }
        }

        private void ReleaseSelf()
        {
            if (false == _active)
            {
                return;
            }

            _active = false;
            _release?.Invoke();
        }
    }
}
