using UnityEditor;
using UnityEngine;

namespace JuiceUp.Editor
{
    [CustomEditor(typeof(UiJuiceAnimator)), CanEditMultipleObjects]
    public class UiJuiceAnimatorEditor : UnityEditor.Editor
    {
        private SerializedProperty autoPlayProp;
        private SerializedProperty staggerProp;
        private SerializedProperty includeInactiveProp;
        private SerializedProperty presetProp;
        private SerializedProperty autoRandomizeProp;
        private SerializedProperty strengthProp;
        private SerializedProperty durationMinProp;
        private SerializedProperty durationMaxProp;
        private SerializedProperty elementsProp;
        private SerializedProperty allowPreviewInEditModeProp;
        private SerializedProperty allowAutoPlayInEditModeProp;
        private SerializedProperty autoApplyPresetInEditModeProp;
        private SerializedProperty verboseLogsProp;
        private SerializedProperty deactivateAfterPlayOutProp;
        private SerializedProperty destroyAfterPlayOutProp;

        private void OnEnable()
        {
            autoPlayProp = serializedObject.FindProperty("autoPlayOnEnable");
            staggerProp = serializedObject.FindProperty("stagger");
            includeInactiveProp = serializedObject.FindProperty("includeInactiveChildren");
            presetProp = serializedObject.FindProperty("preset");
            autoRandomizeProp = serializedObject.FindProperty("autoRandomizeOnPlay");
            strengthProp = serializedObject.FindProperty("strength");
            durationMinProp = serializedObject.FindProperty("durationMin");
            durationMaxProp = serializedObject.FindProperty("durationMax");
            elementsProp = serializedObject.FindProperty("elements");
            allowPreviewInEditModeProp = serializedObject.FindProperty("allowPreviewInEditMode");
            allowAutoPlayInEditModeProp = serializedObject.FindProperty("allowAutoPlayInEditMode");
            autoApplyPresetInEditModeProp = serializedObject.FindProperty("autoApplyPresetInEditMode");
            verboseLogsProp = serializedObject.FindProperty("verboseLogs");
            deactivateAfterPlayOutProp = serializedObject.FindProperty("deactivateGameObjectAfterPlayOut");
            destroyAfterPlayOutProp = serializedObject.FindProperty("destroyGameObjectAfterPlayOut");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawPlaybackSection();
            EditorGUILayout.Space();
            DrawRandomizationSection();
            EditorGUILayout.Space();
            DrawHierarchySection();
            EditorGUILayout.Space();
            DrawElementsSection();

            serializedObject.ApplyModifiedProperties();
        }

        private void CommitInspectorChanges()
        {
            // Important: Buttons (Play/Randomize/etc.) should operate on the latest inspector values.
            // Without this, changing a value (e.g., Feeling Preset) and immediately clicking a button
            // can run using the old serialized value.
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }

        private void DrawPlaybackSection()
        {
            DrawSectionHeader("Playback");
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(autoPlayProp);
                EditorGUILayout.PropertyField(staggerProp);
                EditorGUILayout.PropertyField(includeInactiveProp, new GUIContent("Include Inactive On Scan"));

                EditorGUILayout.Space(6);
                EditorGUILayout.LabelField("Runtime Completion (Play Mode Only)", EditorStyles.miniBoldLabel);
                EditorGUILayout.PropertyField(deactivateAfterPlayOutProp);
                EditorGUILayout.PropertyField(destroyAfterPlayOutProp);

                EditorGUILayout.Space(4);
                EditorGUILayout.PropertyField(allowPreviewInEditModeProp);
                
                if (allowPreviewInEditModeProp != null && allowPreviewInEditModeProp.boolValue)
                {
                    EditorGUILayout.HelpBox("Warning: Modifying UI in edit mode can break your UI layout. Make sure to have backups and test in runtime as much as possible.", MessageType.Warning);
                }
                
                EditorGUILayout.PropertyField(allowAutoPlayInEditModeProp);
                EditorGUILayout.PropertyField(autoApplyPresetInEditModeProp);
                EditorGUILayout.PropertyField(verboseLogsProp);
            }
        }

