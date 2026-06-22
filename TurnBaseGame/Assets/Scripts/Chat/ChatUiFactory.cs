using RTLTMPro;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nakama.Helpers
{
    /// <summary>
    /// Procedural UI primitives for the chat drawer. The project no longer ships a UI
    /// kit (the HONETi panels were deleted), so panels, bubbles and round buttons are
    /// generated at runtime as anti-aliased rounded sprites — zero art-asset dependency,
    /// works identically in every scene. Sprites are cached and tinted via Image.color.
    /// </summary>
    public static class ChatUiFactory
    {
        private static Sprite _rounded;
        private static Sprite _circle;

        public static TMP_FontAsset Font; // assigned once by ChatManager (Persian SDF from the scene)

        /// <summary>Drop cached sprite refs (call after (re)baking the sprite assets in the editor).</summary>
        public static void ClearCache() { _rounded = null; _circle = null; }

        // ── Sprites ───────────────────────────────────────────────────────────────

        // Persisted sprite assets (baked by the chat authoring editor tool) so authored
        // scene/prefab Images keep a valid reference. Falls back to runtime generation.
        public const int RoundedRadius = 28;
        public const string RoundedResource = "Chat/chat_rounded";
        public const string CircleResource = "Chat/chat_circle";

        /// <summary>9-sliced rounded rectangle (white). Tint with Image.color.</summary>
        public static Sprite RoundedRect()
        {
            if (_rounded != null) return _rounded;
            _rounded = Resources.Load<Sprite>(RoundedResource);
            if (_rounded == null) _rounded = GenerateRounded();
            return _rounded;
        }

        /// <summary>Solid filled circle (white). Tint with Image.color.</summary>
        public static Sprite Circle()
        {
            if (_circle != null) return _circle;
            _circle = Resources.Load<Sprite>(CircleResource);
            if (_circle == null) _circle = GenerateCircle();
            return _circle;
        }

        // Runtime-generated fallbacks (used only if the baked assets are missing).
        public static Sprite GenerateRounded()
        {
            const int r = RoundedRadius;
            int size = r * 2 + 6;
            var tex = BuildRoundedTexture(size, size, r);
            var s = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f),
                100f, 0, SpriteMeshType.FullRect, new Vector4(r, r, r, r));
            s.name = "ChatRoundedRect";
            return s;
        }

        public static Sprite GenerateCircle()
        {
            const int size = 96;
            var tex = BuildRoundedTexture(size, size, size / 2);
            var s = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            s.name = "ChatCircle";
            return s;
        }

        public static Texture2D BuildRoundedTexturePublic(int w, int h, float radius) =>
            BuildRoundedTexture(w, h, radius);

        private static Texture2D BuildRoundedTexture(int w, int h, float radius)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            var px = new Color32[w * h];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    // Distance to the nearest "corner center" (clamped inside the rounded inset).
                    float cx = Mathf.Clamp(x, radius, w - 1 - radius);
                    float cy = Mathf.Clamp(y, radius, h - 1 - radius);
                    float dx = x - cx, dy = y - cy;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float a = Mathf.Clamp01(radius + 0.5f - dist); // 1px anti-aliased edge
                    px[y * w + x] = new Color32(255, 255, 255, (byte)(a * 255));
                }
            }
            tex.SetPixels32(px);
            tex.Apply();
            return tex;
        }

        // ── Elements ──────────────────────────────────────────────────────────────

        public static RectTransform Rect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            return rt;
        }

        public static Image Panel(string name, Transform parent, Color color, bool rounded = true)
        {
            var rt = Rect(name, parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = rounded ? RoundedRect() : null;
            img.type = Image.Type.Sliced;
            img.color = color;
            return img;
        }

        public static RTLTextMeshPro Text(string name, Transform parent, string text, int size,
            Color color, TextAlignmentOptions align = TextAlignmentOptions.MidlineRight)
        {
            var rt = Rect(name, parent);
            var t = rt.gameObject.AddComponent<RTLTextMeshPro>();
            if (Font != null) t.font = Font;
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.richText = false;
            t.text = text;
            return t;
        }

        /// <summary>
        /// Plain (non-RTL) TMP text. Use for an editable TMP_InputField text component —
        /// RTLTMPro reshapes on every change and corrupts in-progress editing/caret.
        /// </summary>
        public static TextMeshProUGUI PlainText(string name, Transform parent, string text, int size,
            Color color, TextAlignmentOptions align = TextAlignmentOptions.MidlineRight)
        {
            var rt = Rect(name, parent);
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            if (Font != null) t.font = Font;
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.richText = false;
            t.text = text;
            return t;
        }

        /// <summary>Round icon button (circle). The glyph is a TMP label drawn on top.</summary>
        public static Button RoundButton(string name, Transform parent, Color bg, string glyph,
            Color glyphColor, int glyphSize, float diameter)
        {
            var rt = Rect(name, parent);
            rt.sizeDelta = new Vector2(diameter, diameter);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = Circle();
            img.color = bg;
            var btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;

            if (!string.IsNullOrEmpty(glyph))
            {
                var label = Text(name + "_Glyph", rt, glyph, glyphSize, glyphColor, TextAlignmentOptions.Center);
                Stretch(label.rectTransform);
            }
            return btn;
        }

        /// <summary>Rounded pill button with a label (used for presets / send).</summary>
        public static Button PillButton(string name, Transform parent, Color bg, string label,
            Color labelColor, int labelSize, out RTLTextMeshPro labelRef)
        {
            var rt = Rect(name, parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = RoundedRect();
            img.type = Image.Type.Sliced;
            img.color = bg;
            var btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;

            labelRef = Text(name + "_Label", rt, label, labelSize, labelColor, TextAlignmentOptions.Center);
            var lr = labelRef.rectTransform;
            lr.anchorMin = Vector2.zero; lr.anchorMax = Vector2.one;
            lr.offsetMin = new Vector2(16, 6); lr.offsetMax = new Vector2(-16, -6);
            return btn;
        }

        // ── Layout helpers ──────────────────────────────────────────────────────────

        public static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        public static void Anchor(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 pivot, Vector2 anchoredPos, Vector2 size)
        {
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
        }
    }
}
