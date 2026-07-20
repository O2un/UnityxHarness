using O2un.DataStore;
using O2un.DI;
using R3;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace O2un.ProjectB.Platformer
{
    public sealed class PlayerHudView : MonoBehaviour, ISceneInitializable
    {
        private const string HEALTH_FILL_NAME = "health-fill";
        private const string SKILL_COOLDOWN_NAME = "skill-cooldown-1";

        [SerializeField] private UIDocument _document;

        [Inject] private IPlayerDataReader _playerReader;
        [Inject] private IPlayerSkillStatusReader _skillStatus;

        private VisualElement _healthFill;
        private VisualElement _skillCooldown;

        private readonly CompositeDisposable _disposables = new();

        private bool _injected;

        public void Init()
        {
            if (null == _document)
            {
                Debug.LogError($"[PlayerHudView] '{name}' UIDocument가 비어 있어 HUD를 바인딩하지 않습니다.");
                return;
            }

            _injected = true;
        }

        // Init()은 LifetimeScope.Awake에서 불려 UIDocument의 rootVisualElement가 아직 없다. 바인딩은 Start까지 미룬다
        private void Start()
        {
            if (false == _injected)
            {
                return;
            }

            VisualElement root = _document.rootVisualElement;
            if (null == root)
            {
                Debug.LogError($"[PlayerHudView] '{name}' rootVisualElement가 없어 HUD를 바인딩하지 않습니다.");
                return;
            }

            _healthFill = root.Q<VisualElement>(HEALTH_FILL_NAME);
            _skillCooldown = root.Q<VisualElement>(SKILL_COOLDOWN_NAME);

            BindHealth();
            BindSkillCooldown();
        }

        private void BindHealth()
        {
            if (null == _healthFill || null == _playerReader)
            {
                Debug.LogWarning($"[PlayerHudView] '{name}' 체력 바인딩 대상이 없어 체력 표시를 건너뜁니다.");
                return;
            }

            _playerReader.CurrentHP.Subscribe(_ => RefreshHealth()).AddTo(_disposables);
            _playerReader.MaxHP.Subscribe(_ => RefreshHealth()).AddTo(_disposables);
        }

        private void BindSkillCooldown()
        {
            if (null == _skillCooldown || null == _skillStatus)
            {
                Debug.LogWarning($"[PlayerHudView] '{name}' 스킬 쿨다운 바인딩 대상이 없어 쿨다운 표시를 건너뜁니다.");
                return;
            }

            _skillStatus.RangedCooldownNormalized.Subscribe(RefreshSkillCooldown).AddTo(_disposables);
        }

        private void RefreshHealth()
        {
            int maxHP = _playerReader.MaxHP.CurrentValue;
            float ratio = 0 < maxHP ? (float)_playerReader.CurrentHP.CurrentValue / maxHP : 0f;

            _healthFill.style.width = Length.Percent(Mathf.Clamp01(ratio) * 100f);
        }

        private void RefreshSkillCooldown(float normalized)
        {
            _skillCooldown.style.height = Length.Percent(normalized * 100f);
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }
    }
}
