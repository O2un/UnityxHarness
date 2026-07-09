using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using O2un.Manager;
using R3;
using UnityEngine;
using VContainer.Unity;

namespace O2un.Actors
{
    public sealed class ItemDropContext : IAsyncStartable
    {
        private readonly IAssetService _assetService;
        private readonly IPoolService _poolService;
        private readonly IEnemyKillEvent _killEvent;
        private readonly IExpGainedPublisher _expPublisher;
        private readonly ItemDropDataSO _itemDropData;

        private readonly CompositeDisposable _disposables = new();

        public ItemDropContext(
            IAssetService assetService,
            IPoolService poolService,
            IEnemyKillEvent killEvent,
            IExpGainedPublisher expPublisher,
            ItemDropDataSO itemDropData)
        {
            _assetService = assetService;
            _poolService = poolService;
            _killEvent = killEvent;
            _expPublisher = expPublisher;
            _itemDropData = itemDropData;
        }

        public async UniTask StartAsync(CancellationToken cancellation = default)
        {
            GameObject prefab = await _assetService.LoadAsync<GameObject>(_itemDropData.PrefabKey);
            ItemView view = prefab.GetComponent<ItemView>();
            if (null == view)
            {
                Debug.LogError($"[ItemDropContext] '{_itemDropData.PrefabKey}' 프리팹에 ItemView가 없습니다.");
                return;
            }

            if (false == _poolService.IsRegistered(_itemDropData.PrefabKey))
            {
                _poolService.Register(_itemDropData.PrefabKey, view);
            }

            _killEvent.OnKilled.Subscribe(OnEnemyKilled).AddTo(_disposables);
        }

        private void OnEnemyKilled(EnemyKilledInfo info)
        {
            IPoolHandle<ItemView> handle = _poolService.GetHandle<ItemView>(_itemDropData.PrefabKey);
            if (null == handle)
            {
                return;
            }

            ItemView item = handle.Get();
            item.transform.position = info.Position;
            item.Configure(info.Exp);

            IDisposable pickSubscription = null;
            pickSubscription = item.OnPicked.Subscribe(amount =>
            {
                _expPublisher.Publish(amount);
                pickSubscription?.Dispose();
            });
        }
    }
}
