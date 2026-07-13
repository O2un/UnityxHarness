using System;
using O2un.DataStore;
using R3;
using UnityEngine;

namespace O2un.UI 
{
    public sealed class HudVM : IDisposable
    {
        public ReadOnlyReactiveProperty<bool> IsVisible {get;}
        private readonly ReactiveProperty<float> _currentHp = new();
        public ReadOnlyReactiveProperty<float> CurrentHp => _currentHp;

        private readonly CompositeDisposable _disposables = new();

        public HudVM(IUIReader uireader, IPlayerDataReader playerData)
        {
            IsVisible = uireader.GetVisible(UIType.HUD);
            playerData.CurrentHP.Subscribe(x=>
            {
                _currentHp.Value = (float)x/ playerData.MaxHP.CurrentValue;
            }).AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
            _currentHp.Dispose();
        }
    }
}
