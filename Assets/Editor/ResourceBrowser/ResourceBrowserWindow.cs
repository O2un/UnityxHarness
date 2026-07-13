using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CommonFramework.EditorTools.ResourceBrowser
{
    public class ResourceBrowserWindow : EditorWindow
    {
        private const string DEFAULT_SEARCH_ROOT = "Assets";
        private const string DEFAULT_INVENTORY_PATH = "docs/resource-polish/resource-inventory.md";

        private const string UXML_PATH = "Assets/Editor/ResourceBrowser/ResourceBrowserWindow.uxml";
        private const string USS_PATH = "Assets/Editor/ResourceBrowser/ResourceBrowserWindow.uss";

        private enum ResourceKind
        {
            All,
            UIIcon,
            Model3D,
            Effect,
            Sound
        }

        private class ResourceEntry
        {
            public string Name;
            public string Path;
            public string TypeName;
            public ResourceKind Kind;
            public string Status;
        }

        private string _searchRoot = DEFAULT_SEARCH_ROOT;
        private string _inventoryPath = DEFAULT_INVENTORY_PATH;
        private string _searchText = string.Empty;
        private ResourceKind _kindFilter = ResourceKind.All;
        private bool _trackedOnly;

        private readonly List<ResourceEntry> _allEntries = new();
        private List<ResourceEntry> _filtered = new();
        private readonly List<InventoryRow> _inventoryRows = new();

        private class InventoryRow
        {
            public string TargetPath;
            public string DocStatus;
            public List<string> FileNames = new();
        }

        private ToolbarSearchField _searchField;
        private ToolbarMenu _typeMenu;
        private Toggle _trackedOnlyToggle;
        private TextField _rootField;
        private TextField _inventoryField;
        private ListView _listView;
        private Label _detailName;
        private Label _detailPath;
        private Label _detailType;
        private Label _detailStatus;
        private Button _pingButton;

        private ResourceEntry _selected;

        [MenuItem("Tools/Resource Browser")]
        public static void Open()
        {
            var window = GetWindow<ResourceBrowserWindow>();
            window.titleContent = new GUIContent("Resource Browser");
            window.minSize = new Vector2(560f, 360f);
        }

        public void CreateGUI()
        {
            var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UXML_PATH);
            if (tree == null)
            {
                rootVisualElement.Add(new Label($"UXML을 찾을 수 없습니다: {UXML_PATH}"));
                return;
            }

            tree.CloneTree(rootVisualElement);

            var style = AssetDatabase.LoadAssetAtPath<StyleSheet>(USS_PATH);
            if (style != null)
                rootVisualElement.styleSheets.Add(style);

            BindElements();
            RegisterCallbacks();
            Refresh();
        }

        private void BindElements()
        {
            _rootField = rootVisualElement.Q<TextField>("root-field");
            _inventoryField = rootVisualElement.Q<TextField>("inventory-field");
            _searchField = rootVisualElement.Q<ToolbarSearchField>("search");
            _typeMenu = rootVisualElement.Q<ToolbarMenu>("type-filter");
            _trackedOnlyToggle = rootVisualElement.Q<Toggle>("tracked-only");
            _listView = rootVisualElement.Q<ListView>("list");
            _detailName = rootVisualElement.Q<Label>("detail-name");
            _detailPath = rootVisualElement.Q<Label>("detail-path");
            _detailType = rootVisualElement.Q<Label>("detail-type");
            _detailStatus = rootVisualElement.Q<Label>("detail-status");
            _pingButton = rootVisualElement.Q<Button>("ping");

            _rootField.value = _searchRoot;
            _inventoryField.value = _inventoryPath;
            _trackedOnlyToggle.value = _trackedOnly;

            foreach (ResourceKind kind in Enum.GetValues(typeof(ResourceKind)))
            {
                var captured = kind;
                _typeMenu.menu.AppendAction(
                    KindLabel(kind),
                    _ => SetKindFilter(captured),
                    a => _kindFilter == captured ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal);
            }
            UpdateTypeMenuText();
        }

        private void RegisterCallbacks()
        {
            _rootField.RegisterValueChangedCallback(evt => _searchRoot = evt.newValue);
            _inventoryField.RegisterValueChangedCallback(evt => _inventoryPath = evt.newValue);
            _searchField.RegisterValueChangedCallback(evt =>
            {
                _searchText = evt.newValue ?? string.Empty;
                ApplyFilter();
            });
            _trackedOnlyToggle.RegisterValueChangedCallback(evt =>
            {
                _trackedOnly = evt.newValue;
                ApplyFilter();
            });

            var refresh = rootVisualElement.Q<ToolbarButton>("refresh");
            if (refresh != null)
                refresh.clicked += Refresh;

            _listView.selectionType = SelectionType.Single;
            _listView.makeItem = MakeRow;
            _listView.bindItem = BindRow;
            _listView.selectionChanged += OnSelectionChanged;

            _pingButton.clicked += PingSelected;
            _pingButton.SetEnabled(false);
        }

        private void SetKindFilter(ResourceKind kind)
        {
            _kindFilter = kind;
            UpdateTypeMenuText();
            ApplyFilter();
        }

        private void UpdateTypeMenuText()
        {
            _typeMenu.text = $"유형: {KindLabel(_kindFilter)}";
        }

        private static string KindLabel(ResourceKind kind) => kind switch
        {
            ResourceKind.All => "전체",
            ResourceKind.UIIcon => "UI Icon",
            ResourceKind.Model3D => "3D Model",
            ResourceKind.Effect => "Effect",
            ResourceKind.Sound => "Sound",
            _ => kind.ToString()
        };

        private void Refresh()
        {
            LoadInventory();
            CollectAssets();
            ApplyFilter();
        }

        private void LoadInventory()
        {
            _inventoryRows.Clear();

            var absolute = ResolveInventoryAbsolutePath(_inventoryField != null ? _inventoryField.value : _inventoryPath);
            if (string.IsNullOrEmpty(absolute) || !File.Exists(absolute))
                return;

            foreach (var line in File.ReadLines(absolute))
            {
                if (!line.TrimStart().StartsWith("|"))
                    continue;

                var status = ExtractDocStatus(line);
                if (status == null)
                    continue;

                var row = new InventoryRow { DocStatus = status };

                var cells = line.Split('|');
                foreach (var cell in cells)
                {
                    var text = cell.Trim().Trim('`').Trim();

                    foreach (Match m in Regex.Matches(text, @"[\w][\w/]*/[\w./]+"))
                    {
                        var candidate = NormalizeAssetPath(m.Value);
                        if (candidate.Contains("/") && row.TargetPath == null)
                            row.TargetPath = candidate;
                    }

                    foreach (Match m in Regex.Matches(text, @"[\w][\w.\- ]*\.\w{2,6}"))
                    {
                        var fileName = m.Value.Trim();
                        if (!row.FileNames.Contains(fileName))
                            row.FileNames.Add(fileName);
                    }
                }

                if (row.TargetPath != null || row.FileNames.Count > 0)
                    _inventoryRows.Add(row);
            }
        }

        private static string ExtractDocStatus(string line)
        {
            if (line.Contains("✅") || line.Contains("정상"))
                return "정상";
            if (line.Contains("🟡") || line.Contains("임시"))
                return "임시";
            if (line.Contains("🔴") || line.Contains("누락"))
                return "누락";
            if (line.Contains("⚪") || line.Contains("확인 필요"))
                return "확인 필요";
            return null;
        }

        private static string ResolveInventoryAbsolutePath(string relativeOrAbsolute)
        {
            if (string.IsNullOrEmpty(relativeOrAbsolute))
                return null;

            if (Path.IsPathRooted(relativeOrAbsolute))
                return relativeOrAbsolute;

            var projectRoot = Directory.GetParent(Application.dataPath)?.FullName;
            return projectRoot == null ? null : Path.GetFullPath(Path.Combine(projectRoot, relativeOrAbsolute));
        }

        private void CollectAssets()
        {
            _allEntries.Clear();

            var root = string.IsNullOrWhiteSpace(_searchRoot) ? DEFAULT_SEARCH_ROOT : _searchRoot.Trim();
            if (!AssetDatabase.IsValidFolder(root))
            {
                _detailStatus?.SetEnabled(true);
                return;
            }

            var guids = AssetDatabase.FindAssets(string.Empty, new[] { root });
            var seen = new HashSet<string>();

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path) || AssetDatabase.IsValidFolder(path))
                    continue;
                if (!seen.Add(path))
                    continue;

                var type = AssetDatabase.GetMainAssetTypeAtPath(path);
                var kind = ClassifyKind(path, type);
                if (kind == ResourceKind.All)
                    continue;

                _allEntries.Add(new ResourceEntry
                {
                    Name = Path.GetFileName(path),
                    Path = path,
                    TypeName = type != null ? type.Name : "Unknown",
                    Kind = kind,
                    Status = ResolveStatus(path)
                });
            }

            _allEntries.Sort((a, b) => string.CompareOrdinal(a.Path, b.Path));
        }

        private static ResourceKind ClassifyKind(string path, Type type)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();

            switch (ext)
            {
                case ".png":
                case ".jpg":
                case ".jpeg":
                case ".tga":
                case ".psd":
                case ".gif":
                case ".bmp":
                    return ResourceKind.UIIcon;
                case ".wav":
                case ".mp3":
                case ".ogg":
                case ".aiff":
                case ".aif":
                    return ResourceKind.Sound;
                case ".fbx":
                case ".obj":
                case ".blend":
                case ".dae":
                case ".mat":
                    return ResourceKind.Model3D;
            }

            if (type == typeof(Texture2D) || type == typeof(Sprite))
                return ResourceKind.UIIcon;
            if (type == typeof(AudioClip))
                return ResourceKind.Sound;
            if (type == typeof(Mesh) || type == typeof(Material) || (type == typeof(GameObject) && IsModelPath(path)))
                return ResourceKind.Model3D;

            if (ext == ".prefab")
            {
                if (IsEffectPath(path))
                    return ResourceKind.Effect;
                if (IsModelPath(path))
                    return ResourceKind.Model3D;
                return ResourceKind.All;
            }

            return ResourceKind.All;
        }

        private static bool IsModelPath(string path) =>
            path.IndexOf("51_3D", StringComparison.OrdinalIgnoreCase) >= 0 ||
            path.IndexOf("Model", StringComparison.OrdinalIgnoreCase) >= 0;

        private static bool IsEffectPath(string path) =>
            path.IndexOf("52_Effect", StringComparison.OrdinalIgnoreCase) >= 0 ||
            path.IndexOf("Effect", StringComparison.OrdinalIgnoreCase) >= 0 ||
            path.IndexOf("vfx", StringComparison.OrdinalIgnoreCase) >= 0;

        private string ResolveStatus(string path)
        {
            if (_inventoryRows.Count == 0)
                return "-";

            var full = NormalizeAssetPath(path);
            var fileName = Path.GetFileName(path);

            foreach (var row in _inventoryRows)
            {
                var nameMatch = row.FileNames.Any(f => string.Equals(f, fileName, StringComparison.OrdinalIgnoreCase));
                if (!nameMatch)
                    continue;

                if (string.IsNullOrEmpty(row.TargetPath))
                    return row.DocStatus;

                var target = row.TargetPath.TrimEnd('/');
                return full.Contains(target + "/", StringComparison.OrdinalIgnoreCase) || full.EndsWith(target, StringComparison.OrdinalIgnoreCase)
                    ? row.DocStatus
                    : "확인 필요";
            }

            return "-";
        }

        private static string NormalizeAssetPath(string path) =>
            path.Replace('\\', '/').Trim().TrimEnd('/');

        private void ApplyFilter()
        {
            IEnumerable<ResourceEntry> query = _allEntries;

            if (_kindFilter != ResourceKind.All)
                query = query.Where(e => e.Kind == _kindFilter);

            if (_trackedOnly)
                query = query.Where(e => e.Status != "-");

            if (!string.IsNullOrWhiteSpace(_searchText))
            {
                var needle = _searchText.Trim();
                query = query.Where(e =>
                    e.Name.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    e.Path.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            _filtered = query.ToList();
            _listView.itemsSource = _filtered;
            _listView.RefreshItems();

            _selected = null;
            ShowDetail(null);
        }

        private static VisualElement MakeRow()
        {
            var row = new VisualElement();
            row.AddToClassList("row");

            var name = new Label { name = "row-name" };
            name.AddToClassList("row-name");
            var type = new Label { name = "row-type" };
            type.AddToClassList("row-type");
            var status = new Label { name = "row-status" };
            status.AddToClassList("row-status");
            var path = new Label { name = "row-path" };
            path.AddToClassList("row-path");

            row.Add(name);
            row.Add(type);
            row.Add(status);
            row.Add(path);
            return row;
        }

        private void BindRow(VisualElement element, int index)
        {
            if (index < 0 || index >= _filtered.Count)
                return;

            var entry = _filtered[index];
            element.Q<Label>("row-name").text = entry.Name;
            element.Q<Label>("row-type").text = entry.TypeName;
            element.Q<Label>("row-path").text = entry.Path;

            var status = element.Q<Label>("row-status");
            status.text = entry.Status;
            status.EnableInClassList("status-warn", entry.Status == "확인 필요");
            status.EnableInClassList("status-missing", entry.Status == "누락");
            status.EnableInClassList("status-temp", entry.Status == "임시");
            status.EnableInClassList("status-none", entry.Status == "-");
        }

        private void OnSelectionChanged(IEnumerable<object> selection)
        {
            _selected = selection.FirstOrDefault() as ResourceEntry;
            ShowDetail(_selected);
        }

        private void ShowDetail(ResourceEntry entry)
        {
            if (entry == null)
            {
                _detailName.text = "-";
                _detailPath.text = string.Empty;
                _detailType.text = string.Empty;
                _detailStatus.text = string.Empty;
                _pingButton.SetEnabled(false);
                return;
            }

            _detailName.text = entry.Name;
            _detailPath.text = entry.Path;
            _detailType.text = $"타입: {entry.TypeName}";
            _detailStatus.text = $"상태: {entry.Status}";
            _pingButton.SetEnabled(true);
        }

        private void PingSelected()
        {
            if (_selected == null)
                return;

            var asset = AssetDatabase.LoadMainAssetAtPath(_selected.Path);
            if (asset == null)
                return;

            EditorUtility.FocusProjectWindow();
            EditorGUIUtility.PingObject(asset);
        }
    }
}
