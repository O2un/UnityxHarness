using System;
using O2un.Input;
using O2un.Manager;
using R3;
using UnityEngine;
using VContainer;

namespace O2un.ProjectB.Platformer
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class RewardCardView : MonoBehaviour, IPoolable
    {
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private GameObject _promptRoot;
        [SerializeField] private GameObject _slotsFullRoot;
        [SerializeField] private LayerMask _playerLayers;

        private readonly Subject<IUpgradeCardData> _onSelectRequested = new();
        private readonly CompositeDisposable _disposables = new();

        private IUpgradeCardData _card;
        private Action _release;
        private bool _playerInRange;

        public Observable<IUpgradeCardData> OnSelectRequested => _onSelectRequested;

        public IUpgradeCardData Card => _card;

        private bool CanInteract => null != _card && true == _playerInRange;

        [Inject]
        public void Construct(IInputReader input)
        {
            // RoomDoorView와 같은 이유로 구독은 상시 유지하고 핸들러 안에서 상태를 검사한다.
            input.IsAttackPressed
                .Subscribe(_ => OnAttackPressed())
                .AddTo(_disposables);
        }

        public void SetReleaseCallback(Action release)
        {
            _release = release;
        }

        public void Bind(IUpgradeCardData card, Vector3 position, Color tint)
        {
            _card = card;
            transform.position = position;

            if (null != _renderer)
            {
                _renderer.color = tint;
            }

            ApplyPromptVisibility();
            ShowSlotsFullNotice(false);
        }

        public void ShowSlotsFullNotice(bool visible)
        {
            if (null == _slotsFullRoot)
            {
                return;
            }

            _slotsFullRoot.SetActive(visible);
        }

        public void ReleaseSelf()
        {
            _release?.Invoke();
        }

        public void OnSpawned()
        {
            // NULL — 스폰 직후 스포너가 Bind로 상태를 채운다
        }

        public void OnDespawned()
        {
            _card = null;
            _playerInRange = false;
            ApplyPromptVisibility();
            ShowSlotsFullNotice(false);
        }

        private void OnAttackPressed()
        {
            if (false == CanInteract)
            {
                return;
            }

            _onSelectRequested.OnNext(_card);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (false == IsPlayer(other))
            {
                return;
            }

            _playerInRange = true;
            ApplyPromptVisibility();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (false == IsPlayer(other))
            {
                return;
            }

            _playerInRange = false;
            ApplyPromptVisibility();
            ShowSlotsFullNotice(false);
        }

        private bool IsPlayer(Collider2D other)
        {
            return 0 != (_playerLayers.value & (1 << other.gameObject.layer));
        }

        private void ApplyPromptVisibility()
        {
            if (null == _promptRoot)
            {
                return;
            }

            _promptRoot.SetActive(CanInteract);
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
            _onSelectRequested.Dispose();
        }
    }
}
