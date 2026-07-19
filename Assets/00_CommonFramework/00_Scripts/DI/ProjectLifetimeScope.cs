using O2un.Data;
using O2un.Input;
using O2un.Manager;
using O2un.Sound;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;


namespace O2un.DI
{
    public class ProjectLifetimeScope : LifetimeScope
    {
        [SerializeField] private InputActionAsset _uiInputActions;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<InputManager>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<DataProvider>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<OptionManager>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<SceneManager>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<AssetManager>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<SoundSignalChannel>(Lifetime.Singleton).AsImplementedInterfaces();

            if (null == _uiInputActions)
            {
                Debug.LogError($"[ProjectLifetimeScope] '{name}' _uiInputActions가 비어 있습니다. UI EventSystem이 생성되지 않아 UI 클릭이 동작하지 않습니다.");
            }
            else
            {
                // UI EventSystem 소비처가 UIEventSystemService 하나뿐이라 전역 등록 대신 파라미터로 넘긴다.
                builder.RegisterEntryPoint<UIEventSystemService>()
                        .As<IUIEventSystemService>()
                        .WithParameter(_uiInputActions);
            }

            builder.RegisterEntryPoint<ProjectBootStrap>();
        }
    }
}
