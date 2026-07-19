using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using VContainer.Unity;

namespace O2un.Input
{
    public interface IUIEventSystemService
    {
        bool IsReady { get; }
    }

    // UI Toolkit 런타임 패널은 EventSystem이 없으면 포인터 이벤트를 받지 못한다.
    // 씬마다 두면 중복·누락이 생기므로 루트 스코프에서 하나만 만들고 DontDestroyOnLoad로 유지한다
    public sealed class UIEventSystemService : IUIEventSystemService, IInitializable, IDisposable
    {
        private const string OBJECT_NAME = "[UIEventSystem]";

        private readonly InputActionAsset _uiActions;

        private GameObject _instance;

        public bool IsReady => null != _instance;

        public UIEventSystemService(InputActionAsset uiActions)
        {
            _uiActions = uiActions;
        }

        public void Initialize()
        {
            if (null != EventSystem.current)
            {
                // 씬에 수동 배치된 EventSystem이 있으면 그쪽을 존중하고 중복 생성하지 않는다
                return;
            }

            if (null == _uiActions)
            {
                Debug.LogError("[UIEventSystemService] UI InputActionAsset이 비어 있어 EventSystem을 만들지 않습니다. UI 클릭이 동작하지 않습니다.");
                return;
            }

            _instance = new GameObject(OBJECT_NAME);
            _instance.AddComponent<EventSystem>();

            InputSystemUIInputModule module = _instance.AddComponent<InputSystemUIInputModule>();
            module.actionsAsset = _uiActions;

            UnityEngine.Object.DontDestroyOnLoad(_instance);
        }

        public void Dispose()
        {
            if (null == _instance)
            {
                return;
            }

            UnityEngine.Object.Destroy(_instance);
            _instance = null;
        }
    }
}
