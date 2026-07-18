using UnityEngine;

namespace O2un.Manager
{
    public interface ISpawnPlacer
    {
        void Place(EnemyContext enemy, Vector3 position);
    }

    public sealed class DefaultSpawnPlacer : ISpawnPlacer
    {
        public void Place(EnemyContext enemy, Vector3 position)
        {
            if (null == enemy)
            {
                return;
            }

            enemy.transform.SetPositionAndRotation(position, Quaternion.identity);
        }
    }
}
