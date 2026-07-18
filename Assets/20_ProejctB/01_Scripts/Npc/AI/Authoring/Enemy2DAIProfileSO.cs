using O2un.AI;
using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    public abstract class Enemy2DAIProfileSO : ScriptableObject
    {
        public abstract BaseEnemyAI Build(Enemy2DAIContext context);
    }
}
