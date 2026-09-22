using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NinjaBattle.Editor
{
    public static class GameplayRegressionValidator
    {
        private static readonly string[] BattleScenes =
        {
            "Assets/-Scenes/4-VerticalAndHorizontal.unity",
            "Assets/-Scenes/5-FourByThree1.unity",
            "Assets/-Scenes/6-FourByFour1.unity"
        };

        [MenuItem("Tools/NinjaBattle/Validation/Repair Battle Scene References")]
        public static void RepairBattleSceneReferences()
        {
            Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprite/Sprite-Dice.png")
                .OfType<Sprite>()
                .ToArray();
            Sprite enabledSprite = sprites.FirstOrDefault(sprite => sprite.name == "Sprite-Dice_10");
            Sprite disabledSprite = sprites.FirstOrDefault(sprite => sprite.name == "Sprite-Dice_13");
            Ensure(enabledSprite != null && disabledSprite != null,
                "Could not resolve the enabled/disabled roll-button sprites.");

            foreach (string scenePath in BattleScenes)
            {
                Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                UiManager uiManager = UnityEngine.Object.FindObjectsByType<UiManager>(
                        FindObjectsInactive.Include, FindObjectsSortMode.None)
                    .FirstOrDefault();
                Ensure(uiManager != null, $"UiManager is missing from {scenePath}.");

                SerializedObject serialized = new SerializedObject(uiManager);
                SerializedProperty spritesProperty = serialized.FindProperty("DiceRollsSprite");
                bool needsRepair = spritesProperty.arraySize != 2 ||
                                   spritesProperty.GetArrayElementAtIndex(0).objectReferenceValue != enabledSprite ||
                                   spritesProperty.GetArrayElementAtIndex(1).objectReferenceValue != disabledSprite;
                if (!needsRepair)
                    continue;

                spritesProperty.arraySize = 2;
                spritesProperty.GetArrayElementAtIndex(0).objectReferenceValue = enabledSprite;
                spritesProperty.GetArrayElementAtIndex(1).objectReferenceValue = disabledSprite;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(uiManager);
                EditorSceneManager.SaveScene(scene);
            }

            Debug.Log("GameplayRegressionValidator: battle scene references repaired.");
        }

        [MenuItem("Tools/NinjaBattle/Validation/Validate Gameplay Changes")]
        public static void ValidateGameplayChanges()
        {
            ValidateMatchParticles();
            ValidateTutorialInputRules();
            ValidateBattleScenes();
            Debug.Log("GameplayRegressionValidator: all gameplay checks passed.");
        }

        [MenuItem("Tools/NinjaBattle/Validation/Validate Match Particles")]
        public static void ValidateMatchParticles()
        {
            ValidateMatchParticleColor(2);
            ValidateMatchParticleColor(3);
            Debug.Log("GameplayRegressionValidator: match particle checks passed.");
        }

        private static void ValidateTutorialInputRules()
        {
            Ensure(TutorialInputRules.CanRoll(false, false), "Normal gameplay roll was blocked.");
            Ensure(!TutorialInputRules.CanRoll(true, false), "Tutorial allowed an out-of-step roll.");
            Ensure(TutorialInputRules.CanRoll(true, true), "Tutorial blocked the requested roll.");

            Ensure(!TutorialInputRules.CanPlace(true, false, 2, -1, false, 0),
                "Tutorial allowed an out-of-step placement.");
            Ensure(TutorialInputRules.CanPlace(true, true, 2, -1, false, 2),
                "Tutorial blocked the first guided placement.");
            Ensure(TutorialInputRules.CanPlace(true, true, 6, 1, false, 1),
                "Tutorial blocked the same-line double placement.");
            Ensure(!TutorialInputRules.CanPlace(true, true, 6, 1, false, 2),
                "Tutorial allowed the double practice outside the original line.");
            Ensure(TutorialInputRules.CanPlace(true, true, 6, 1, true, 2),
                "Tutorial stayed line-locked after the anchor die was eliminated.");
        }

        private static void ValidateBattleScenes()
        {
            foreach (string scenePath in BattleScenes)
            {
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                UiManager uiManager = UnityEngine.Object.FindObjectsByType<UiManager>(
                        FindObjectsInactive.Include, FindObjectsSortMode.None)
                    .FirstOrDefault();
                Ensure(uiManager != null, $"UiManager is missing from {scenePath}.");

                SerializedObject serialized = new SerializedObject(uiManager);
                Ensure(serialized.FindProperty("dicRollButton").objectReferenceValue != null,
                    $"Roll button is not wired in {scenePath}.");
                SerializedProperty spritesProperty = serialized.FindProperty("DiceRollsSprite");
                Ensure(spritesProperty.arraySize >= 2 &&
                       spritesProperty.GetArrayElementAtIndex(0).objectReferenceValue != null &&
                       spritesProperty.GetArrayElementAtIndex(1).objectReferenceValue != null,
                    $"Roll button sprites are incomplete in {scenePath}.");

                ClickInCell[] cells = UnityEngine.Object.FindObjectsByType<ClickInCell>(
                    FindObjectsInactive.Include, FindObjectsSortMode.None);
                Ensure(cells.Length > 0, $"No local board cells were found in {scenePath}.");
                Ensure(cells.All(cell => cell.GetComponentInChildren<ParticleSystem>(true) != null),
                    $"A local board cell has no particle system in {scenePath}.");

                if (scenePath == BattleScenes[0])
                    ValidateTutorialWiring(scenePath);
            }
        }

        private static void ValidateTutorialWiring(string scenePath)
        {
            TutorialManager tutorial = UnityEngine.Object.FindObjectsByType<TutorialManager>(
                    FindObjectsInactive.Include, FindObjectsSortMode.None)
                .FirstOrDefault();
            Ensure(tutorial != null, $"TutorialManager is missing from {scenePath}.");

            SerializedObject serialized = new SerializedObject(tutorial);
            string[] requiredReferences =
            {
                "overlayGroup", "overlayPanel", "bubbleRect", "bubbleGroup", "messageText",
                "stepText", "nextButton", "nextButtonText", "skipButton", "arrowImage",
                "highlightRect", "diceBtnRect", "myGridRect", "oppGridRect", "scoreAreaRect"
            };

            foreach (string propertyName in requiredReferences)
            {
                SerializedProperty property = serialized.FindProperty(propertyName);
                Ensure(property != null && property.objectReferenceValue != null,
                    $"Tutorial reference '{propertyName}' is missing in {scenePath}.");
            }
        }

        private static void ValidateMatchParticleColor(int matchCount)
        {
            GameObject calculatorObject = new GameObject("ParticleRegressionCalculator");
            var calculator = calculatorObject.AddComponent<CalculterRowScore>();
            calculator.whitecolor = Color.white;
            calculator.colorParticle2Count = new Color(0.1f, 0.8f, 1f, 1f);
            calculator.colorParticle3Count = new Color(1f, 0.35f, 0.1f, 1f);
            calculator.colorParticle4Count = new Color(0.8f, 0.2f, 1f, 1f);

            var cells = new List<ClickInCell>();
            try
            {
                for (int i = 0; i < matchCount; i++)
                    cells.Add(CreatePlayingCell(i, 3));

                calculator.TileMe(cells, out int count);
                if (count != matchCount)
                    throw new InvalidOperationException($"Expected a {matchCount}-match, got {count}.");

                Color expected = matchCount == 2
                    ? calculator.colorParticle2Count
                    : calculator.colorParticle3Count;

                foreach (ClickInCell cell in cells)
                {
                    ParticleSystem particle = cell.GetComponentInChildren<ParticleSystem>();
                    if (!particle.isPlaying)
                        throw new InvalidOperationException($"{matchCount}-match particle did not restart.");

                    particle.Simulate(0.1f, true, false, false);
                    var liveParticles = new ParticleSystem.Particle[particle.particleCount];
                    int liveCount = particle.GetParticles(liveParticles);
                    if (liveCount == 0)
                        throw new InvalidOperationException("The regression fixture did not emit particles.");

                    Color actual = liveParticles[0].GetCurrentColor(particle);
                    if (!Approximately(actual, expected))
                    {
                        throw new InvalidOperationException(
                            $"{matchCount}-match kept stale particle color {actual}; expected {expected}.");
                    }
                }
            }
            finally
            {
                foreach (ClickInCell cell in cells)
                {
                    if (cell != null)
                        UnityEngine.Object.DestroyImmediate(cell.gameObject);
                }
                UnityEngine.Object.DestroyImmediate(calculatorObject);
            }
        }

        private static ClickInCell CreatePlayingCell(int index, int value)
        {
            GameObject cellObject = new GameObject($"Cell_{index}");
            var cell = cellObject.AddComponent<ClickInCell>();
            cell.numberLine = 0;
            cell.numberRow = index;
            cell.ValueTile = value;
            cell.isLock = true;

            GameObject particleObject = new GameObject("MatchParticle");
            particleObject.transform.SetParent(cellObject.transform, false);
            var particle = particleObject.AddComponent<ParticleSystem>();
            var main = particle.main;
            main.loop = true;
            main.startLifetime = 5f;
            main.startSpeed = 0f;
            main.startSize = 1f;
            main.startColor = Color.white;
            var emission = particle.emission;
            emission.rateOverTime = 30f;
            particle.Play(true);
            particle.Simulate(0.25f, true, false, false);

            return cell;
        }

        private static bool Approximately(Color actual, Color expected)
        {
            const float tolerance = 0.03f;
            return Mathf.Abs(actual.r - expected.r) <= tolerance &&
                   Mathf.Abs(actual.g - expected.g) <= tolerance &&
                   Mathf.Abs(actual.b - expected.b) <= tolerance &&
                   Mathf.Abs(actual.a - expected.a) <= tolerance;
        }

        private static void Ensure(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }
    }
}
