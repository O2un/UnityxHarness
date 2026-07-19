using System;
using System.Collections.Generic;
using R3;
using VContainer.Unity;

namespace O2un.ProjectB.Platformer
{
    public sealed class RoomRewardModule : IInitializable, IDisposable
    {
        private readonly IRoomProgression _progression;
        private readonly UpgradeCardAcquisition _acquisition;

        private readonly Subject<IReadOnlyList<UpgradeCardSO>> _onCandidatesReady = new();
        private readonly Subject<Unit> _onCandidatesCleared = new();
        private readonly Subject<Unit> _onSlotsFull = new();

        private readonly CompositeDisposable _disposables = new();

        private bool _hasTakenReward;

        public Observable<IReadOnlyList<UpgradeCardSO>> OnCandidatesReady => _onCandidatesReady;
        public Observable<Unit> OnCandidatesCleared => _onCandidatesCleared;
        public Observable<Unit> OnSlotsFull => _onSlotsFull;

        public RoomRewardModule(IRoomProgression progression, UpgradeCardAcquisition acquisition)
        {
            _progression = progression;
            _acquisition = acquisition;
        }

        public void Initialize()
        {
            _progression.OnRoomEntered
                .Subscribe(_ => OnRoomEntered())
                .AddTo(_disposables);

            _progression.OnRoomCleared
                .Subscribe(_ => OnRoomCleared())
                .AddTo(_disposables);
        }

        public void RequestSelect(IUpgradeCardData card)
        {
            // 카드 오브젝트 제거가 아니라 플래그로 막는다. 회수 타이밍과 입력 프레임이 엇갈려도 중복 획득이 없다.
            if (true == _hasTakenReward)
            {
                return;
            }

            if (false == _acquisition.TryAcquire(card))
            {
                _onSlotsFull.OnNext(Unit.Default);
                return;
            }

            _hasTakenReward = true;
            _onCandidatesCleared.OnNext(Unit.Default);
        }

        public void Dispose()
        {
            _disposables.Dispose();
            _onCandidatesReady.Dispose();
            _onCandidatesCleared.Dispose();
            _onSlotsFull.Dispose();
        }

        private void OnRoomEntered()
        {
            _hasTakenReward = false;
            _onCandidatesCleared.OnNext(Unit.Default);
        }

        private void OnRoomCleared()
        {
            if (true == _hasTakenReward)
            {
                return;
            }

            _onCandidatesReady.OnNext(_acquisition.DrawCandidates());
        }
    }
}