        private void DrawRandomizationSection()
        {
            DrawSectionHeader("Randomization & Feel");
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(presetProp, new GUIContent("Feeling Preset"));
                EditorGUILayout.PropertyField(strengthProp, new GUIContent("Strength"));
                EditorGUILayout.PropertyField(autoRandomizeProp, new GUIContent("Auto Randomize On Play"));

                DrawDurationRange();

                if (!EditorApplication.isPlaying && allowPreviewInEditModeProp != null && !allowPreviewInEditModeProp.boolValue)
                {
                    EditorGUILayout.HelpBox("Edit-mode preview is disabled. Enable 'Allow Preview In Edit Mode' to animate without entering Play Mode.", MessageType.Info);
                }

                EditorGUILayout.Space(4);
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Play In", EditorStyles.miniButton))
                    {
                        CommitInspectorChanges();
                        foreach (var obj in targets)
                        {
                            if (obj is UiJuiceAnimator animator)
                            {
                                Undo.RegisterFullObjectHierarchyUndo(animator.gameObject, "Play In UI Juice");
                                animator.PlayIn();
                                EditorUtility.SetDirty(animator);
                            }
                        }
                    }

                    if (GUILayout.Button("Play Out", EditorStyles.miniButton))
                    {
                        CommitInspectorChanges();
                        foreach (var obj in targets)
                        {
                            if (obj is UiJuiceAnimator animator)
                            {
                                Undo.RegisterFullObjectHierarchyUndo(animator.gameObject, "Play Out UI Juice");
                                animator.PlayOut();
                                EditorUtility.SetDirty(animator);
                            }
                        }
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Stop", EditorStyles.miniButtonLeft))
                    {
                        CommitInspectorChanges();
                        foreach (var obj in targets)
                        {
                            if (obj is UiJuiceAnimator animator)
                            {
                                Undo.RegisterFullObjectHierarchyUndo(animator.gameObject, "Stop UI Juice");
                                animator.Stop();
                                EditorUtility.SetDirty(animator);
                            }
                        }
                    }

                    if (GUILayout.Button("Reset To Initial", EditorStyles.miniButtonRight))
                    {
                        CommitInspectorChanges();
                        foreach (var obj in targets)
                        {
                            if (obj is UiJuiceAnimator animator)
                            {
                                Undo.RegisterFullObjectHierarchyUndo(animator.gameObject, "Reset UI Juice");
                                animator.ResetToInitialState();
                                EditorUtility.SetDirty(animator);
                            }
                        }
                    }
                }
            }
        }

        private void DrawHierarchySection()
        {
            DrawSectionHeader("Hierarchy");
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Scan Children", EditorStyles.miniButtonLeft))
                    {
                        CommitInspectorChanges();
                        foreach (var obj in targets)
                        {
                            if (obj is UiJuiceAnimator animator)
                            {
                                Undo.RecordObject(animator, "Scan UI Juice Children");
                                animator.RebuildFromHierarchy(includeInactiveProp.boolValue);
                                EditorUtility.SetDirty(animator);
                            }
                        }
                    }

                    if (GUILayout.Button("Clear Elements", EditorStyles.miniButtonRight))
                    {
                        CommitInspectorChanges();
                        foreach (var obj in targets)
                        {
                            if (obj is UiJuiceAnimator animator)
                            {
                                Undo.RecordObject(animator, "Clear UI Juice Elements");
                                animator.elements.Clear();
                                EditorUtility.SetDirty(animator);
                            }
                        }
                    }
                }
            }
        }

        private void DrawElementsSection()
        {
            DrawSectionHeader("Elements");
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(elementsProp, true);
            }
        }

        private void DrawDurationRange()
        {
            EditorGUILayout.LabelField("Duration Range (s)", EditorStyles.miniBoldLabel);

            var min = durationMinProp.floatValue;
            var max = durationMaxProp.floatValue;

            EditorGUILayout.MinMaxSlider(new GUIContent("Min / Max"), ref min, ref max, 0.1f, 5f);
            min = Mathf.Clamp(min, 0.1f, 5f);
            max = Mathf.Clamp(max, 0.1f, 5f);
            if (max < min) max = min;

            durationMinProp.floatValue = min;
            durationMaxProp.floatValue = max;

            Rect rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight);
            float spacing = 5f;
            float fieldWidth = (rect.width - spacing) / 2f;
            
            Rect minRect = new Rect(rect.x, rect.y, fieldWidth, rect.height);
            Rect maxRect = new Rect(rect.x + fieldWidth + spacing, rect.y, fieldWidth, rect.height);
            
            EditorGUI.BeginChangeCheck();
            float newMin = EditorGUI.FloatField(minRect, min);
            float newMax = EditorGUI.FloatField(maxRect, max);
            if (EditorGUI.EndChangeCheck())
            {
                durationMinProp.floatValue = newMin;
                durationMaxProp.floatValue = newMax;
            }
        }

        private static void DrawSectionHeader(string label)
        {
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
        }
    }
}

