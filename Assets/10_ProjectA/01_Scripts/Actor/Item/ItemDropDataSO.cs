using UnityEngine;

namespace O2un.Actors
{
    [CreateAssetMenu(menuName = "O2un/Item/ItemDropData", fileName = "ItemDropData")]
    public sealed class ItemDropDataSO : ScriptableObject
    {
        [SerializeField] private string _prefabKey;
        [SerializeField] private int _poolSize = 8;

        public string PrefabKey => _prefabKey;
        public int PoolSize => _poolSize;
    }
}
