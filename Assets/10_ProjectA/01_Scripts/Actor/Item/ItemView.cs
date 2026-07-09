using System;
using O2un.Combat;
using O2un.Manager;
using R3;
using UnityEngine;

namespace O2un.Actors
{
    public sealed class ItemView : MonoBehaviour, IPoolable
    {
        [SerializeField] private Collider _trigger;

        private ItemActor _actor;
        private Action _release;

        public Observable<int> OnPicked => (_actor ??= new ItemActor()).OnPicked;

        public void Configure(int amount)
        {
            (_actor ??= new ItemActor()).Configure(amount);
        }

        public void SetReleaseCallback(Action release) => _release = release;

        public void OnSpawned()
        {
            // NULL
        }

        public void OnDespawned()
        {
            // NULL
        }

        private void Awake()
        {
            _actor ??= new ItemActor();
        }

        private void OnTriggerEnter(Collider other)
        {
            DamageableView damageable = other.GetComponentInParent<DamageableView>();
            if (null == damageable || ActorType.Player != damageable.Team)
            {
                return;
            }

            _actor.Pick();
            _release?.Invoke();
        }

        private void OnDestroy()
        {
            _actor?.Dispose();
        }
    }
}
