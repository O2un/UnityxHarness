using System;
using O2un.Combat;
using O2un.Manager;
using R3;
using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class Projectile2DView : MonoBehaviour, IPoolable
    {
        private readonly CompositeDisposable _despawnBag = new();

        [SerializeField] private SpriteRenderer _renderer;

        private HitboxModule _hitbox;
        private Vector2 _direction;
        private float _speed;
        private bool _releaseOnHit;
        private Action _release;
        private Collider2D _collider;
        private bool _active;

        private Transform _homingTarget;
        private float _turnRate;

        private Collider2D Collider => _collider ??= GetComponent<Collider2D>();

        public void SetReleaseCallback(Action release)
        {
            _release = release;
        }

        public void Configure(HitboxModule hitbox, Vector2 direction, float speed, Vector3 origin, bool releaseOnHit)
        {
            _hitbox = hitbox;
            _direction = direction;
            _speed = speed;
            _releaseOnHit = releaseOnHit;

            transform.position = origin;

            if (null != _renderer)
            {
                _renderer.flipX = direction.x < 0f;
            }

            _hitbox.OnHit.Subscribe(OnHit).AddTo(_despawnBag);
            Collider.enabled = true;
            _active = true;
        }

        // Configure 뒤에 따로 호출한다. 부르지 않으면 기존 직선 발사와 동일하게 동작한다.
        public void SetHoming(Transform target, float turnRate)
        {
            _homingTarget = target;
            _turnRate = turnRate;
        }

        public void OnSpawned()
        {
            // NULL — 스폰 직후 발사자가 Configure로 상태를 채운다
        }

        public void OnDespawned()
        {
            _active = false;
            Collider.enabled = false;
            _despawnBag.Clear();
            _hitbox = null;
            _homingTarget = null;
            _turnRate = 0f;
        }

        private void Update()
        {
            if (false == _active)
            {
                return;
            }

            float dt = Time.deltaTime;
            _hitbox.Tick(dt);

            SteerTowardTarget(dt);

            if (0f < _speed)
            {
                transform.position += (Vector3)(_direction * (_speed * dt));
            }

            if (true == _hitbox.IsExpired)
            {
                ReleaseSelf();
            }
        }

        // 대상이 도중에 사라지면 마지막 방향을 그대로 유지한다.
        private void SteerTowardTarget(float dt)
        {
            if (_turnRate <= 0f || null == _homingTarget || false == _homingTarget.gameObject.activeInHierarchy)
            {
                return;
            }

            Vector2 desired = (Vector2)(_homingTarget.position - transform.position);
            if (desired.sqrMagnitude < Mathf.Epsilon)
            {
                return;
            }

            float maxRadians = _turnRate * Mathf.Deg2Rad * dt;
            Vector3 steered = Vector3.RotateTowards(_direction, desired.normalized, maxRadians, 0f);
            _direction = ((Vector2)steered).normalized;

            if (null != _renderer)
            {
                _renderer.flipX = _direction.x < 0f;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryHit(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            TryHit(other);
        }

        private void TryHit(Collider2D other)
        {
            if (false == _active || null == _hitbox)
            {
                return;
            }

            Damageable2DView target = other.GetComponentInParent<Damageable2DView>();
            if (null == target)
            {
                return;
            }

            _hitbox.TryHit(target);
        }

        private void OnHit(DamageEvent e)
        {
            e.Target.ApplyDamage(e.Damage);

            if (true == _releaseOnHit)
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

        private void OnDestroy()
        {
            _despawnBag.Dispose();
        }
    }
}
