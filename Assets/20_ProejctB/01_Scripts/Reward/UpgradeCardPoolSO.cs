using System.Collections.Generic;
using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    [CreateAssetMenu(menuName = "ProjectB/Platformer/UpgradeCardPool")]
    public sealed class UpgradeCardPoolSO : ScriptableObject
    {
        [SerializeField] private List<UpgradeCardSO> _cards = new();

        public IReadOnlyList<UpgradeCardSO> Cards => _cards;
    }
}
