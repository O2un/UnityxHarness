using UnityEngine;

namespace O2un.Manager
{
    public sealed class CharacterControllerSpawnPlacer : ISpawnPlacer
    {
        public void Place(EnemyContext enemy, Vector3 position)
        {
            if (null == enemy)
            {
                return;
            }

            Transform target = enemy.transform;
            CharacterController controller = target.GetComponent<CharacterController>();
            if (null == controller)
            {
                target.position = position;
                return;
            }

            controller.enabled = false;
            target.SetPositionAndRotation(position, Quaternion.identity);
            controller.enabled = true;
        }
    }
}
