using Cysharp.Threading.Tasks;
using O2un.DI;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;

namespace O2un.ProjectB.Platformer
{
    // 문 상호작용(4/4 PRD)이 들어오기 전까지 룸 진행을 Play 모드로 확인하기 위한 임시 하네스.
    public sealed class RoomSpawnTestTrigger : MonoBehaviour, ISceneInitializable
    {
        private IRoomProgression _progression;

        private readonly CompositeDisposable _disposables = new();

        [Inject]
        public void Construct(IRoomProgression progression)
        {
            _progression = progression;
        }

        public void Init()
        {
            _progression.OnRoomEntered
                .Subscribe(index => Debug.Log($"[RoomTest] 룸 {index} 진입"))
                .AddTo(_disposables);

            _progression.OnRoomCleared
                .Subscribe(_ => Debug.Log("[RoomTest] 룸 클리어 — T 키로 다음 룸 이동"))
                .AddTo(_disposables);

            _progression.OnStageCleared
                .Subscribe(_ => Debug.Log("[RoomTest] 스테이지 클리어"))
                .AddTo(_disposables);

            _progression.OnLoadFailed
                .Subscribe(_ => Debug.Log("[RoomTest] 룸 로드 실패"))
                .AddTo(_disposables);

            _progression.BeginStageAsync().Forget();
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (null == keyboard)
            {
                return;
            }

            if (true == keyboard.tKey.wasPressedThisFrame)
            {
                _progression.RequestTransition("next");
            }
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }
    }
}
