// FigmaImporterWindow.cs
// پنجره Editor برای وارد کردن صفحات فیگما به یونیتی
// منو:  Tools ▸ Figma Importer
// محل قرارگیری: Assets/Editor/Figma/
//
// توجه: تمام متن‌های رابط کاربری انگلیسی هستند چون IMGUI یونیتی
// متن راست‌به‌چپ را برعکس رندر می‌کند.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace FigmaImport
{
    public class FigmaImporterWindow : EditorWindow
    {
        const string PrefToken = "FigmaImporter.Token";
        const string PrefFileKey = "FigmaImporter.FileKey";

        [Serializable]
        public class ScreenEntry
        {
            public bool selected;
            public string nodeId;
            public string label;

            public ScreenEntry(string id, string name, bool sel = true)
            {
                nodeId = id; label = name; selected = sel;
            }
        }

        string _token = "";
        string _fileKey = "ULLcTai3SWAQimBRDDnsKf";
        string _status = "Ready.";
        bool _busy;
        Vector2 _scroll;

        FigmaUIBuilder.Options _opt = new FigmaUIBuilder.Options();

        readonly List<ScreenEntry> _screens = new List<ScreenEntry>
        {
            new ScreenEntry("2005:802",  "HOME"),
            new ScreenEntry("2005:1441", "Shop",        false),
            new ScreenEntry("2005:1319", "Leaderboard", false),
            new ScreenEntry("2005:1110", "Profile",     false),
            new ScreenEntry("2005:5",    "Game_3x3",    false),
            new ScreenEntry("2005:261",  "Win",         false),
            new ScreenEntry("2005:441",  "Lose",        false),
            new ScreenEntry("2005:363",  "Tutorial",    false),
            new ScreenEntry("2005:2571", "Missions",    false),
            new ScreenEntry("2005:1011", "Awards",      false),
        };

        [MenuItem("Tools/Figma Importer")]
        public static void Open()
        {
            var w = GetWindow<FigmaImporterWindow>("Figma Importer");
            w.minSize = new Vector2(430, 560);
            w.Show();
        }

        void OnEnable()
        {
            _token = EditorPrefs.GetString(PrefToken, "");
            _fileKey = EditorPrefs.GetString(PrefFileKey, _fileKey);
        }

        void OnGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            EditorGUILayout.LabelField("Connection", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            _token = EditorGUILayout.PasswordField(
                new GUIContent("Personal Token",
                    "figma.com > Settings > Security > Personal access tokens. Scope needed: file_content:read"),
                _token);
            _fileKey = EditorGUILayout.TextField(
                new GUIContent("File Key / URL", "You can paste the full Figma file URL here."),
                _fileKey);
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetString(PrefToken, _token);
                EditorPrefs.SetString(PrefFileKey, _fileKey);
            }

            if (GUILayout.Button("Test Connection", GUILayout.Height(22)))
                _ = TestConnection();

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Canvas", EditorStyles.boldLabel);

            _opt.referenceResolution = EditorGUILayout.Vector2Field("Reference Resolution", _opt.referenceResolution);
            _opt.touchCanvasScaler = EditorGUILayout.Toggle(
                new GUIContent("Overwrite Canvas Scaler",
                    "OFF = leave the existing Canvas settings untouched. Recommended for projects already in progress."),
                _opt.touchCanvasScaler);
            using (new EditorGUI.DisabledScope(!_opt.touchCanvasScaler))
                _opt.matchWidthOrHeight = EditorGUILayout.Slider("Match Width <-> Height", _opt.matchWidthOrHeight, 0f, 1f);

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Fonts", EditorStyles.boldLabel);

            _opt.persianFontOverride = (TMP_FontAsset)EditorGUILayout.ObjectField(
                new GUIContent("Persian / Arabic Font",
                    "Used for any text containing Arabic-script characters. Leave empty to auto-match by the Figma font name."),
                _opt.persianFontOverride, typeof(TMP_FontAsset), false);
            _opt.latinFontOverride = (TMP_FontAsset)EditorGUILayout.ObjectField(
                new GUIContent("Latin Font", "Used for English text and numbers."),
                _opt.latinFontOverride, typeof(TMP_FontAsset), false);

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Import Options", EditorStyles.boldLabel);

            _opt.imageScale = EditorGUILayout.IntPopup("Image Scale", _opt.imageScale,
                new[] { "1x", "2x", "3x", "4x" }, new[] { 1, 2, 3, 4 });
            _opt.rasterizeInstances = EditorGUILayout.Toggle(
                new GUIContent("Rasterize Components",
                    "Flatten each Component/Instance into a single PNG instead of rebuilding it layer by layer."),
                _opt.rasterizeInstances);
            _opt.createButtons = EditorGUILayout.Toggle(
                new GUIContent("Auto-create Buttons", "Nodes whose name contains 'btn' or 'button'."),
                _opt.createButtons);
            _opt.savePrefab = EditorGUILayout.Toggle(
                new GUIContent("Save as Prefab", "Writes each imported screen to Assets/Figma/Prefabs."),
                _opt.savePrefab);

            EditorGUILayout.Space(8);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Screens", EditorStyles.boldLabel);
                if (GUILayout.Button("All", GUILayout.Width(46)))
                    _screens.ForEach(s => s.selected = true);
                if (GUILayout.Button("None", GUILayout.Width(46)))
                    _screens.ForEach(s => s.selected = false);
            }

            int removeIndex = -1;
            for (int i = 0; i < _screens.Count; i++)
            {
                var s = _screens[i];
                using (new EditorGUILayout.HorizontalScope())
                {
                    s.selected = EditorGUILayout.Toggle(s.selected, GUILayout.Width(18));
                    s.label = EditorGUILayout.TextField(s.label, GUILayout.Width(120));
                    s.nodeId = EditorGUILayout.TextField(s.nodeId);
                    if (GUILayout.Button("-", GUILayout.Width(22)))
                        removeIndex = i;
                }
            }
            if (removeIndex >= 0) _screens.RemoveAt(removeIndex);

            if (GUILayout.Button("+ Add Screen"))
                _screens.Add(new ScreenEntry("", "NewScreen", false));

            EditorGUILayout.Space(10);

            using (new EditorGUI.DisabledScope(_busy))
            {
                GUI.backgroundColor = new Color(0.6f, 0.85f, 1f);
                if (GUILayout.Button("Import Selected Screens", GUILayout.Height(34)))
                    _ = ImportSelected();
                GUI.backgroundColor = Color.white;

                if (GUILayout.Button("Dump File Structure to Console", GUILayout.Height(22)))
                    _ = DumpStructure();
            }

            EditorGUILayout.Space(6);
            EditorGUILayout.HelpBox(_status, _busy ? MessageType.Info : MessageType.None);

            EditorGUILayout.Space(4);
            EditorGUILayout.HelpBox(
                "Naming conventions in Figma:\n" +
                "  layer name ending in @img  ->  downloaded as a single PNG (backgrounds, avatars, composite icons)\n" +
                "  layer name containing btn / button  ->  gets a Button component\n\n" +
                "Vectors, ellipses, gradients and rounded rectangles are rasterized automatically.",
                MessageType.Info);

            EditorGUILayout.EndScrollView();
        }

        // ---------------- عملیات ----------------

        void Report(string msg, float p)
        {
            _status = msg;
            EditorUtility.DisplayProgressBar("Figma Importer", msg, Mathf.Clamp01(p));
            Repaint();
        }

        async Task TestConnection()
        {
            _busy = true;
            try
            {
                using (var api = new FigmaApi(_token, FigmaApi.ExtractFileKey(_fileKey)))
                {
                    Report("Connecting...", 0.3f);
                    var doc = await api.GetFileAsync(1);
                    _status = $"Connected. File: {doc.name ?? "(unnamed)"}";
                    Debug.Log($"[Figma] Connected. Root: {doc.name} - {doc.children?.Count ?? 0} page(s)");
                }
            }
            catch (Exception e)
            {
                _status = "FAILED: " + e.Message;
                Debug.LogException(e);
            }
            finally
            {
                _busy = false;
                EditorUtility.ClearProgressBar();
                Repaint();
            }
        }

        async Task DumpStructure()
        {
            _busy = true;
            try
            {
                using (var api = new FigmaApi(_token, FigmaApi.ExtractFileKey(_fileKey)))
                {
                    Report("Fetching file structure...", 0.3f);
                    var doc = await api.GetFileAsync(3);

                    var sb = new System.Text.StringBuilder();
                    Dump(doc, sb, 0);
                    Debug.Log("[Figma] File structure:\n" + sb);
                    _status = "Structure printed to Console.";
                }
            }
            catch (Exception e)
            {
                _status = "FAILED: " + e.Message;
                Debug.LogException(e);
            }
            finally
            {
                _busy = false;
                EditorUtility.ClearProgressBar();
                Repaint();
            }
        }

        static void Dump(FigmaNode n, System.Text.StringBuilder sb, int depth)
        {
            if (n == null || depth > 3) return;
            sb.AppendLine($"{new string(' ', depth * 2)}[{n.type}] {n.name}   ->  {n.id}");
            if (n.children != null)
                foreach (var c in n.children) Dump(c, sb, depth + 1);
        }

        async Task ImportSelected()
        {
            var picked = _screens.FindAll(s => s.selected && !string.IsNullOrWhiteSpace(s.nodeId));
            if (picked.Count == 0)
            {
                _status = "No screen selected.";
                return;
            }

            _busy = true;
            FigmaUIBuilder.ClearFontCache();

            try
            {
                using (var api = new FigmaApi(_token, FigmaApi.ExtractFileKey(_fileKey)))
                {
                    var canvas = FigmaUIBuilder.EnsureCanvas(_opt);

                    for (int i = 0; i < picked.Count; i++)
                    {
                        var s = picked[i];
                        float baseP = (float)i / picked.Count;

                        Report($"[{i + 1}/{picked.Count}] Fetching {s.label}...", baseP);

                        var node = await api.GetNodeAsync(s.nodeId);

                        var go = await FigmaUIBuilder.BuildScreenAsync(
                            node, canvas.transform, api, _opt,
                            (m, p) => Report($"[{i + 1}/{picked.Count}] {m}", baseP + p / picked.Count));

                        if (go != null)
                        {
                            go.name = string.IsNullOrWhiteSpace(s.label) ? node.name : s.label;
                            Undo.RegisterCreatedObjectUndo(go, "Import Figma Screen");
                            if (_opt.savePrefab) FigmaUIBuilder.SavePrefab(go, _opt);
                            go.SetActive(i == 0);   // فقط اولی روشن بماند
                        }

                        if (i < picked.Count - 1)
                            await Task.Delay(2000);   // احترام به Rate Limit
                    }

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    _status = $"Done. {picked.Count} screen(s) imported.";
                }
            }
            catch (Exception e)
            {
                _status = "FAILED: " + e.Message;
                Debug.LogException(e);
            }
            finally
            {
                _busy = false;
                EditorUtility.ClearProgressBar();
                Repaint();
            }
        }
    }
}
