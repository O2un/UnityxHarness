using O2un.Actors;
using UnityEngine;

namespace O2un.Feedback
{
    [CreateAssetMenu(menuName = "O2un/Feedback/HitFeedbackData", fileName = "HitFeedbackData")]
    public sealed class HitFeedbackDataSO : ScriptableObject
    {
        // 플레이어 피격을 적 피격보다 강하게 주는 기본 차등. 배율이 아니라 두 필드 값의 차이로만 존재한다.
        [SerializeField] private HitFeedbackProfile _playerHitProfile = new(0.09f, 0.8f);
        [SerializeField] private HitFeedbackProfile _enemyHitProfile = new(0.06f, 0.4f);

        public HitFeedbackProfile GetProfile(ActorType team)
        {
            return ActorType.Player == team ? _playerHitProfile : _enemyHitProfile;
        }
    }
}
