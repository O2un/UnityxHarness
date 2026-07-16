using O2un.Actors;
using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    [RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
    public sealed class NpcView : MonoBehaviour, IActorView
    {
        private Actor _actor;

        public void Bind(Actor actor)
        {
            _actor = actor;

            if (true == isActiveAndEnabled)
            {
                _actor.Register();
            }
        }

        public void Unbind(Actor actor)
        {
            if (false == ReferenceEquals(_actor, actor))
            {
                return;
            }

            _actor = null;
        }

        private void OnEnable()
        {
            _actor?.Register();
        }

        private void OnDisable()
        {
            _actor?.Unregister();
        }
    }
}
