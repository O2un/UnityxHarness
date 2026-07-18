using O2un.Input;
using R3;
using UnityEngine;
using VContainer;

namespace O2un.ProjectB.Platformer
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class RoomDoorView : MonoBehaviour, IRoomDoor
    {
        [SerializeField] private string _destinationId = "next";
        [SerializeField] private GameObject _promptRoot;
        [SerializeField] private GameObject _openedEffectRoot;
        [SerializeField] private LayerMask _playerLayers;

        private readonly Subject<string> _onTransitionRequested = new();
        private readonly CompositeDisposable _disposables = new();

        private bool _isOpen;
        private bool _playerInRange;

        public Observable<string> OnTransitionRequested => _onTransitionRequested;

        private bool CanInteract => true == _isOpen && true == _playerInRange;

        [Inject]
        public void Construct(IInputReader input, IRoomProgression progression)
        {
            // 구독은 상시 유지하고 핸들러 안에서 상태를 검사한다. 범위 진입/이탈마다 붙였다 떼면
            // 이탈 타이밍과 입력 프레임이 엇갈릴 때 동작이 흔들린다.
            input.IsAttackPressed
                .Subscribe(_ => OnAttackPressed())
                .AddTo(_disposables);

            progression.OnRoomCleared
                .Subscribe(_ => Open())
                .AddTo(_disposables);

            ApplyPromptVisibility();
            ApplyOpenedEffect();
        }

        private void Open()
        {
            _isOpen = true;

            // 콜라이더를 껐다 켜는 대신 상태 플래그로 막으므로, 이미 범위 안에 서 있어도 여기서 프롬프트가 뜬다.
            ApplyPromptVisibility();
            ApplyOpenedEffect();
        }

        private void OnAttackPressed()
        {
            if (false == CanInteract)
            {
                return;
            }

            _playerInRange = false;
            ApplyPromptVisibility();

            _onTransitionRequested.OnNext(_destinationId);
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

        // 프롬프트와 달리 범위와 무관하게 열림 상태만 따른다.
        private void ApplyOpenedEffect()
        {
            if (null == _openedEffectRoot)
            {
                return;
            }

            _openedEffectRoot.SetActive(_isOpen);
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
            _onTransitionRequested.Dispose();
        }
    }
}
