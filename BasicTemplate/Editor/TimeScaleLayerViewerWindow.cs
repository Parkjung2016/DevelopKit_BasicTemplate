using System;
using PJDev.DevelopKit.BasicTemplate.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PJDev.DevelopKit.BasicTemplate.Editors
{
    public sealed class TimeScaleLayerViewerWindow : EditorWindow
    {
        private Label playModeLabel;
        private Label timeScaleLabel;
        private Label effectiveScaleLabel;
        private Label layerCountLabel;
        private VisualElement listContent;
        private Label emptyLabel;
        private Toggle autoRefreshToggle;
        private double nextRefreshTime;

        [MenuItem("PJDev/Time Scale/Layer Viewer", priority = 120)]
        public static void Open()
        {
            var window = GetWindow<TimeScaleLayerViewerWindow>("Time Scale Layers");
            window.minSize = new Vector2(360f, 360f);
        }

        private void OnEnable()
        {
            EditorApplication.update += OnEditorUpdate;
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
        }

        public void CreateGUI()
        {
            rootVisualElement.Clear();
            rootVisualElement.style.flexGrow = 1;
            rootVisualElement.style.backgroundColor = EditorGUIUtility.isProSkin
                ? new Color(0.16f, 0.16f, 0.16f)
                : new Color(0.86f, 0.86f, 0.86f);

            var root = new VisualElement { style = { flexGrow = 1 } };
            CDebugTagFilterUIStyles.ApplyRootPadding(root);
            rootVisualElement.Add(root);

            BuildHeader(root);
            BuildSummary(root);
            BuildLayerList(root);
            Refresh();
        }

        private void BuildHeader(VisualElement parent)
        {
            var header = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Column,
                    alignItems = Align.Stretch,
                    marginBottom = 12
                }
            };

            var titleBlock = new VisualElement { style = { flexGrow = 1, marginBottom = 10 } };
            var title = new Label("Time Scale Layers")
            {
                style =
                {
                    fontSize = 18,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginBottom = 3
                }
            };
            var subtitle = new Label("Runtime TimeScaleLayerManager 상태를 실시간으로 확인합니다.")
            {
                style =
                {
                    fontSize = 11,
                    whiteSpace = WhiteSpace.Normal,
                    color = new Color(0.65f, 0.65f, 0.65f)
                }
            };
            titleBlock.Add(title);
            titleBlock.Add(subtitle);

            var actions = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    flexWrap = Wrap.Wrap,
                    alignItems = Align.Center
                }
            };
            autoRefreshToggle = new Toggle("Auto") { value = true, tooltip = "자동 새로고침" };
            autoRefreshToggle.style.height = 24;
            autoRefreshToggle.style.marginRight = 8;
            autoRefreshToggle.style.marginBottom = 6;
            actions.Add(autoRefreshToggle);

            var refreshButton = CreateHeaderButton("Refresh");
            refreshButton.clicked += Refresh;
            actions.Add(refreshButton);

            var clearButton = CreateHeaderButton("Clear Layers");
            clearButton.tooltip = "모든 TimeScale 레이어를 제거하고 Time.timeScale을 1로 되돌립니다.";
            clearButton.clicked += () =>
            {
                TimeScaleLayerManager.Instance.ClearLayers();
                Refresh();
            };
            actions.Add(clearButton);

            header.Add(titleBlock);
            header.Add(actions);
            parent.Add(header);
        }

        private static Button CreateHeaderButton(string text)
        {
            var button = CDebugTagFilterUIStyles.CreateButton(text);
            button.style.height = 24;
            button.style.minWidth = 0;
            button.style.marginRight = 6;
            button.style.marginBottom = 6;
            button.style.paddingLeft = 8;
            button.style.paddingRight = 8;
            button.style.flexShrink = 1;
            return button;
        }
        private void BuildSummary(VisualElement parent)
        {
            var grid = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    flexWrap = Wrap.Wrap,
                    marginBottom = 12
                }
            };

            playModeLabel = AddMetric(grid, "Mode", "Edit");
            timeScaleLabel = AddMetric(grid, "Unity Time.timeScale", "1.000");
            effectiveScaleLabel = AddMetric(grid, "Manager Effective", "1.000");
            layerCountLabel = AddMetric(grid, "Layers", "0");
            parent.Add(grid);
        }

        private void BuildLayerList(VisualElement parent)
        {
            VisualElement body;
            var group = CDebugTagFilterUIStyles.CreateGroup(
                "Active Layers",
                "우선순위가 가장 높은 레이어들 중 가장 낮은 Scale 값이 최종 Time.timeScale로 적용됩니다.",
                out body);

            AddListHeader(body);

            var scroll = new ScrollView(ScrollViewMode.Vertical)
            {
                style =
                {
                    flexGrow = 1,
                    minHeight = 170,
                    backgroundColor = CDebugTagFilterUIStyles.ListBackground,
                    borderTopWidth = 1,
                    borderBottomWidth = 1,
                    borderLeftWidth = 1,
                    borderRightWidth = 1,
                    borderTopColor = CDebugTagFilterUIStyles.BorderColor,
                    borderBottomColor = CDebugTagFilterUIStyles.BorderColor,
                    borderLeftColor = CDebugTagFilterUIStyles.BorderColor,
                    borderRightColor = CDebugTagFilterUIStyles.BorderColor,
                    borderTopLeftRadius = 6,
                    borderTopRightRadius = 6,
                    borderBottomLeftRadius = 6,
                    borderBottomRightRadius = 6
                }
            };
            listContent = new VisualElement { style = { paddingTop = 4, paddingBottom = 4 } };
            scroll.Add(listContent);
            body.Add(scroll);

            emptyLabel = new Label("No active time scale layers.")
            {
                style =
                {
                    unityTextAlign = TextAnchor.MiddleCenter,
                    color = new Color(0.62f, 0.62f, 0.62f),
                    paddingTop = 22,
                    paddingBottom = 22
                }
            };
            listContent.Add(emptyLabel);
            parent.Add(group);
        }

        private static Label AddMetric(VisualElement parent, string title, string value)
        {
            var card = new VisualElement
            {
                style =
                {
                    minWidth = 116,
                    flexGrow = 1,
                    marginRight = 8,
                    marginBottom = 8,
                    paddingTop = 9,
                    paddingBottom = 9,
                    paddingLeft = 10,
                    paddingRight = 10,
                    backgroundColor = CDebugTagFilterUIStyles.GroupBackground,
                    borderTopWidth = 1,
                    borderBottomWidth = 1,
                    borderLeftWidth = 1,
                    borderRightWidth = 1,
                    borderTopColor = CDebugTagFilterUIStyles.BorderColor,
                    borderBottomColor = CDebugTagFilterUIStyles.BorderColor,
                    borderLeftColor = CDebugTagFilterUIStyles.BorderColor,
                    borderRightColor = CDebugTagFilterUIStyles.BorderColor,
                    borderTopLeftRadius = 7,
                    borderTopRightRadius = 7,
                    borderBottomLeftRadius = 7,
                    borderBottomRightRadius = 7
                }
            };

            var titleLabel = new Label(title)
            {
                style =
                {
                    fontSize = 10,
                    color = new Color(0.62f, 0.62f, 0.62f),
                    marginBottom = 3
                }
            };
            var valueLabel = new Label(value)
            {
                style =
                {
                    fontSize = 16,
                    unityFontStyleAndWeight = FontStyle.Bold
                }
            };

            card.Add(titleLabel);
            card.Add(valueLabel);
            parent.Add(card);
            return valueLabel;
        }

        private static void AddListHeader(VisualElement parent)
        {
            var row = CreateRowBase();
            row.style.backgroundColor = CDebugTagFilterUIStyles.GroupHeaderBackground;
            row.style.borderBottomWidth = 1;
            row.style.borderBottomColor = CDebugTagFilterUIStyles.BorderColor;
            AddCell(row, "Key", 1f, true);
            AddCell(row, "Scale", 74f, true);
            AddCell(row, "Priority", 74f, true);
            AddCell(row, "Applied", 70f, true);
            parent.Add(row);
        }

        private void Refresh()
        {
            TimeScaleLayerManager manager = TimeScaleLayerManager.Instance;
            TimeScaleLayerSnapshot[] snapshots = manager.GetLayerSnapshots();
            Array.Sort(snapshots, CompareSnapshots);

            playModeLabel.text = EditorApplication.isPlaying ? "Play" : "Edit";
            timeScaleLabel.text = Time.timeScale.ToString("0.###");
            effectiveScaleLabel.text = manager.EffectiveScale.ToString("0.###");
            layerCountLabel.text = snapshots.Length.ToString();

            listContent.Clear();
            if (snapshots.Length == 0)
            {
                listContent.Add(emptyLabel);
                return;
            }

            for (int i = 0; i < snapshots.Length; i++)
                listContent.Add(CreateLayerRow(snapshots[i], i));
        }

        private VisualElement CreateLayerRow(TimeScaleLayerSnapshot snapshot, int index)
        {
            var row = CreateRowBase();
            row.style.backgroundColor = snapshot.IsEffective
                ? new Color(0.22f, 0.42f, 0.68f, EditorGUIUtility.isProSkin ? 0.72f : 0.34f)
                : index % 2 == 0
                    ? CDebugTagFilterUIStyles.RowBackground
                    : CDebugTagFilterUIStyles.ListBackground;

            AddCell(row, snapshot.Key, 1f, false);
            AddCell(row, snapshot.Scale.ToString("0.###"), 74f, false);
            AddCell(row, snapshot.Priority.ToString(), 74f, false);
            AddCell(row, snapshot.IsEffective ? "Yes" : "", 70f, false);
            return row;
        }

        private static VisualElement CreateRowBase()
        {
            return new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    minHeight = 28,
                    paddingLeft = 8,
                    paddingRight = 8
                }
            };
        }

        private static void AddCell(VisualElement row, string text, float widthOrGrow, bool header)
        {
            var label = new Label(text)
            {
                style =
                {
                    fontSize = header ? 10 : 11,
                    unityFontStyleAndWeight = header ? FontStyle.Bold : FontStyle.Normal,
                    color = header ? new Color(0.7f, 0.7f, 0.7f) : Color.white,
                    overflow = Overflow.Hidden,
                    textOverflow = TextOverflow.Ellipsis
                }
            };

            if (widthOrGrow <= 1f)
            {
                label.style.flexGrow = widthOrGrow;
                label.style.minWidth = 120;
            }
            else
            {
                label.style.width = widthOrGrow;
                label.style.flexShrink = 0;
                label.style.unityTextAlign = TextAnchor.MiddleCenter;
            }

            row.Add(label);
        }

        private static int CompareSnapshots(TimeScaleLayerSnapshot a, TimeScaleLayerSnapshot b)
        {
            int priority = b.Priority.CompareTo(a.Priority);
            if (priority != 0)
                return priority;

            int scale = a.Scale.CompareTo(b.Scale);
            return scale != 0 ? scale : string.Compare(a.Key, b.Key, StringComparison.OrdinalIgnoreCase);
        }

        private void OnEditorUpdate()
        {
            if (autoRefreshToggle == null || !autoRefreshToggle.value)
                return;

            if (EditorApplication.timeSinceStartup < nextRefreshTime)
                return;

            nextRefreshTime = EditorApplication.timeSinceStartup + 0.15d;
            Refresh();
        }
    }
}

