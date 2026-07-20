using System;
using Cysharp.Threading.Tasks;
using O2un.Manager;
using R3;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace O2un.ProjectB.Platformer
{
    public sealed class RoomModule : IRoomProgression, IDisposable
    {
        private readonly StageDataSO _stageData;
        private readonly IAdditiveSceneLoader _sceneLoader;
        private readonly ISceneService _sceneService;
        private readonly IScreenFader _fader;
        private readonly IPlayerPlacer _playerPlacer;
        private readonly IRoomSignalPublisher _publisher;
        private readonly IRoomSignalSource _source;
        private readonly LifetimeScope _parentScope;

        private readonly Subject<int> _onRoomEntered = new();
        private readonly Subject<Unit> _onRoomCleared = new();
        private readonly Subject<Unit> _onStageCleared = new();
        private readonly Subject<Unit> _onLoadFailed = new();
        private readonly CompositeDisposable _disposables = new();

        private RoomState _state = RoomState.Idle;
        private int _currentIndex = -1;
        private Scene _currentScene;
        private bool _hasCurrentScene;
        private Vector3 _pendingSpawnPosition;
        private bool _hasPendingSpawnPosition;

        public RoomModule(
            StageDataSO stageData,
            IAdditiveSceneLoader sceneLoader,
            ISceneService sceneService,
            IScreenFader fader,
            IPlayerPlacer playerPlacer,
            IRoomSignalPublisher publisher,
            IRoomSignalSource source,
            LifetimeScope parentScope)
        {
            _stageData = stageData;
            _sceneLoader = sceneLoader;
            _sceneService = sceneService;
            _fader = fader;
            _playerPlacer = playerPlacer;
            _publisher = publisher;
            _source = source;
            _parentScope = parentScope;

            _source.OnRoomReady
                .Subscribe(position =>
                {
                    _pendingSpawnPosition = position;
                    _hasPendingSpawnPosition = true;
                })
                .AddTo(_disposables);

            _source.OnRoomCleared
                .Subscribe(_ => OnCurrentRoomCleared())
                .AddTo(_disposables);
        }

        public Observable<int> OnRoomEntered => _onRoomEntered;
        public Observable<Unit> OnRoomCleared => _onRoomCleared;
        public Observable<Unit> OnStageCleared => _onStageCleared;
        public Observable<Unit> OnLoadFailed => _onLoadFailed;

        public RoomState State => _state;

        public async UniTask BeginStageAsync()
        {
            if (RoomState.Idle != _state)
            {
                return;
            }

            if (null == _stageData || 0 == _stageData.Rooms.Count)
            {
                Debug.LogError("[RoomModule] StageDataSO에 룸이 없습니다. 스테이지를 시작할 수 없습니다.");
                return;
            }

            await EnterRoomAsync(0);
        }

        public void RequestTransition(string destinationId)
        {
            if (RoomState.Cleared != _state)
            {
                return;
            }

            int next = _currentIndex + 1;
            if (next >= _stageData.Rooms.Count)
            {
                FinishStage();
                return;
            }

            EnterRoomAsync(next).Forget();
        }

        private void OnCurrentRoomCleared()
        {
            if (RoomState.Playing != _state)
            {
                return;
            }

            _state = RoomState.Cleared;
            _onRoomCleared.OnNext(Unit.Default);
        }

        private void FinishStage()
        {
            _state = RoomState.Finished;
            _onStageCleared.OnNext(Unit.Default);
        }

        private async UniTask EnterRoomAsync(int index)
        {
            _state = RoomState.Transitioning;

            await _fader.FadeOutAsync(_stageData.FadeOutDuration);

            if (true == _hasCurrentScene)
            {
                await _sceneLoader.UnloadSceneAsync(_currentScene);
                _hasCurrentScene = false;
            }

            _state = RoomState.Loading;
            _hasPendingSpawnPosition = false;

            RoomDataSO room = _stageData.Rooms[index];
            try
            {
                _currentScene = await _sceneLoader.LoadAdditiveSceneAsync(room.SceneKey, _parentScope);
                _hasCurrentScene = true;
            }
            catch (Exception exception)
            {
                await HandleLoadFailureAsync(room, exception);
                return;
            }

            _currentIndex = index;

            if (true == _hasPendingSpawnPosition)
            {
                _playerPlacer.PlaceAt(_pendingSpawnPosition);
            }
            else
            {
                Debug.LogWarning($"[RoomModule] '{room.SceneKey}'가 스폰 지점을 보고하지 않아 플레이어를 옮기지 않았습니다.");
            }

            await _fader.FadeInAsync(_stageData.FadeInDuration);

            _state = RoomState.Playing;
            _onRoomEntered.OnNext(index);

            _publisher.RequestSpawnBegin();
        }

        // 룸 로드 실패는 게임 진행 불가로 본다. 재시도하지 않고 페이드를 유지한 채 게임 선택으로 되돌린다.
        private async UniTask HandleLoadFailureAsync(RoomDataSO room, Exception exception)
        {
            Debug.LogError($"[RoomModule] 룸 로드 실패로 스테이지를 중단합니다. key={room.SceneKey}\n{exception}");

            _state = RoomState.Finished;
            _onLoadFailed.OnNext(Unit.Default);

            await _sceneService.LoadSceneAsync(SCENE_NAME.GAME_SELECT_SCENE);
        }

        public void Dispose()
        {
            _disposables.Dispose();
            _onRoomEntered.Dispose();
            _onRoomCleared.Dispose();
            _onStageCleared.Dispose();
            _onLoadFailed.Dispose();
        }
    }
}
