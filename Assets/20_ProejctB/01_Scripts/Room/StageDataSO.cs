using System.Collections.Generic;
using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    [CreateAssetMenu(fileName = "StageData", menuName = "O2un/Room/StageData")]
    public sealed class StageDataSO : ScriptableObject
    {
        [SerializeField] private List<RoomDataSO> _rooms = new();
        [SerializeField] private float _fadeOutDuration = 0.3f;
        [SerializeField] private float _fadeInDuration = 0.3f;

        public IReadOnlyList<RoomDataSO> Rooms => _rooms;
        public float FadeOutDuration => _fadeOutDuration;
        public float FadeInDuration => _fadeInDuration;
    }
}
