using O2un.Data;
using O2un.Input;
using O2un.Manager;
using UnityEngine;
using VContainer;
using VContainer.Unity;


namespace O2un.DI 
{
    public class ProjectLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<InputManager>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<DataProvider>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
            builder.Register<OptionManager>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<SceneManager>(Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<AssetManager>(Lifetime.Singleton).AsImplementedInterfaces();

            builder.RegisterEntryPoint<ProjectBootStrap>();
        }
    }
}
