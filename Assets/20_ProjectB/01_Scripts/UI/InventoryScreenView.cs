using System.Collections.Generic;
using O2un.DataStore;
using O2un.DI;
using O2un.Input;
using O2un.Manager;
using R3;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace O2un.ProjectB.Platformer
{
    public sealed class InventoryScreenView : MonoBehaviour, ISceneInitializable
    {
        private const string ROOT_NAME = "inventory-screen";
        private const string HEALTH_VALUE_NAME = "health-value";
        private const string ATTACK_VALUE_NAME = "attack-value";
        private const string MOVE_SPEED_VALUE_NAME = "move-speed-value";
        private const string ITEM_GRID_NAME = "acquired-item-cards";
        private const string SKILL_ICON_NAME = "selected-skill-icon";
        private const string SKILL_NAME_NAME = "selected-skill-name";
        private const string SKILL_LEVEL_NAME = "selected-skill-level";
        private const string SKILL_DESCRIPTION_NAME = "selected-skill-description";

        private const string SELECTED_CLASS = "item-card--selected";
        private const string EMPTY_SELECTION_TEXT = "Select a skill to view its description.";

        [SerializeField] private UIDocument _document;
        [SerializeField] private MeleeComboData _meleeData;
        [SerializeField] private PassiveSkillData _passiveData;

        [Inject] private IPlayerDataReader _playerReader;
        [Inject] private IPlayerStatReader _statReader;
        [Inject] private IInventoryReader _inventory;
        [Inject] private IInputReader _input;

        private VisualElement _root;
        private Label _healthValue;
        private Label _attackValue;
        private Label _moveSpeedValue;
        private VisualElement _itemGrid;
        private VisualElement _skillIcon;
        private Label _skillName;
        private Label _skillLevel;
        private Label _skillDescription;

        private readonly AcquiredItemGroupModule _grouping = new();
        private readonly SkillDescriptionModule _description = new();
        private readonly CompositeDisposable _disposables = new();

        private bool _injected;
        private bool _isOpen;
        private string _selectedId;

        public void Init()
        {
            if (null == _document)
            {
                Debug.LogError($"[InventoryScreenView] '{name}' UIDocument가 비어 있어 인벤토리를 바인딩하지 않습니다.");
                return;
            }

            _injected = true;
        }

        // Init()은 LifetimeScope.Awake 시점이라 rootVisualElement가 아직 없다. 바인딩은 Start까지 미룬다
        private void Start()
        {
            if (false == _injected)
            {
                return;
            }

            VisualElement documentRoot = _document.rootVisualElement;
            if (null == documentRoot)
            {
                Debug.LogError($"[InventoryScreenView] '{name}' rootVisualElement가 없어 인벤토리를 바인딩하지 않습니다.");
                return;
            }

            _root = documentRoot.Q<VisualElement>(ROOT_NAME);
            _healthValue = documentRoot.Q<Label>(HEALTH_VALUE_NAME);
            _attackValue = documentRoot.Q<Label>(ATTACK_VALUE_NAME);
            _moveSpeedValue = documentRoot.Q<Label>(MOVE_SPEED_VALUE_NAME);
            _itemGrid = documentRoot.Q<VisualElement>(ITEM_GRID_NAME);
            _skillIcon = documentRoot.Q<VisualElement>(SKILL_ICON_NAME);
            _skillName = documentRoot.Q<Label>(SKILL_NAME_NAME);
            _skillLevel = documentRoot.Q<Label>(SKILL_LEVEL_NAME);
            _skillDescription = documentRoot.Q<Label>(SKILL_DESCRIPTION_NAME);

            BindStatus();
            BindItems();
            BindToggle();

            SetOpen(false);
        }

        private void BindStatus()
        {
            if (null == _playerReader || null == _statReader)
            {
                Debug.LogWarning($"[InventoryScreenView] '{name}' 플레이어 데이터/스탯 미주입 — 스테이터스 표시를 건너뜁니다.");
                return;
            }

            if (null != _healthValue)
            {
                _playerReader.CurrentHP.Subscribe(_ => RefreshHealth()).AddTo(_disposables);
                _playerReader.MaxHP.Subscribe(_ => RefreshHealth()).AddTo(_disposables);
            }

            if (null != _attackValue)
            {
                _statReader.AttackBonus.Subscribe(RefreshAttack).AddTo(_disposables);
            }

            if (null != _moveSpeedValue)
            {
                _statReader.MoveSpeed.Subscribe(RefreshMoveSpeed).AddTo(_disposables);
            }
        }

        private void BindItems()
        {
            if (null == _itemGrid || null == _inventory)
            {
                Debug.LogWarning($"[InventoryScreenView] '{name}' 인벤토리 미주입 — 획득 아이템 표시를 건너뜁니다.");
                return;
            }

            _inventory.Slots.Subscribe(RefreshItems).AddTo(_disposables);
        }

        private void BindToggle()
        {
            if (null == _input)
            {
                Debug.LogWarning($"[InventoryScreenView] '{name}' 입력 미주입 — 인벤토리 토글을 건너뜁니다.");
                return;
            }

            _input.IsInventoryPressed.Subscribe(_ => SetOpen(false == _isOpen)).AddTo(_disposables);
        }

        private void RefreshHealth()
        {
            _healthValue.text = $"{_playerReader.CurrentHP.CurrentValue} / {_playerReader.MaxHP.CurrentValue}";
        }

        // 콤보 1타 기준 피해에 강화 보너스를 더한 값. 2·3타는 스테이지별 base가 달라 대표값으로 1타를 쓴다
        private void RefreshAttack(int attackBonus)
        {
            int baseDamage = null != _meleeData && _meleeData.StageCount > 0 ? _meleeData.GetStage(1).Damage : 0;
            _attackValue.text = Mathf.Max(1, baseDamage + attackBonus).ToString();
        }

        private void RefreshMoveSpeed(float moveSpeed)
        {
            _moveSpeedValue.text = moveSpeed.ToString("0.0");
        }

        private void RefreshItems(IReadOnlyList<InventorySlot> slots)
        {
            IReadOnlyList<AcquiredItemEntry> entries = _grouping.Group(slots);

            _itemGrid.Clear();

            AcquiredItemEntry selected = default;
            bool hasSelection = false;

            for (int i = 0; i < entries.Count; i++)
            {
                _itemGrid.Add(CreateCard(entries[i]));

                if (entries[i].Card.Id == _selectedId)
                {
                    selected = entries[i];
                    hasSelection = true;
                }
            }

            // 카드를 다시 먹으면 그리드가 재생성되므로 선택과 레벨 표기를 다시 맞춘다
            if (true == hasSelection)
            {
                Select(selected);
                return;
            }

            ClearSelection();
        }

        private VisualElement CreateCard(AcquiredItemEntry entry)
        {
            var card = new VisualElement();
            card.AddToClassList("item-card");

            if (entry.Card.Id == _selectedId)
            {
                card.AddToClassList(SELECTED_CLASS);
            }

            var icon = new VisualElement();
            icon.AddToClassList("item-icon");
            icon.pickingMode = PickingMode.Ignore;
            if (null != entry.Card.Icon)
            {
                icon.style.backgroundImage = new StyleBackground(entry.Card.Icon);
            }

            var itemName = new Label(entry.Card.DisplayName);
            itemName.AddToClassList("item-name");
            itemName.pickingMode = PickingMode.Ignore;

            var itemLevel = new Label($"Lv. {entry.Count}");
            itemLevel.AddToClassList("item-level");
            itemLevel.pickingMode = PickingMode.Ignore;

            card.Add(icon);
            card.Add(itemName);
            card.Add(itemLevel);

            card.RegisterCallback<ClickEvent>(_ => Select(entry));
            return card;
        }

        private void Select(AcquiredItemEntry entry)
        {
            _selectedId = entry.Card.Id;

            for (int i = 0; i < _itemGrid.childCount; i++)
            {
                _itemGrid[i].RemoveFromClassList(SELECTED_CLASS);
            }

            int index = IndexOfCard(entry.Card.Id);
            if (index >= 0)
            {
                _itemGrid[index].AddToClassList(SELECTED_CLASS);
            }

            if (null != _skillIcon)
            {
                _skillIcon.style.backgroundImage = null != entry.Card.Icon
                    ? new StyleBackground(entry.Card.Icon)
                    : new StyleBackground();
            }

            if (null != _skillName)
            {
                _skillName.text = entry.Card.DisplayName;
            }

            if (null != _skillLevel)
            {
                _skillLevel.text = _description.BuildLevelText(entry.Count);
            }

            if (null != _skillDescription)
            {
                _skillDescription.text = _description.BuildDescription(entry.Card, entry.Count, _passiveData);
            }
        }

        private int IndexOfCard(string id)
        {
            IReadOnlyList<AcquiredItemEntry> entries = _grouping.Entries;

            for (int i = 0; i < entries.Count && i < _itemGrid.childCount; i++)
            {
                if (entries[i].Card.Id == id)
                {
                    return i;
                }
            }

            return -1;
        }

        private void ClearSelection()
        {
            _selectedId = null;

            if (null != _skillIcon)
            {
                _skillIcon.style.backgroundImage = new StyleBackground();
            }

            if (null != _skillName)
            {
                _skillName.text = "Selected Skill";
            }

            if (null != _skillLevel)
            {
                _skillLevel.text = string.Empty;
            }

            if (null != _skillDescription)
            {
                _skillDescription.text = EMPTY_SELECTION_TEXT;
            }
        }

        private void SetOpen(bool isOpen)
        {
            _isOpen = isOpen;

            if (null != _root)
            {
                _root.style.display = true == isOpen ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
        }
    }
}
