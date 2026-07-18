using O2un.AI;
using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    public abstract class Enemy2DStateSO : ScriptableObject
    {
        public abstract IState Build(Enemy2DAIContext context);
    }
}
