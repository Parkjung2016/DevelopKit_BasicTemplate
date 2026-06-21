using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PJDev.DevelopKit.BasicTemplate.Editors
{
    internal static class CDebugTagFilterUiStyles
    {
        internal static Color BorderColor => EditorGUIUtility.isProSkin
            ? new Color(0.18f, 0.18f, 0.18f)
            : new Color(0.6f, 0.6f, 0.6f);

        internal static Color GroupBackground => EditorGUIUtility.isProSkin
            ? new Color(0.21f, 0.21f, 0.21f)
            : new Color(0.88f, 0.88f, 0.88f);

        internal static Color GroupHeaderBackground => EditorGUIUtility.isProSkin
            ? new Color(0.26f, 0.26f, 0.26f)
            : new Color(0.82f, 0.82f, 0.82f);

        internal static Color ListBackground => EditorGUIUtility.isProSkin
            ? new Color(0.17f, 0.17f, 0.17f)
            : new Color(0.93f, 0.93f, 0.93f);

        internal static Color RowBackground => EditorGUIUtility.isProSkin
            ? new Color(0.24f, 0.24f, 0.24f)
            : new Color(0.96f, 0.96f, 0.96f);

        internal static Color PendingHighlight => new Color(0.95f, 0.78f, 0.35f);
        internal static Color PrimaryButton => new Color(0.23f, 0.47f, 0.77f);
        internal static Color PrimaryButtonDirty => new Color(0.18f, 0.41f, 0.70f);
        internal static Color WarningBackground => EditorGUIUtility.isProSkin
            ? new Color(0.35f, 0.28f, 0.10f)
            : new Color(1f, 0.95f, 0.78f);

        internal static void ApplyRootPadding(VisualElement element)
        {
            element.style.paddingTop = 12;
            element.style.paddingBottom = 16;
            element.style.paddingLeft = 14;
            element.style.paddingRight = 14;
        }

        internal static VisualElement CreateGroup(string title, string description, out VisualElement body)
        {
            var group = new VisualElement();
            group.style.backgroundColor = GroupBackground;
            group.style.borderTopWidth = 1;
            group.style.borderBottomWidth = 1;
            group.style.borderLeftWidth = 1;
            group.style.borderRightWidth = 1;
            group.style.borderTopColor = BorderColor;
            group.style.borderBottomColor = BorderColor;
            group.style.borderLeftColor = BorderColor;
            group.style.borderRightColor = BorderColor;
            group.style.borderTopLeftRadius = 8;
            group.style.borderTopRightRadius = 8;
            group.style.borderBottomLeftRadius = 8;
            group.style.borderBottomRightRadius = 8;
            group.style.marginBottom = 14;
            group.style.overflow = Overflow.Hidden;

            var header = new VisualElement();
            header.style.backgroundColor = GroupHeaderBackground;
            header.style.paddingTop = 10;
            header.style.paddingBottom = 10;
            header.style.paddingLeft = 12;
            header.style.paddingRight = 12;
            header.style.borderBottomWidth = 1;
            header.style.borderBottomColor = BorderColor;

            var titleLabel = new Label(title);
            titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            titleLabel.style.fontSize = 13;
            titleLabel.style.marginBottom = 2;

            var descLabel = new Label(description);
            descLabel.style.fontSize = 11;
            descLabel.style.whiteSpace = WhiteSpace.Normal;
            descLabel.style.color = new Color(0.65f, 0.65f, 0.65f);

            header.Add(titleLabel);
            header.Add(descLabel);

            body = new VisualElement();
            body.style.paddingTop = 12;
            body.style.paddingBottom = 12;
            body.style.paddingLeft = 12;
            body.style.paddingRight = 12;

            group.Add(header);
            group.Add(body);
            return group;
        }

        internal static VisualElement CreateBanner(string message)
        {
            var banner = new VisualElement();
            banner.style.backgroundColor = WarningBackground;
            banner.style.borderTopWidth = 1;
            banner.style.borderBottomWidth = 1;
            banner.style.borderLeftWidth = 1;
            banner.style.borderRightWidth = 1;
            banner.style.borderTopColor = new Color(0.95f, 0.70f, 0.20f, 0.55f);
            banner.style.borderBottomColor = new Color(0.95f, 0.70f, 0.20f, 0.55f);
            banner.style.borderLeftWidth = 3;
            banner.style.borderLeftColor = new Color(0.95f, 0.70f, 0.20f);
            banner.style.borderRightColor = new Color(0.95f, 0.70f, 0.20f, 0.55f);
            banner.style.borderTopLeftRadius = 6;
            banner.style.borderTopRightRadius = 6;
            banner.style.borderBottomLeftRadius = 6;
            banner.style.borderBottomRightRadius = 6;
            banner.style.paddingTop = 10;
            banner.style.paddingBottom = 10;
            banner.style.paddingLeft = 12;
            banner.style.paddingRight = 12;
            banner.style.marginBottom = 12;
            banner.style.display = DisplayStyle.None;

            var label = new Label(message);
            label.style.whiteSpace = WhiteSpace.Normal;
            label.style.fontSize = 11;
            banner.Add(label);
            return banner;
        }

        internal static ScrollView CreateListScrollView(out VisualElement content, float minHeight = 150f)
        {
            var scroll = new ScrollView(ScrollViewMode.Vertical);
            scroll.style.minHeight = minHeight;
            scroll.style.maxHeight = 260;
            scroll.style.backgroundColor = ListBackground;
            scroll.style.borderTopWidth = 1;
            scroll.style.borderBottomWidth = 1;
            scroll.style.borderLeftWidth = 1;
            scroll.style.borderRightWidth = 1;
            scroll.style.borderTopColor = BorderColor;
            scroll.style.borderBottomColor = BorderColor;
            scroll.style.borderLeftColor = BorderColor;
            scroll.style.borderRightColor = BorderColor;
            scroll.style.borderTopLeftRadius = 6;
            scroll.style.borderTopRightRadius = 6;
            scroll.style.borderBottomLeftRadius = 6;
            scroll.style.borderBottomRightRadius = 6;
            scroll.style.marginBottom = 8;

            content = new VisualElement();
            content.style.paddingTop = 6;
            content.style.paddingBottom = 6;
            content.style.paddingLeft = 6;
            content.style.paddingRight = 6;
            scroll.Add(content);
            return scroll;
        }

        internal static VisualElement CreateHorizontalRow()
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.FlexEnd;
            row.style.marginBottom = 10;
            return row;
        }

        internal static Button CreateButton(string text, bool primary = false)
        {
            var button = new Button { text = text };
            button.style.height = 26;
            button.style.minWidth = 72;
            button.style.borderTopLeftRadius = 4;
            button.style.borderTopRightRadius = 4;
            button.style.borderBottomLeftRadius = 4;
            button.style.borderBottomRightRadius = 4;

            if (primary)
            {
                button.style.backgroundColor = PrimaryButton;
                button.style.color = Color.white;
                button.style.unityFontStyleAndWeight = FontStyle.Bold;
                button.style.minWidth = 110;
            }

            return button;
        }

        internal static VisualElement CreateTagRow(string tag, bool isPending, bool isDefault, System.Action onRemove)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.backgroundColor = RowBackground;
            row.style.borderTopLeftRadius = 5;
            row.style.borderTopRightRadius = 5;
            row.style.borderBottomLeftRadius = 5;
            row.style.borderBottomRightRadius = 5;
            row.style.paddingTop = 8;
            row.style.paddingBottom = 8;
            row.style.paddingLeft = 10;
            row.style.paddingRight = 10;
            row.style.marginBottom = 4;

            if (isPending)
            {
                row.style.borderTopWidth = 1;
                row.style.borderBottomWidth = 1;
                row.style.borderLeftWidth = 1;
                row.style.borderRightWidth = 1;
                row.style.borderTopColor = new Color(PendingHighlight.r, PendingHighlight.g, PendingHighlight.b, 0.5f);
                row.style.borderBottomColor = new Color(PendingHighlight.r, PendingHighlight.g, PendingHighlight.b, 0.5f);
                row.style.borderLeftColor = new Color(PendingHighlight.r, PendingHighlight.g, PendingHighlight.b, 0.5f);
                row.style.borderRightColor = new Color(PendingHighlight.r, PendingHighlight.g, PendingHighlight.b, 0.5f);
            }

            var label = new Label(tag);
            label.style.flexGrow = 1;
            label.style.fontSize = 12;
            if (isPending)
            {
                label.style.color = PendingHighlight;
                label.style.unityFontStyleAndWeight = FontStyle.Bold;
            }

            row.Add(label);

            if (isDefault)
            {
                var badge = new Label("Required");
                badge.style.fontSize = 10;
                badge.style.color = new Color(0.65f, 0.65f, 0.65f);
                badge.style.backgroundColor = GroupBackground;
                badge.style.paddingLeft = 8;
                badge.style.paddingRight = 8;
                badge.style.paddingTop = 2;
                badge.style.paddingBottom = 2;
                badge.style.borderTopLeftRadius = 4;
                badge.style.borderTopRightRadius = 4;
                badge.style.borderBottomLeftRadius = 4;
                badge.style.borderBottomRightRadius = 4;
                row.Add(badge);
            }
            else
            {
                var removeButton = CreateButton("Remove");
                removeButton.style.height = 22;
                removeButton.style.minWidth = 68;
                removeButton.style.fontSize = 11;
                removeButton.clicked += onRemove;
                row.Add(removeButton);
            }

            return row;
        }

        internal static Label CreateEmptyLabel(string text)
        {
            var label = new Label(text);
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            label.style.color = new Color(0.55f, 0.55f, 0.55f);
            label.style.fontSize = 11;
            label.style.paddingTop = 20;
            label.style.paddingBottom = 20;
            label.style.marginBottom = 8;
            label.style.display = DisplayStyle.None;
            return label;
        }

        internal static void SetPrimaryDirty(Button button, bool dirty, bool enabled)
        {
            button.SetEnabled(enabled);
            button.style.backgroundColor = dirty && enabled ? PrimaryButtonDirty : PrimaryButton;
            button.style.color = enabled ? Color.white : new Color(0.55f, 0.55f, 0.55f);
            button.style.unityFontStyleAndWeight = dirty && enabled ? FontStyle.Bold : FontStyle.Normal;
        }
    }
}
