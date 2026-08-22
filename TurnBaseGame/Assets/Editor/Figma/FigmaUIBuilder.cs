// FigmaUIBuilder.cs
// تبدیل درخت نودهای فیگما به سلسله‌مراتب uGUI در یونیتی
// محل قرارگیری: Assets/Editor/Figma/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace FigmaImport
{
    public static class FigmaUIBuilder
    {
        // ================= تنظیمات =================

        [Serializable]
        public class Options
        {
            public Vector2 referenceResolution = new Vector2(1080, 2400);
            public float matchWidthOrHeight = 0.5f;    // هماهنگ با HomeMenuAuthoring پروژه
            public bool touchCanvasScaler = false;      // اگر Canvas از قبل تنظیم شده، دست نزن

            public int imageScale = 2;                  // 1 | 2 | 3 | 4
            public string imageFormat = "png";          // png | jpg | svg

            public bool rasterizeInstances = false;     // کامپوننت/اینستنس‌ها را یک‌تکه عکس بگیر
            public bool createButtons = true;           // نودهایی با نام btn/button → Button
            public bool addRectMask = true;             // clipsContent → RectMask2D
            public bool skipHiddenNodes = true;
            public bool savePrefab = true;

            public string imagesFolder = "Assets/Figma/Images";
            public string prefabsFolder = "Assets/Figma/Prefabs";

            /// <summary>
            /// اگر خالی باشد، در کل پروژه دنبال TMP_FontAsset می‌گردد.
            /// اگر پر باشد، فقط داخل آن پوشه (مثلاً "Assets/Font").
            /// </summary>
            public string fontsSearchFolder = "";

            /// <summary>اگر ست شود، همه‌ی متن‌های فارسی این فونت را می‌گیرند (نام فیگما نادیده گرفته می‌شود).</summary>
            public TMP_FontAsset persianFontOverride;

            /// <summary>اگر ست شود، همه‌ی متن‌های لاتین این فونت را می‌گیرند.</summary>
            public TMP_FontAsset latinFontOverride;
        }

        // ================= نقطه ورود =================

        public static async Task<GameObject> BuildScreenAsync(
            FigmaNode root, Transform parent, FigmaApi api, Options opt,
            Action<string, float> progress = null)
        {
            if (root == null) throw new ArgumentNullException(nameof(root));

            // ۱) تصمیم بگیر کدام نودها باید به صورت تصویر دانلود شوند
            var toRasterize = new List<FigmaNode>();
            CollectRasterizable(root, opt, toRasterize, isRoot: true);

            // ۲) URL بگیر و دانلود کن
            var sprites = new Dictionary<string, Sprite>();
            if (toRasterize.Count > 0)
            {
                progress?.Invoke($"Preparing {toRasterize.Count} image(s)...", 0.05f);

                var ids = toRasterize.Select(n => n.id).Distinct().ToList();
                var urls = await api.GetImageUrlsAsync(ids, opt.imageScale, opt.imageFormat, progress);

                Directory.CreateDirectory(opt.imagesFolder);

                int done = 0;
                foreach (var node in toRasterize)
                {
                    done++;
                    if (!urls.TryGetValue(FigmaApi.NormalizeId(node.id), out var url) || string.IsNullOrEmpty(url))
                        continue;

                    progress?.Invoke($"Downloading image {done}/{toRasterize.Count}: {node.name}",
                                     0.1f + 0.7f * done / toRasterize.Count);

                    var bytes = await api.DownloadAsync(url);
                    if (bytes == null || bytes.Length == 0) continue;

                    var sprite = SaveAsSprite(bytes, node, opt);
                    if (sprite != null) sprites[node.id] = sprite;
                }

                AssetDatabase.Refresh();
            }

            // ۳) بساز
            progress?.Invoke("Building UI hierarchy...", 0.85f);

            var rootBox = root.absoluteBoundingBox ?? new FigmaRect { width = opt.referenceResolution.x, height = opt.referenceResolution.y };
            var go = BuildNode(root, parent, rootBox, sprites, opt, isRoot: true);

            progress?.Invoke("Done.", 1f);
            return go;
        }

        // ================= انتخاب نودهای تصویری =================

        static void CollectRasterizable(FigmaNode node, Options opt, List<FigmaNode> outList, bool isRoot = false)
        {
            if (node == null) return;
            if (opt.skipHiddenNodes && !node.visible) return;

            if (!isRoot && ShouldRasterize(node, opt))
            {
                if (node.absoluteBoundingBox != null &&
                    node.absoluteBoundingBox.width >= 1f &&
                    node.absoluteBoundingBox.height >= 1f)
                {
                    outList.Add(node);
                }
                return;   // داخلش نرو، کل نود یک تصویر می‌شود
            }

            if (node.children != null)
                foreach (var c in node.children)
                    CollectRasterizable(c, opt, outList);
        }

        static bool ShouldRasterize(FigmaNode n, Options opt)
        {
            if (n.type == "TEXT") return false;

            // نام‌گذاری دستی: هر نودی که نامش به @img ختم شود
            if (!string.IsNullOrEmpty(n.name) && n.name.TrimEnd().EndsWith("@img", StringComparison.OrdinalIgnoreCase))
                return true;

            switch (n.type)
            {
                case "VECTOR":
                case "STAR":
                case "LINE":
                case "REGULAR_POLYGON":
                case "BOOLEAN_OPERATION":
                    return true;

                case "ELLIPSE":
                    return true;   // دایره را با Image ساده نمی‌شود کشید

                case "RECTANGLE":
                    if (n.HasImageFill() || n.HasGradientFill()) return true;
                    if (n.cornerRadius > 0.5f) return true;
                    if (n.rectangleCornerRadii != null && n.rectangleCornerRadii.Any(v => v > 0.5f)) return true;
                    if (n.HasVisibleStroke()) return true;
                    return false;
            }

            if (n.IsContainer())
            {
                // فریم‌های بدون فرزند عملاً یک شکل هستند
                bool leaf = n.children == null || n.children.Count == 0;
                if (leaf && (n.HasImageFill() || n.HasGradientFill() || n.cornerRadius > 0.5f))
                    return true;

                if (opt.rasterizeInstances && (n.type == "INSTANCE" || n.type == "COMPONENT"))
                    return true;
            }

            return false;
        }

        // ================= ذخیره اسپرایت =================

        static Sprite SaveAsSprite(byte[] bytes, FigmaNode node, Options opt)
        {
            string safe = SanitizeFileName($"{node.name}_{node.id}");
            string ext = opt.imageFormat == "jpg" ? "jpg" : opt.imageFormat;
            string path = $"{opt.imagesFolder}/{safe}.{ext}";

            File.WriteAllBytes(path, bytes);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti != null)
            {
                ti.textureType = TextureImporterType.Sprite;
                ti.spriteImportMode = SpriteImportMode.Single;
                ti.alphaIsTransparency = true;
                ti.mipmapEnabled = false;
                ti.wrapMode = TextureWrapMode.Clamp;
                ti.filterMode = FilterMode.Bilinear;
                ti.spritePixelsPerUnit = 100f;
                ti.maxTextureSize = 2048;
                ti.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        static string SanitizeFileName(string s)
        {
            if (string.IsNullOrEmpty(s)) return "node";
            foreach (var c in Path.GetInvalidFileNameChars()) s = s.Replace(c, '_');
            s = s.Replace(':', '_').Replace(' ', '_').Replace('/', '_');
            return s.Length > 64 ? s.Substring(0, 64) : s;
        }

        // ================= ساخت درخت =================

        static GameObject BuildNode(FigmaNode node, Transform parent, FigmaRect parentBox,
                                    Dictionary<string, Sprite> sprites, Options opt, bool isRoot = false)
        {
            if (node == null) return null;
            if (opt.skipHiddenNodes && !node.visible) return null;

            var go = new GameObject(string.IsNullOrEmpty(node.name) ? node.type : node.name,
                                    typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);

            var box = node.absoluteBoundingBox ?? parentBox;

            if (isRoot)
            {
                // ریشه صفحه را کامل روی کانواس بکش
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
            }
            else
            {
                SetRectFromFigma(rt, box, parentBox);
            }

            bool rasterized = !isRoot && sprites.ContainsKey(node.id);

            if (rasterized)
            {
                var img = go.AddComponent<Image>();
                img.sprite = sprites[node.id];
                img.type = Image.Type.Simple;
                img.raycastTarget = false;
                img.color = new Color(1f, 1f, 1f, Mathf.Clamp01(node.opacity));
                MaybeMakeButton(go, node, img, opt);
                return go;   // فرزندان داخل تصویر هستند
            }

            if (node.type == "TEXT")
            {
                BuildText(go, node, opt);
                return go;
            }

            // پس‌زمینه‌ی رنگی ساده
            var fill = node.FirstVisibleFill();
            if (fill != null && fill.type == "SOLID" && fill.color != null)
            {
                var img = go.AddComponent<Image>();
                img.color = fill.color.ToUnity(fill.opacity * node.opacity);
                img.raycastTarget = false;
                MaybeMakeButton(go, node, img, opt);
            }
            else if (Mathf.Abs(node.opacity - 1f) > 0.001f)
            {
                var cg = go.AddComponent<CanvasGroup>();
                cg.alpha = Mathf.Clamp01(node.opacity);
            }

            if (opt.addRectMask && node.clipsContent)
                go.AddComponent<RectMask2D>();

            if (node.children != null)
            {
                foreach (var child in node.children)
                    BuildNode(child, rt, box, sprites, opt);
            }

            return go;
        }

        /// <summary>
        /// فیگما مبدأ را بالا-چپ می‌گیرد و Y به سمت پایین زیاد می‌شود.
        /// یونیتی پایین-چپ. با لنگر و pivot بالا-چپ، تبدیل ساده می‌شود.
        /// </summary>
        static void SetRectFromFigma(RectTransform rt, FigmaRect box, FigmaRect parentBox)
        {
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.sizeDelta = new Vector2(box.width, box.height);
            rt.anchoredPosition = new Vector2(box.x - parentBox.x, -(box.y - parentBox.y));
            rt.localScale = Vector3.one;
        }

        static void MaybeMakeButton(GameObject go, FigmaNode node, Image img, Options opt)
        {
            if (!opt.createButtons || img == null) return;
            string n = node.name ?? "";
            if (n.IndexOf("btn", StringComparison.OrdinalIgnoreCase) < 0 &&
                n.IndexOf("button", StringComparison.OrdinalIgnoreCase) < 0) return;

            img.raycastTarget = true;
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
        }

        // ================= متن =================

        static Type _rtlType;
        static bool _rtlChecked;

        /// <summary>اگر پکیج RTLTMPro نصب باشد از آن استفاده می‌کنیم، وگرنه TextMeshProUGUI ساده.</summary>
        static Type RtlType
        {
            get
            {
                if (_rtlChecked) return _rtlType;
                _rtlChecked = true;
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        var t = asm.GetType("RTLTMPro.RTLTextMeshPro");
                        if (t != null) { _rtlType = t; break; }
                    }
                    catch { /* ignore */ }
                }
                return _rtlType;
            }
        }

        static void BuildText(GameObject go, FigmaNode node, Options opt)
        {
            bool rtl = ContainsRtl(node.characters);
            Type compType = (rtl && RtlType != null) ? RtlType : typeof(TextMeshProUGUI);

            var tmp = go.AddComponent(compType) as TMP_Text;
            if (tmp == null)
            {
                tmp = go.AddComponent<TextMeshProUGUI>();
            }

            var st = node.style ?? new FigmaTypeStyle();

            tmp.text = node.characters ?? "";
            tmp.fontSize = Mathf.Max(1f, st.fontSize);
            tmp.enableAutoSizing = false;
            tmp.raycastTarget = false;
            tmp.overflowMode = TextOverflowModes.Overflow;
            EnableWordWrap(tmp);   // نام این پراپرتی بین نسخه‌های TMP فرق دارد

            // رنگ از fills نود متن می‌آید
            var fill = node.FirstVisibleFill();
            tmp.color = (fill?.color != null)
                ? fill.color.ToUnity(fill.opacity * node.opacity)
                : Color.white;

            tmp.alignment = MapAlignment(st.textAlignHorizontal, st.textAlignVertical, rtl);

            if (st.fontSize > 0.1f)
            {
                if (Mathf.Abs(st.letterSpacing) > 0.01f)
                    tmp.characterSpacing = st.letterSpacing / st.fontSize * 100f;

                if (st.lineHeightPx > 0.1f)
                    tmp.lineSpacing = (st.lineHeightPx / st.fontSize - 1.2f) * 100f;
            }

            // اولویت: override دستی ← بعد تطبیق نام fontFamily فیگما
            var font = rtl ? opt.persianFontOverride : opt.latinFontOverride;
            if (font == null) font = FindFont(st.fontFamily, opt);
            if (font != null) tmp.font = font;

            // RTLTMPro: فارسی‌سازی اعداد و حروف
            if (rtl && RtlType != null && compType == RtlType)
            {
                TrySetBool(tmp, "Farsi", true);
                TrySetBool(tmp, "PreserveNumbers", false);
            }
            else if (rtl)
            {
                tmp.isRightToLeftText = true;
            }
        }

        /// <summary>
        /// TMP قدیمی: enableWordWrapping — TMP جدید (Unity 6): textWrappingMode.
        /// با ریفلکشن هر کدام که موجود بود ست می‌شود تا کد روی هر نسخه‌ای کامپایل شود.
        /// </summary>
        static void EnableWordWrap(TMP_Text tmp)
        {
            var t = tmp.GetType();

            var wrapMode = t.GetProperty("textWrappingMode");
            if (wrapMode != null && wrapMode.CanWrite)
            {
                try
                {
                    var val = Enum.Parse(wrapMode.PropertyType, "Normal");
                    wrapMode.SetValue(tmp, val);
                    return;
                }
                catch { /* ادامه بده */ }
            }

            TrySetBool(tmp, "enableWordWrapping", true);
        }

        static void TrySetBool(object target, string propName, bool value)
        {
            try
            {
                var p = target.GetType().GetProperty(propName);
                if (p != null && p.CanWrite && p.PropertyType == typeof(bool))
                    p.SetValue(target, value);
            }
            catch { /* ignore */ }
        }

        static bool ContainsRtl(string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            foreach (var c in s)
            {
                // بازه عربی/فارسی و فرم‌های نمایشی
                if ((c >= 0x0600 && c <= 0x06FF) ||
                    (c >= 0x0750 && c <= 0x077F) ||
                    (c >= 0xFB50 && c <= 0xFDFF) ||
                    (c >= 0xFE70 && c <= 0xFEFF))
                    return true;
            }
            return false;
        }

        static TextAlignmentOptions MapAlignment(string h, string v, bool rtl)
        {
            bool top = v == "TOP" || string.IsNullOrEmpty(v);
            bool bottom = v == "BOTTOM";

            string hh = h;
            if (string.IsNullOrEmpty(hh)) hh = rtl ? "RIGHT" : "LEFT";

            switch (hh)
            {
                case "CENTER":
                    return top ? TextAlignmentOptions.Top
                         : bottom ? TextAlignmentOptions.Bottom
                         : TextAlignmentOptions.Center;
                case "RIGHT":
                    return top ? TextAlignmentOptions.TopRight
                         : bottom ? TextAlignmentOptions.BottomRight
                         : TextAlignmentOptions.Right;
                case "JUSTIFIED":
                    return top ? TextAlignmentOptions.TopJustified
                         : bottom ? TextAlignmentOptions.BottomJustified
                         : TextAlignmentOptions.Justified;
                default:
                    return top ? TextAlignmentOptions.TopLeft
                         : bottom ? TextAlignmentOptions.BottomLeft
                         : TextAlignmentOptions.Left;
            }
        }

        static Dictionary<string, TMP_FontAsset> _fontCache;

        static TMP_FontAsset FindFont(string family, Options opt)
        {
            if (string.IsNullOrEmpty(family)) return null;

            if (_fontCache == null)
            {
                _fontCache = new Dictionary<string, TMP_FontAsset>(StringComparer.OrdinalIgnoreCase);

                // کل پروژه را می‌گردیم (نه فقط Resources) — فونت‌ها هرجا باشند پیدا می‌شوند
                string[] folders = string.IsNullOrWhiteSpace(opt.fontsSearchFolder)
                    ? null
                    : new[] { opt.fontsSearchFolder };

                var guids = folders == null
                    ? AssetDatabase.FindAssets("t:TMP_FontAsset")
                    : AssetDatabase.FindAssets("t:TMP_FontAsset", folders);

                foreach (var guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);

                    // نمونه‌های داخل پکیج‌ها و دموها را رد کن
                    if (path.Contains("/Examples") || path.Contains("Examples & Extras")) continue;

                    var f = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
                    if (f != null && !_fontCache.ContainsKey(f.name))
                        _fontCache[f.name] = f;
                }
            }

            if (_fontCache.TryGetValue(family, out var exact)) return exact;

            string key = family.Replace(" ", "");
            foreach (var kv in _fontCache)
            {
                var candidate = kv.Key.Replace(" ", "");
                if (candidate.IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    key.IndexOf(candidate, StringComparison.OrdinalIgnoreCase) >= 0)
                    return kv.Value;
            }

            // اگر فقط یک فونت داریم، همان را بده
            return _fontCache.Count == 1 ? _fontCache.Values.First() : null;
        }

        public static void ClearFontCache() { _fontCache = null; }

        // ================= کانواس و پریفب =================

        /// <summary>سازگار با نسخه‌های قدیمی و جدید یونیتی</summary>
        static T FindInScene<T>() where T : UnityEngine.Object
        {
#if UNITY_2022_2_OR_NEWER
            return UnityEngine.Object.FindFirstObjectByType<T>();
#else
            return UnityEngine.Object.FindObjectOfType<T>();
#endif
        }

        public static Canvas EnsureCanvas(Options opt)
        {
            var canvas = FindInScene<Canvas>();
            if (canvas == null)
            {
                var go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = go.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                Undo.RegisterCreatedObjectUndo(go, "Create Canvas");
            }

            var scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = canvas.gameObject.AddComponent<CanvasScaler>();
                ApplyScaler(scaler, opt);
            }
            else if (opt.touchCanvasScaler)
            {
                ApplyScaler(scaler, opt);
            }
            else if (scaler.referenceResolution != opt.referenceResolution)
            {
                Debug.LogWarning(
                    $"[Figma] Canvas Scaler reference resolution is {scaler.referenceResolution} but the design is " +
                    $"{opt.referenceResolution}. Either align the reference resolution or enable " +
                    "'Overwrite Canvas Scaler' in the importer window.");
            }

            if (FindInScene<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var es = new GameObject("EventSystem",
                    typeof(UnityEngine.EventSystems.EventSystem),
                    typeof(UnityEngine.EventSystems.StandaloneInputModule));
                Undo.RegisterCreatedObjectUndo(es, "Create EventSystem");
            }

            return canvas;
        }

        static void ApplyScaler(CanvasScaler scaler, Options opt)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = opt.referenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = opt.matchWidthOrHeight;
            EditorUtility.SetDirty(scaler);
        }

        public static void SavePrefab(GameObject go, Options opt)
        {
            if (go == null) return;
            Directory.CreateDirectory(opt.prefabsFolder);
            string path = AssetDatabase.GenerateUniqueAssetPath($"{opt.prefabsFolder}/{SanitizeFileName(go.name)}.prefab");
            PrefabUtility.SaveAsPrefabAssetAndConnect(go, path, InteractionMode.AutomatedAction);
            Debug.Log($"[Figma] Prefab saved: {path}");
        }
    }
}
