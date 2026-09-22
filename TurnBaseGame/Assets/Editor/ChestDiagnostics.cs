using System.Collections.Generic;
using JuiceUp;
using Nakama.Helpers;
using UnityEditor;
using UnityEngine;

namespace NinjaBattle.Editor
{
    /// <summary>
    /// Bisects the StackOverflowException thrown when the chest panel opens.
    /// Run both menu items in PLAY mode on the Home scene and compare:
    ///   1. "Open Chest Panel (juicers disabled)" — opens the panel with every
    ///      UiJuiceAnimator in its subtree turned off.
    ///   2. "Open Chest Panel (raw)" — opens it untouched.
    /// Whichever variant stops the StackOverflow identifies the culprit.
    /// </summary>
    public static class ChestDiagnostics
    {
        [MenuItem("Tools/NinjaBattle/Validation/Open Chest Panel (juicers disabled)")]
        public static void OpenWithoutJuicers() => Open(disableJuicers: true);

        [MenuItem("Tools/NinjaBattle/Validation/Open Chest Panel (raw)")]
        public static void OpenRaw() => Open(disableJuicers: false);

        private static void Open(bool disableJuicers)
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[ChestDiag] Enter Play mode on the Home scene first.");
                return;
            }

            ChestManager manager = null;
            foreach (ChestManager m in Resources.FindObjectsOfTypeAll<ChestManager>())
            {
                if (m.gameObject.scene.IsValid()) { manager = m; break; }
            }

            if (manager == null)
            {
                Debug.LogError("[ChestDiag] No ChestManager found in the loaded scene.");
                return;
            }

            GameObject panel = manager.gameObject;
            var disabled = new List<Behaviour>();

            if (disableJuicers)
            {
                foreach (var juicer in panel.GetComponentsInChildren<UiJuiceAnimator>(true))
                {
                    if (juicer.enabled)
                    {
                        juicer.enabled = false;
                        disabled.Add(juicer);
                    }
                }

                Debug.Log($"[ChestDiag] step1: disabled {disabled.Count} UiJuiceAnimator(s).");
            }

            Debug.Log("[ChestDiag] step2: activating chest panel...");
            panel.SetActive(true);
            Debug.Log("[ChestDiag] step3: panel activated WITHOUT StackOverflow " +
                      (disableJuicers ? "(juicers off → if the raw variant overflows, the juicer is the culprit)"
                                      : "(raw)"));
        }
    }
}
