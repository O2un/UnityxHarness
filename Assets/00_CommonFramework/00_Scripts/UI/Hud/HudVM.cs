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

        public HudVM(IUIReader uireader, IPlayerDataReader playerData)
        {
            IsVisible = uireader.GetVisible(UIType.HUD);
            playerData.CurrentHP.Subscribe(x=>
            {
                _currentHp.Value = (float)x/ playerData.MaxHP.CurrentValue;
            });
            
        }

        public void Dispose()
        {
            _currentHp.Dispose();
        }
    }
}
