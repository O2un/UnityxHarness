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
        }

        private void Update()
        {
            if (false == _active)
            {
                return;
            }

            float dt = Time.deltaTime;
            _hitbox.Tick(dt);

            if (0f < _speed)
            {
                transform.position += (Vector3)(_direction * (_speed * dt));
            }

            if (true == _hitbox.IsExpired)
            {
                ReleaseSelf();
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
