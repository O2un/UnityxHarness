using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    public sealed class Player2DPlacer : MonoBehaviour, IPlayerPlacer
    {
        [SerializeField] private Rigidbody2D _body;

        private Rigidbody2D Body => _body ??= GetComponent<Rigidbody2D>();

        public void PlaceAt(Vector3 position)
        {
            // 이전 룸에서 이동하던 속도가 남으면 배치 직후 밀려나므로 같이 끊는다.
            if (null != Body)
            {
                Body.linearVelocity = Vector2.zero;
                Body.angularVelocity = 0f;
            }

            transform.position = position;
        }
    }
}
