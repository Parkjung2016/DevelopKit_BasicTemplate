using System.Collections.Generic;
using System.Linq;
using PJDev.DevelopKit.BasicTemplate.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PJDev.DevelopKit.BasicTemplate.Editors
{
    public class CDebugTagFilterWindow : EditorWindow
    {
        private const string DefaultTagName = "Default";
        private const string FilterEnabledKey = "CDebug.TagFilter.Enabled";
        private const string TagEnabledKeyPrefix = "CDebug.Tag.";

        private CDebugTagDefinitions definitions;
        private List<string> pendingTags = new List<string>();
        private List<string> savedTags = new List<string>();

        private VisualElement pendingWarning;
        private VisualElement filterWarning;
        private Label tagCountLabel;
        private TextField newTagField;
        private VisualElement tagListContent;
        private Label tagEmptyLabel;
        private Button revertButton;
        private Button saveButton;
        private Toggle filterEnabledToggle;
        private VisualElement filterContent;
        private TextField filterSearchField;
        private VisualElement filterListContent;
        private Label filterEmptyLabel;

        [MenuItem("PJDev/CDebug Tag Filter", priority = -9600)]
        public static void ShowWindow()
        {
            var window = GetWindow<CDebugTagFilterWindow>("CDebug Tags");
            window.minSize = new Vector2(420f, 540f);
        }

        private void OnEnable()
        {
            CDebugTagFilterSetup.EnsureInitialized();
            LoadFromAsset();
        }

        public void CreateGUI()
        {
            rootVisualElement.Clear();
            rootVisualElement.style.flexGrow = 1;

            var scroll = new ScrollView(ScrollViewMode.Vertical) { style = { flexGrow = 1 } };
            rootVisualElement.Add(scroll);

            var content = new VisualElement();
            CDebugTagFilterUIStyles.ApplyRootPadding(content);
            scroll.Add(content);

            BuildHeader(content);
            BuildTagDefinitionGroup(content);
            BuildFilterGroup(content);

            filterEnabledToggle.SetValueWithoutNotify(EditorPrefs.GetBool(FilterEnabledKey, false));
            BindFilterContentState();
            RefreshUI();
        }

        private void BuildHeader(VisualElement parent)
        {
            var header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.justifyContent = Justify.SpaceBetween;
            header.style.alignItems = Align.FlexStart;
            header.style.marginBottom = 12;

            var textBlock = new VisualElement { style = { flexGrow = 1, marginRight = 12 } };

            var title = new Label("CDebug Tags");
            title.style.fontSize = 18;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            title.style.marginBottom = 4;

            var subtitle = new Label("태그 정의와 Console 필터를 한 곳에서 관리합니다.");
            subtitle.style.fontSize = 11;
            subtitle.style.whiteSpace = WhiteSpace.Normal;
            subtitle.style.color = new Color(0.65f, 0.65f, 0.65f);

            textBlock.Add(title);
            textBlock.Add(subtitle);

            tagCountLabel = new Label("0 tags");
            tagCountLabel.style.fontSize = 11;
            tagCountLabel.style.color = new Color(0.65f, 0.65f, 0.65f);
            tagCountLabel.style.backgroundColor = CDebugTagFilterUIStyles.GroupBackground;
            tagCountLabel.style.borderTopWidth = 1;
            tagCountLabel.style.borderBottomWidth = 1;
            tagCountLabel.style.borderLeftWidth = 1;
            tagCountLabel.style.borderRightWidth = 1;
            tagCountLabel.style.borderTopColor = CDebugTagFilterUIStyles.BorderColor;
            tagCountLabel.style.borderBottomColor = CDebugTagFilterUIStyles.BorderColor;
            tagCountLabel.style.borderLeftColor = CDebugTagFilterUIStyles.BorderColor;
            tagCountLabel.style.borderRightColor = CDebugTagFilterUIStyles.BorderColor;
            tagCountLabel.style.borderTopLeftRadius = 10;
            tagCountLabel.style.borderTopRightRadius = 10;
            tagCountLabel.style.borderBottomLeftRadius = 10;
            tagCountLabel.style.borderBottomRightRadius = 10;
            tagCountLabel.style.paddingTop = 4;
            tagCountLabel.style.paddingBottom = 4;
            tagCountLabel.style.paddingLeft = 10;
            tagCountLabel.style.paddingRight = 10;

            header.Add(textBlock);
            header.Add(tagCountLabel);
            parent.Add(header);

            pendingWarning = CDebugTagFilterUIStyles.CreateBanner(
                "저장되지 않은 태그 변경사항이 있습니다. Save Tags를 눌러야 CDebugTag enum이 갱신됩니다.");
            parent.Add(pendingWarning);
        }

        private void BuildTagDefinitionGroup(VisualElement parent)
        {
            var group = CDebugTagFilterUIStyles.CreateGroup(
                "Tag Definition",
                "추가/삭제는 임시 반영 · Save Tags 시 enum 생성",
                out VisualElement body);

            var inputRow = CDebugTagFilterUIStyles.CreateHorizontalRow();
            newTagField = new TextField("New Tag") { style = { flexGrow = 1, marginRight = 8 } };
            var addButton = CDebugTagFilterUIStyles.CreateButton("Add");
            addButton.clicked += AddPendingTag;
            inputRow.Add(newTagField);
            inputRow.Add(addButton);
            body.Add(inputRow);

            newTagField.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode != KeyCode.Return && evt.keyCode != KeyCode.KeypadEnter)
                    return;

                AddPendingTag();
                evt.StopPropagation();
            });

            CreateListScrollView(body, out tagListContent);
            tagEmptyLabel = CDebugTagFilterUIStyles.CreateEmptyLabel("태그가 없습니다.");
            body.Add(tagEmptyLabel);

            var actionRow = new VisualElement();
            actionRow.style.flexDirection = FlexDirection.Row;
            actionRow.style.justifyContent = Justify.SpaceBetween;
            actionRow.style.alignItems = Align.Center;
            actionRow.style.marginTop = 6;
            actionRow.style.paddingTop = 10;
            actionRow.style.borderTopWidth = 1;
            actionRow.style.borderTopColor = CDebugTagFilterUIStyles.BorderColor;

            revertButton = CDebugTagFilterUIStyles.CreateButton("Revert");
            saveButton = CDebugTagFilterUIStyles.CreateButton("Save Tags", primary: true);
            revertButton.clicked += RevertChanges;
            saveButton.clicked += SaveTagChanges;

            actionRow.Add(revertButton);
            actionRow.Add(saveButton);
            body.Add(actionRow);

            parent.Add(group);
        }

        private void BuildFilterGroup(VisualElement parent)
        {
            var group = CDebugTagFilterUIStyles.CreateGroup(
                "Console Filter",
                "Play Mode에서 선택한 태그의 CDebug 로그만 출력",
                out VisualElement body);

            filterEnabledToggle = new Toggle("Enable Tag Filter");
            filterEnabledToggle.style.marginBottom = 10;
            filterEnabledToggle.RegisterValueChangedCallback(_ => OnFilterEnabledChanged());
            body.Add(filterEnabledToggle);

            filterContent = new VisualElement();

            filterSearchField = new TextField("Search");
            filterSearchField.style.marginBottom = 8;
            filterSearchField.RegisterValueChangedCallback(_ => RefreshFilterList());
            filterContent.Add(filterSearchField);

            var buttonRow = CDebugTagFilterUIStyles.CreateHorizontalRow();
            var selectAllButton = CDebugTagFilterUIStyles.CreateButton("Select All");
            selectAllButton.style.flexGrow = 1;
            selectAllButton.style.marginRight = 6;
            selectAllButton.clicked += () => SetAllFilterTags(true);

            var clearAllButton = CDebugTagFilterUIStyles.CreateButton("Clear All");
            clearAllButton.style.flexGrow = 1;
            clearAllButton.clicked += () => SetAllFilterTags(false);

            buttonRow.Add(selectAllButton);
            buttonRow.Add(clearAllButton);
            filterContent.Add(buttonRow);

            CreateListScrollView(filterContent, out filterListContent);
            filterEmptyLabel = CDebugTagFilterUIStyles.CreateEmptyLabel("표시할 태그가 없습니다.");
            filterContent.Add(filterEmptyLabel);

            filterWarning = CDebugTagFilterUIStyles.CreateBanner("선택된 태그가 없어 CDebug 로그가 모두 숨겨집니다.");
            filterContent.Add(filterWarning);

            body.Add(filterContent);
            parent.Add(group);
        }

        private static void CreateListScrollView(VisualElement parent, out VisualElement content)
        {
            var scroll = CDebugTagFilterUIStyles.CreateListScrollView(out content);
            parent.Add(scroll);
        }

        private void OnFilterEnabledChanged()
        {
            EditorPrefs.SetBool(FilterEnabledKey, filterEnabledToggle.value);
            BindFilterContentState();
            RefreshFilterList();
        }

        private void BindFilterContentState()
        {
            if (filterContent == null)
                return;

            filterContent.SetEnabled(filterEnabledToggle.value);
            filterContent.style.opacity = filterEnabledToggle.value ? 1f : 0.45f;
        }

        private void RefreshUI()
        {
            if (tagListContent == null)
                return;

            SetVisible(pendingWarning, HasPendingChanges);
            CDebugTagFilterUIStyles.SetPrimaryDirty(saveButton, HasPendingChanges, HasPendingChanges);
            revertButton.SetEnabled(HasPendingChanges);

            tagCountLabel.text = $"{pendingTags.Count} tag{(pendingTags.Count == 1 ? string.Empty : "s")}";

            RefreshTagList();
            RefreshFilterList();
        }

        private void RefreshTagList()
        {
            tagListContent.Clear();

            if (pendingTags.Count == 0)
            {
                SetVisible(tagEmptyLabel, true);
                return;
            }

            SetVisible(tagEmptyLabel, false);

            foreach (var tag in pendingTags)
            {
                tagListContent.Add(CDebugTagFilterUIStyles.CreateTagRow(
                    tag,
                    IsPendingTagChange(tag),
                    tag == DefaultTagName,
                    () => RemovePendingTag(tag)));
            }
        }

        private void RefreshFilterList()
        {
            if (filterListContent == null)
                return;

            filterListContent.Clear();

            var visibleTags = GetFilteredTags().ToList();
            SetVisible(filterEmptyLabel, visibleTags.Count == 0);

            foreach (var tag in visibleTags)
            {
                var toggle = new Toggle(tag)
                {
                    value = EditorPrefs.GetBool(GetTagEnabledKey(tag), false)
                };
                toggle.style.marginBottom = 2;
                toggle.style.paddingTop = 2;
                toggle.style.paddingBottom = 2;
                toggle.style.paddingLeft = 4;
                toggle.style.paddingRight = 4;
                toggle.style.borderTopLeftRadius = 3;
                toggle.style.borderTopRightRadius = 3;
                toggle.style.borderBottomLeftRadius = 3;
                toggle.style.borderBottomRightRadius = 3;
                toggle.RegisterValueChangedCallback(evt =>
                {
                    EditorPrefs.SetBool(GetTagEnabledKey(tag), evt.newValue);
                    RefreshFilterWarning(visibleTags);
                });

                filterListContent.Add(toggle);
            }

            RefreshFilterWarning(visibleTags);
        }

        private void RefreshFilterWarning(IReadOnlyList<string> visibleTags)
        {
            if (filterWarning == null)
                return;

            var showWarning = filterEnabledToggle.value &&
                              visibleTags.Count > 0 &&
                              visibleTags.All(tag => !EditorPrefs.GetBool(GetTagEnabledKey(tag), false));

            SetVisible(filterWarning, showWarning);
        }

        private static void SetVisible(VisualElement element, bool visible)
        {
            if (element == null)
                return;

            element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private IEnumerable<string> GetFilteredTags()
        {
            var source = GetFilterTags();
            var search = filterSearchField?.value?.Trim();

            if (string.IsNullOrEmpty(search))
                return source;

            return source.Where(tag => tag.IndexOf(search, System.StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private IEnumerable<string> GetFilterTags()
        {
            return HasPendingChanges ? pendingTags : savedTags;
        }

        private bool HasPendingChanges => !pendingTags.SequenceEqual(savedTags);

        private bool IsPendingTagChange(string tag)
        {
            return HasPendingChanges && !savedTags.Contains(tag);
        }

        private void AddPendingTag()
        {
            if (!CDebugTagEnumGenerator.TryValidateNewTag(pendingTags, newTagField.value, out string normalized,
                    out string error))
            {
                EditorUtility.DisplayDialog("태그 추가 실패", error, "확인");
                return;
            }

            pendingTags.Add(normalized);
            newTagField.SetValueWithoutNotify(string.Empty);
            newTagField.Focus();
            RefreshUI();
        }

        private void RemovePendingTag(string tag)
        {
            if (tag == DefaultTagName)
                return;

            pendingTags.Remove(tag);
            RefreshUI();
        }

        private void SaveTagChanges()
        {
            CDebugTagEnumGenerator.ApplyAndGenerate(definitions, pendingTags);
            LoadFromAsset();
            ShowNotification(new GUIContent("Tags saved"));
        }

        private void RevertChanges()
        {
            pendingTags = new List<string>(savedTags);
            RefreshUI();
        }

        private void LoadFromAsset()
        {
            definitions = CDebugTagDefinitionsProvider.GetOrCreate();
            savedTags = definitions.Tags.ToList();
            pendingTags = new List<string>(savedTags);
            CDebugTagFilterSetup.LoadPersistedTags();
            RefreshUI();
        }

        private void SetAllFilterTags(bool enabled)
        {
            foreach (var tag in GetFilteredTags())
                EditorPrefs.SetBool(GetTagEnabledKey(tag), enabled);

            RefreshFilterList();
        }

        internal static string GetTagEnabledKey(string tag) => $"{TagEnabledKeyPrefix}{tag}.Enabled";

        internal static bool IsFilterEnabled() => EditorPrefs.GetBool(FilterEnabledKey, false);

        internal static bool IsTagEnabled(string tag) => EditorPrefs.GetBool(GetTagEnabledKey(tag), false);
    }
}
