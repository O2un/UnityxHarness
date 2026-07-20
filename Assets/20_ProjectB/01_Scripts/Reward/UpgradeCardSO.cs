using O2un.Manager;
using UnityEngine;

namespace O2un.ProjectB.Platformer
{
    [CreateAssetMenu(menuName = "ProjectB/Platformer/UpgradeCard")]
    public sealed class UpgradeCardSO : ScriptableObject, IUpgradeCardData
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField, TextArea(2, 4)] private string _description;
        [SerializeField] private UpgradeCardKind _kind;
        [SerializeField] private Sprite _icon;

        [Header("StatModifier")]
        [SerializeField] private UpgradeStatType _targetStat;
        [SerializeField] private float _modifierValue;

        [Header("PassiveSkill")]
        [SerializeField] private PassiveSkillType _passiveSkill;

        public string Id => _id;
        public ItemCategory Category => ItemCategory.Passive;
        public int MaxStack => 1;
        public string IconKey => _id;

        public UpgradeCardKind Kind => _kind;
        public UpgradeStatType TargetStat => _targetStat;
        public float ModifierValue => _modifierValue;
        public PassiveSkillType PassiveSkill => _passiveSkill;
        public string DisplayName => _displayName;
        public string Description => _description;
        public Sprite Icon => _icon;
    }
}
