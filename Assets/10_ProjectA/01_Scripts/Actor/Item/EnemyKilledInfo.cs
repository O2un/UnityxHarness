using UnityEngine;

namespace O2un.Actors
{
    public readonly struct EnemyKilledInfo
    {
        public Vector3 Position { get; }
        public int Exp { get; }

        public EnemyKilledInfo(Vector3 position, int exp)
        {
            Position = position;
            Exp = exp;
        }
    }
}
