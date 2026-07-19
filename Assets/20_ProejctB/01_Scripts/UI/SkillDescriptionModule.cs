using System.Text;

namespace O2un.ProjectB.Platformer
{
    public sealed class SkillDescriptionModule
    {
        private readonly StringBuilder _builder = new();

        public string BuildLevelText(int count)
        {
            return $"Level {count}";
        }

        // 작성된 설명 + 누적 수치를 합쳐 보여준다. 설명이 비어 있어도 효과 요약만으로 정보가 되도록 한다
        public string BuildDescription(IUpgradeCardData card, int count, PassiveSkillData passiveData)
        {
            if (null == card)
            {
                return string.Empty;
            }

            _builder.Clear();

            if (false == string.IsNullOrWhiteSpace(card.Description))
            {
                _builder.Append(card.Description);
            }

            string effect = BuildEffect(card, count, passiveData);
            if (false == string.IsNullOrEmpty(effect))
            {
                if (_builder.Length > 0)
                {
                    _builder.Append("\n\n");
                }

                _builder.Append(effect);
            }

            return _builder.ToString();
        }

        private string BuildEffect(IUpgradeCardData card, int count, PassiveSkillData passiveData)
        {
            if (UpgradeCardKind.PassiveSkill == card.Kind)
            {
                return BuildPassiveEffect(card.PassiveSkill, passiveData);
            }

            float total = card.ModifierValue * count;
            string statName = GetStatName(card.TargetStat);

            if (count > 1)
            {
                return $"{statName} +{FormatValue(card.TargetStat, total)}  ({FormatValue(card.TargetStat, card.ModifierValue)} × {count}중첩)";
            }

            return $"{statName} +{FormatValue(card.TargetStat, total)}";
        }

        private string BuildPassiveEffect(PassiveSkillType skill, PassiveSkillData data)
        {
            if (null == data)
            {
                return string.Empty;
            }

            if (PassiveSkillType.CriticalOnHit == skill)
            {
                return $"치명타 확률 {data.CriticalChance * 100f:0}% · 피해 {data.CriticalMultiplier:0.#}배";
            }

            return $"미사일 피해 {data.MissileDamage} · 재발사 간격 {data.MissileCooldown:0.##}초";
        }

        private string GetStatName(UpgradeStatType stat)
        {
            switch (stat)
            {
                case UpgradeStatType.AttackDamage:
                    return "공격력";

                case UpgradeStatType.MaxHealth:
                    return "최대 체력";

                default:
                    return "이동 속도";
            }
        }

        private string FormatValue(UpgradeStatType stat, float value)
        {
            return UpgradeStatType.MoveSpeed == stat ? value.ToString("0.#") : value.ToString("0");
        }
    }
}
