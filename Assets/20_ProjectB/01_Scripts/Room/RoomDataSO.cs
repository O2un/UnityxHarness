using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    [CreateAssetMenu(fileName = "RoomData", menuName = "O2un/Room/RoomData")]
    public sealed class RoomDataSO : ScriptableObject
    {
        [SerializeField] private string _sceneKey;

        public string SceneKey => _sceneKey;
    }
}
