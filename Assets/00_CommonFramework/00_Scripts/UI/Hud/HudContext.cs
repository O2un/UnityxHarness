using O2un.DataStore;
using O2un.DI;
using UnityEngine;
using VContainer;

namespace O2un.UI
{
    public class HudContext : MonoBehaviour, ISceneInitializable
    {
        [SerializeField] private HudView _view;

        [Inject] private IUIReader _reader;
        [Inject] private IPlayerDataReader _playerData;

        private HudVM _vm;

        public void Init()
        {
            if (null == _reader || null == _playerData)
            {
                Debug.LogError($"[HudContext] '{name}' 의존성 주입 실패 — reader={_reader != null}, playerData={_playerData != null}");
                return;
            }

            _vm = new HudVM(_reader, _playerData);
            _view.Bind(_vm);
        }

        void OnDestroy()
        {
            _vm?.Dispose();
        }
    }
}
