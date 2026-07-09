using UnityEngine;

namespace O2un.Progression
{
    [CreateAssetMenu(menuName = "O2un/Progression/ExperienceData", fileName = "ExperienceData")]
    public sealed class ExperienceDataSO : ScriptableObject
    {
        [SerializeField] private AnimationCurve _requiredExpCurve;

        public AnimationCurve RequiredExpCurve => _requiredExpCurve;
    }
}
