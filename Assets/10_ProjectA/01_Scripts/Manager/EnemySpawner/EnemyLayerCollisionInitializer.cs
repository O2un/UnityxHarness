using UnityEngine;
using VContainer.Unity;

namespace O2un.Manager
{
    public sealed class EnemyLayerCollisionInitializer : IStartable
    {
        private const string ENEMY_LAYER_NAME = "Enemy";

        public void Start()
        {
            int enemyLayer = LayerMask.NameToLayer(ENEMY_LAYER_NAME);
            if (0 <= enemyLayer)
            {
                Physics.IgnoreLayerCollision(enemyLayer, enemyLayer, true);
            }
        }
    }
}
