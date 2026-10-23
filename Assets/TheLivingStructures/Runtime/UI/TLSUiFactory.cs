using System;
using UnityEngine;
using UnityEngine.UI;

namespace TLS.UI
{
    // Small dependency-free Unity uGUI helper; no prefab references or third-party assets.
    internal static class TLSUiFactory
    {
        internal static readonly Color Navy = new Color32(11, 20, 36, 255);
        internal static readonly Color Slate = new Color32(23, 42, 64, 255);
        internal static readonly Color Deep = new Color32(17, 32, 50, 255);
        internal static readonly Color Cyan = new Color32(56, 200, 244, 255);
        internal static readonly Color Violet = new Color32(167, 139, 250, 255);
        internal static readonly Color Amber = new Color32(242, 184, 75, 255);
        internal static readonly Color Pale = new Color32(235, 244, 250, 255);
        internal static readonly Color Muted = new Color32(172, 189, 204, 255);
        internal static readonly Color Green = new Color32(106, 225, 171, 255);

        private static Font font;
        internal static Font Font
        {
            get
            {
                if (font == null)
                {
                    font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    if (font == null)
                        font = Font.CreateDynamicFontFromOSFont(new[] { "Arial", "Liberation Sans" }, 16);
                }
                return font;
            }
        }

        internal static RectTransform Rect(Transform parent, string name, Vector2 min, Vector2 max)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rect = (RectTransform)go.transform;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
            return rect;
        }

        internal static RectTransform Panel(Transform parent, string name, Vector2 min, Vector2 max, Color color)
        {
            RectTransform rect = Rect(parent, name, min, max);
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            return rect;
        }

        internal static Text Label(Transform parent, string name, string value, Vector2 min, Vector2 max,
            int size = 18, Color? color = null, TextAnchor align = TextAnchor.MiddleLeft, bool bold = false)
        {
            RectTransform rect = Rect(parent, name, min, max);
            Text label = rect.gameObject.AddComponent<Text>();
            label.font = Font;
            label.fontSize = size;
            label.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
            label.alignment = align;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            label.color = color ?? Pale;
            label.text = value;
            label.raycastTarget = false;
            return label;
        }

        internal static Button MakeButton(Transform parent, string caption, Vector2 min, Vector2 max,
            Action click, Color? background = null, int fontSize = 17, bool enabled = true)
        {
            RectTransform rect = Panel(parent, "Button_" + caption, min, max, background ?? Slate);
            Button button = rect.gameObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.88f, 1f, 1f);
            colors.pressedColor = new Color(0.7f, 0.85f, 0.9f);
            colors.disabledColor = new Color(0.45f, 0.48f, 0.53f);
            colors.colorMultiplier = 1f;
            button.colors = colors;
            button.interactable = enabled;
            button.onClick.AddListener(() => { if (click != null) click(); });
            Label(rect, "Caption", caption, new Vector2(0.015f, 0.06f), new Vector2(0.985f, 0.94f),
                fontSize, Pale, TextAnchor.MiddleCenter, true);
            return button;
        }

        internal static InputField Field(Transform parent, string name, string text, string hint,
            Vector2 min, Vector2 max, Action<string> changed)
        {
            RectTransform root = Panel(parent, name, min, max, Deep);
            InputField field = root.gameObject.AddComponent<InputField>();
            RectTransform placeholderRect = Rect(root, "Placeholder", new Vector2(0.045f, 0.10f), new Vector2(0.955f, 0.90f));
            Text placeholder = placeholderRect.gameObject.AddComponent<Text>();
            placeholder.font = Font;
            placeholder.fontSize = 18;
            placeholder.alignment = TextAnchor.MiddleLeft;
            placeholder.color = Muted;
            placeholder.fontStyle = FontStyle.Italic;
            placeholder.text = hint;
            placeholder.raycastTarget = false;
            Text value = Label(root, "Text", "", new Vector2(0.045f, 0.10f), new Vector2(0.955f, 0.90f),
                19, Pale);
            field.textComponent = value;
            field.placeholder = placeholder;
            // Phase 8: set MultiLineNewline BEFORE initial source is inserted.
            // Older labs used to switch afterward and could lose embedded newlines.
            bool fullCEditor = name.IndexOf("C source", StringComparison.OrdinalIgnoreCase)>=0 ||
                name.IndexOf("student C editor", StringComparison.OrdinalIgnoreCase)>=0 ||
                name.IndexOf("C function editor", StringComparison.OrdinalIgnoreCase)>=0 ||
                name.IndexOf("WASM C editor input", StringComparison.OrdinalIgnoreCase)>=0;
            if (fullCEditor)
            {
                field.lineType = InputField.LineType.MultiLineNewline;
                field.textComponent.font = TLSPhase8CodeStyle.Font;
                field.textComponent.fontSize = 19;
                field.textComponent.lineSpacing = 1.12f;
                field.textComponent.alignment = TextAnchor.UpperLeft;
                field.textComponent.supportRichText = false;
                placeholder.alignment = TextAnchor.UpperLeft;
            }
            field.text = text;
            field.onValueChanged.AddListener(s => { if (changed != null) changed(s); });
            return field;
        }
    }
}
