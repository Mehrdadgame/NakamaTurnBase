using System.Text;
using Coffee.UIExtensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace NinjaBattle.Editor
{
    /// <summary>
    /// One-click diagnosis for the board cell particles (UIParticle based).
    /// Use "Diagnose Cell Particles" to print a full report of everything that
    /// could prevent rendering, and "Spawn Test Cell Particle" for a visual
    /// test that does not depend on the multiplayer flow (works in Play mode).
    /// </summary>
    public static class CellParticleDiagnostics
    {
        private const string PrefabPath = "Assets/VFX/cellParticle.prefab";

        [MenuItem("Tools/NinjaBattle/Validation/Diagnose Cell Particles")]
        public static void Diagnose()
        {
            var report = new StringBuilder();
            report.AppendLine("===== Cell Particle Diagnosis =====");

            UIParticle[] uiParticles = Object.FindObjectsByType<UIParticle>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            report.AppendLine($"UIParticle components in scene: {uiParticles.Length}");

            UIParticle target = null;
            foreach (UIParticle up in uiParticles)
            {
                GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(up.gameObject);
                if (source != null && AssetDatabase.GetAssetPath(source) == PrefabPath)
                {
                    target = up;
                    break;
                }
            }

            if (target == null)
            {
                report.AppendLine("No cellParticle instance found in the open scene. Open a battle scene first.");
                Debug.LogWarning(report.ToString());
                return;
            }

            GameObject go = target.gameObject;
            report.AppendLine($"Instance: {GetPath(go.transform)}");
            report.AppendLine($"  activeInHierarchy={go.activeInHierarchy}, layer={LayerMask.LayerToName(go.layer)}");
            report.AppendLine($"  UIParticle.enabled={target.enabled}, color={target.color}, maskable={target.maskable}");
            report.AppendLine($"  autoScalingMode={target.autoScalingMode}, scale3D={target.scale3D}");
            report.AppendLine($"  localScale={go.transform.localScale}, lossyScale={go.transform.lossyScale}");

            var ps = go.GetComponent<ParticleSystem>();
            if (ps == null)
            {
                report.AppendLine("  !! No ParticleSystem on the same GameObject.");
            }
            else
            {
                var main = ps.main;
                report.AppendLine($"  ParticleSystem: playOnAwake={main.playOnAwake}, looping={main.loop}, " +
                                  $"startSize={main.startSize.constant}, startColorAlpha={main.startColor.color.a}, " +
                                  $"isPlaying={ps.isPlaying}, particleCount={ps.particleCount}");
                var psr = ps.GetComponent<ParticleSystemRenderer>();
                if (psr != null)
                {
                    report.AppendLine($"  Renderer: renderMode={psr.renderMode}, " +
                                      $"material={(psr.sharedMaterial ? psr.sharedMaterial.name : "NULL")}, " +
                                      $"shader={(psr.sharedMaterial && psr.sharedMaterial.shader ? psr.sharedMaterial.shader.name : "NULL")}, " +
                                      $"texture={(psr.sharedMaterial && psr.sharedMaterial.mainTexture ? psr.sharedMaterial.mainTexture.name : "NULL")}");
                }
            }

            Canvas canvas = go.GetComponentInParent<Canvas>(true);
            if (canvas == null)
            {
                report.AppendLine("  !! No parent Canvas — UIParticle can not render without one.");
            }
            else
            {
                Canvas root = canvas.rootCanvas;
                report.AppendLine($"  Canvas: '{root.name}', renderMode={root.renderMode}, " +
                                  $"worldCamera={(root.worldCamera ? root.worldCamera.name : "NULL")}, " +
                                  $"enabled={root.enabled}, sortingOrder={root.sortingOrder}");
                if (root.renderMode == RenderMode.ScreenSpaceCamera && root.worldCamera == null)
                    report.AppendLine("  !! ScreenSpaceCamera canvas WITHOUT a camera — assign the UI camera.");
            }

            // Anything up the chain that can hide it
            foreach (CanvasGroup cg in go.GetComponentsInParent<CanvasGroup>(true))
                if (cg.alpha < 0.99f)
                    report.AppendLine($"  !! CanvasGroup '{GetPath(cg.transform)}' alpha={cg.alpha}");
            foreach (Mask m in go.GetComponentsInParent<Mask>(true))
                report.AppendLine($"  ?? Mask in parents: {GetPath(m.transform)}");
            foreach (RectMask2D m in go.GetComponentsInParent<RectMask2D>(true))
                report.AppendLine($"  ?? RectMask2D in parents: {GetPath(m.transform)}");

            var canvasRenderer = go.GetComponent<CanvasRenderer>();
            if (canvasRenderer != null)
                report.AppendLine($"  CanvasRenderer: cull={canvasRenderer.cull}, alpha={canvasRenderer.GetAlpha()}");

            report.AppendLine("Note: playOnAwake is OFF by design — these particles only appear after " +
                              "UiManager calls Play() during a real match (or use Spawn Test Cell Particle).");
            Debug.Log(report.ToString());
        }

        [MenuItem("Tools/NinjaBattle/Validation/Spawn Test Cell Particle")]
        public static void SpawnTest()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null)
            {
                Debug.LogError("cellParticle prefab not found at " + PrefabPath);
                return;
            }

            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("No Canvas in the open scene.");
                return;
            }

            Transform parent = canvas.rootCanvas.transform;

            // 1. Reference UI Image — proves the spawn location is on screen.
            var imageGo = new GameObject("TEST_ReferenceImage", typeof(RectTransform), typeof(Image));
            imageGo.transform.SetParent(parent, false);
            var imageRect = imageGo.GetComponent<RectTransform>();
            imageRect.anchorMin = imageRect.anchorMax = new Vector2(0.5f, 0.5f);
            imageRect.anchoredPosition = new Vector2(0f, -200f);
            imageRect.sizeDelta = new Vector2(60f, 60f);
            imageGo.GetComponent<Image>().color = new Color(1f, 0f, 0.85f, 1f);

            // 2. Original cellParticle.
            GameObject original = SpawnParticle(prefab, parent, new Vector2(0f, 100f), "TEST_cellParticle_original");

            // 3. cellParticle with a guaranteed-visible opaque UI material.
            GameObject fallback = SpawnParticle(prefab, parent, new Vector2(0f, -50f), "TEST_cellParticle_uiDefault");
            var psr = fallback.GetComponent<ParticleSystemRenderer>();
            var mat = new Material(Shader.Find("UI/Default")) { name = "TEST_UIDefault" };
            psr.sharedMaterial = mat;
            var main = fallback.GetComponent<ParticleSystem>().main;
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.1f, 0.1f, 1f));
            main.startSize = 2f; // ×190 (UIParticle scale3D) ≈ 380 px on screen

            if (Application.isPlaying)
            {
                var runner = new GameObject("TEST_Reporter").AddComponent<TestReporter>();
                runner.targets = new[] { original, fallback };
            }
            else
            {
                Selection.activeGameObject = original;
                foreach (var go in new[] { original, fallback })
                    go.GetComponent<ParticleSystem>().Simulate(1f, true, false, false);
                Debug.Log("Edit mode: keep a TEST object selected to preview. For a conclusive check run this in PLAY mode.");
            }

            Debug.Log("Spawned: magenta square (position check), original particle (above center), " +
                      "red opaque particle (below center). Check the GAME view. Delete TEST_* objects when done.");
        }

        private static GameObject SpawnParticle(GameObject prefab, Transform parent, Vector2 anchoredPos, string name)
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.name = name;
            var rect = instance.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPos;
            rect.localScale = Vector3.one;
            instance.GetComponent<ParticleSystem>().Play(true);
            return instance;
        }

        /// <summary>Logs simulation + bake state 1.5 s after the test spawn (play mode only).</summary>
        private class TestReporter : MonoBehaviour
        {
            public GameObject[] targets;
            private float _t;

            private void Update()
            {
                _t += Time.unscaledDeltaTime;
                if (_t < 1.5f) return;

                var sb = new StringBuilder("===== Cell Particle Runtime Test =====\n");
                foreach (GameObject go in targets)
                {
                    if (go == null) continue;
                    var ps = go.GetComponent<ParticleSystem>();
                    sb.AppendLine($"{go.name}: isPlaying={ps.isPlaying}, particleCount={ps.particleCount}, " +
                                  $"lossyScale={go.transform.lossyScale.x:F6}");
                    foreach (var cr in go.GetComponentsInChildren<CanvasRenderer>(true))
                        sb.AppendLine($"   child '{cr.gameObject.name}': cull={cr.cull}, " +
                                      $"materialCount={cr.materialCount}, hasMoved={cr.transform.hasChanged}");
                }

                sb.AppendLine("particleCount>0 but nothing on screen → bake/render issue. " +
                              "particleCount=0 → simulation issue.");
                Debug.Log(sb.ToString());
                Destroy(gameObject);
            }
        }

        private static string GetPath(Transform t)
        {
            string path = t.name;
            while (t.parent != null)
            {
                t = t.parent;
                path = t.name + "/" + path;
            }

            return path;
        }
    }
}
