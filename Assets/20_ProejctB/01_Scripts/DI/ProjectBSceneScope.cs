using System.Collections.Generic;
using O2un.Actors;
using O2un.DI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace O2un.ProjectB.Platformer
{
    public class ProjectBSceneScope : LifetimeScope
    {
        [SerializeField] private List<GameObject> _sceneInitializables = new();

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<ActorManager>(Lifetime.Singleton).AsSelf().AsImplementedInterfaces();

            builder.RegisterBuildCallback(InitializeSceneComponents);
        }

        private void InitializeSceneComponents(IObjectResolver resolver)
        {
            for (int i = 0; i < _sceneInitializables.Count; i++)
            {
                GameObject mb = _sceneInitializables[i];
                if (null == mb)
                {
                    Debug.LogError($"[ProjectBSceneScope] '{name}' _sceneInitializables[{i}]가 비어 있습니다.");
                    continue;
                }

                var initializable = mb.GetComponent<ISceneInitializable>();
                if (null != initializable)
                {
                    resolver.InjectGameObject(mb);
                    initializable.Init();
                }
            }
        }
    }
}
