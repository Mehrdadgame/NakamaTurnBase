using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace JuiceUp
{
    /// <summary>
    /// Attach to a UI root to play a staggered intro tween across its children.
    /// </summary>
    [DisallowMultipleComponent]
    public class UiJuiceAnimator : MonoBehaviour
    {
        public enum FeelingPreset
        {
            Happy,
            Scary,
            Slow,
            Shaky,
            Calm,
            Energetic,
            Bounce,
            Gentle,
            Dramatic,
            Snappy,
            Heavy,
            Pop,
            Drift,
            Punchy,
            Rubber,
            SoftRise,
            Flashy,
            SlideLeft,
            SlideRight,
            Flip,
            ZoomPop,
            Wobble,
            Ripple,
            Stomp,
            FloatUp,
            SwoopIn,
            Blink,
            Cascade,
            Jitter,
            PulseGlow,
            TiltDrop,
            SnapLeft,
            SnapRight,
            BounceInPlace,
            DriftSlow,
            PopRotate,
            Whip,
            Wave,
            Breath,
            FlashSlideDown,
            SlideOutLeft,
            SlideOutRight,
            FadeOut,
            FlipOut,
            PopFlash,
            GlowPulse,
            RippleMask,
            SpiralIn,
            GlitchPop,
            BreathingIdle,
            Darker,
            RainbowFlash,
            WarmFlash,
            CoolFlash,
            Desaturate,
            NeonPulse,
            ColorCycle
        }

        [System.Serializable]
        public class JuiceElement
        {
            [Tooltip("The UI element (RectTransform) that this animation entry will control.")]
            public RectTransform target;

            [Header("Position")]
            [Tooltip("If enabled, animates anchoredPosition using Position Offset (and optional curve/shake).")]
            public bool animatePosition = true;
            [Tooltip("Where the element starts (relative to its initial position) when playing IN. When playing OUT, this becomes the end offset.")]
            public Vector2 positionOffset = new Vector2(0f, -80f);
            [Tooltip("Adds a curved arc to the position tween.")]
            public bool useCurvePath = true;
            [Tooltip("Arc height for the curve path.")]
            public float curveAmplitude = 20f;
            [Tooltip("Enables high-frequency shaking jitter.")]
            public bool useShakeJitter = false;
            [Tooltip("Magnitude of shake jitter in UI units.")]
            public float shakeMagnitude = 6f;
            [Tooltip("Rotation jitter in degrees.")]
            public float shakeRotationMagnitude = 4f;
            [Tooltip("Shake oscillations per second.")]
            public float shakeFrequency = 20f;
            [HideInInspector] public float shakeSeed = 0f;

            [Header("Rotation")]
            [Tooltip("If enabled, animates localRotation using Rotation Offset.")]
            public bool animateRotation = false;
            [Tooltip("Rotation applied at the start of Play IN (and at the end of Play OUT). Z is most commonly used for UI.")]
            public Vector3 rotationOffset = new Vector3(0f, 0f, 12f);

            [Header("Scale")]
            [Tooltip("If enabled, animates localScale using Scale From.")]
            public bool animateScale = true;
            [Tooltip("Scale at the start of Play IN (and at the end of Play OUT).")]
            public Vector3 scaleFrom = new Vector3(0.85f, 0.85f, 1f);

            [Header("Fade")]
            [Tooltip("If enabled, animates a CanvasGroup alpha (adds one if missing).")]
            public bool animateFade = false;
            [Tooltip("Alpha at the start of Play IN (and at the end of Play OUT).")]
            public float fadeFrom = 0f;

            [Header("Color/FX")]
            [Tooltip("If enabled, animates the Graphic color (Image/Text/TMP via Graphic) from Color From.")]
            public bool animateColor = false;
            [Tooltip("Optional graphic to animate color on; defaults to target's Graphic.")]
            public Graphic graphicOverride;
            [Tooltip("Color at the start of Play IN (and at the end of Play OUT).")]
            public Color colorFrom = Color.white;

            [Header("Timing")]
            [Tooltip("How long this element's tween takes (seconds).")]
            public float duration = 0.45f;
            [Tooltip("Extra delay before this element starts (seconds). Stagger is added on top.")]
            public float delay = 0f;
            [Tooltip("Easing curve for this element. X=time (0..1), Y=progress (0..1).")]
            public AnimationCurve ease = JuiceMath.DefaultEase;

            // Cached initial state so we can reliably reset before each play.
            [HideInInspector] public Vector2 initialPosition;
            [HideInInspector] public Quaternion initialRotation = Quaternion.identity;
            [HideInInspector] public Vector3 initialScale = Vector3.one;
            [HideInInspector] public float initialAlpha = 1f;
            [HideInInspector] public Color initialColor = Color.white;
            [HideInInspector] public bool hasInitialState = false;

            public static JuiceElement CreateDefault(RectTransform target)
            {
                return new JuiceElement
                {
                    target = target,
                    animatePosition = true,
                    positionOffset = new Vector2(0f, -80f),
                    animateRotation = false,
                    rotationOffset = new Vector3(0f, 0f, 12f),
                    animateScale = true,
                    scaleFrom = new Vector3(0.85f, 0.85f, 1f),
                    animateFade = false,
                    fadeFrom = 0f,
                    animateColor = false,
                    colorFrom = Color.white,
                    duration = 0.45f,
                    delay = 0f,
                    ease = JuiceMath.DefaultEase
                };
            }
        }

        [Header("Playback")]
        [Tooltip("If enabled, automatically plays Play IN when the object becomes enabled (Play Mode by default; optionally Edit Mode too).")]
        public bool autoPlayOnEnable = true;
        [Tooltip("Additional per-element delay (seconds) applied based on list order. Creates a cascading/staggered effect.")]
        public float stagger = 0.01f;
        [Tooltip("When scanning children, include inactive UI objects too (useful for menus that start disabled).")]
        public bool includeInactiveChildren = false;
        [FormerlySerializedAs("autoRandomizeOnEnable")]
        [Tooltip("If enabled, re-applies the current preset (with variation) right before playing (Play In).")]
        public bool autoRandomizeOnPlay = true;
        [Tooltip("Intensity multiplier applied to the chosen feeling/preset. Higher = bigger offsets, punchier motion, shorter durations.")]
        [Range(0.1f, 2.5f)]
        public float strength = 1f;
        [Header("Timing Range (s)")]
        [Tooltip("Minimum duration (seconds) used by presets/variation. Element durations will be clamped to this range.")]
        [Range(0.1f, 5f)] public float durationMin = 0.35f;
        [Tooltip("Maximum duration (seconds) used by presets/variation. Element durations will be clamped to this range.")]
        [Range(0.1f, 5f)] public float durationMax = 0.8f;
        [Tooltip("The style/feeling to apply across elements (offsets, curves, timing, etc.).")]
        public FeelingPreset preset = FeelingPreset.Happy;

        [Header("Runtime Completion (Play Mode Only)")]
        [Tooltip("If enabled, the GameObject will be deactivated when a Play OUT finishes (after the last element completes). Runtime only.")]
        public bool deactivateGameObjectAfterPlayOut = false;
        [Tooltip("If enabled, the GameObject will be destroyed when a Play OUT finishes (after the last element completes). Runtime only. Takes precedence over Deactivate.")]
        public bool destroyGameObjectAfterPlayOut = false;

        [Header("Edit Mode Preview")]
        [Tooltip("Allows Play In/Out buttons to animate while the Editor is not in Play Mode.")]
        public bool allowPreviewInEditMode = false;
        [Tooltip("If true, Auto Play On Enable can run in edit mode too (can move UI in the scene while not playing).")]
        public bool allowAutoPlayInEditMode = false;
        [Tooltip("Automatically applies the selected Feeling Preset when values change in the Inspector (edit mode).")]
        public bool autoApplyPresetInEditMode = false;
        [Tooltip("Enables verbose debug logging (can be noisy).")]
        public bool verboseLogs = false;

        [Header("Elements")]
        [Tooltip("List of UI elements that will be animated. Use 'Scan Children' in the inspector to populate automatically.")]
        public List<JuiceElement> elements = new List<JuiceElement>();

        private readonly HashSet<CanvasGroup> addedCanvasGroups = new HashSet<CanvasGroup>();
        private readonly List<ActiveAnimation> activeAnimations = new List<ActiveAnimation>();
        private bool pendingAutoPlay = false;
        private bool lastRequestedPlayIn = true;
        private const string LogPrefix = "[UiJuiceAnimator] ";
        private static int autoRandomSeedCounter = 0;

#if UNITY_EDITOR
        [SerializeField, HideInInspector] private bool presetCacheValid = false;
        [SerializeField, HideInInspector] private FeelingPreset cachedPreset;
        [SerializeField, HideInInspector] private float cachedStrength;
        [SerializeField, HideInInspector] private float cachedDurationMin;
        [SerializeField, HideInInspector] private float cachedDurationMax;
#endif

#if UNITY_EDITOR
        private bool editorUpdateHooked = false;
#endif

        private struct ActiveAnimation
        {
            public JuiceElement element;
            public bool playIn;
            public float startTime;
            public float duration;
            public Vector2 startPos;
            public Vector2 endPos;
            public Quaternion startRot;
            public Quaternion endRot;
            public Vector3 startScale;
            public Vector3 endScale;
            public float startAlpha;
            public float endAlpha;
            public Color startColor;
            public Color endColor;
            public CanvasGroup group;
            public Graphic graphic;
            public bool createdCanvasGroup;
        }

        private void Reset()
        {
            RebuildFromHierarchy(includeInactiveChildren);
        }

        private void OnValidate()
        {
            durationMin = Mathf.Clamp(durationMin, 0.1f, 5f);
            durationMax = Mathf.Clamp(durationMax, 0.1f, 5f);
            if (durationMax < durationMin)
                durationMax = durationMin;

            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i] == null)
                {
                    elements[i] = new JuiceElement();
                }

                if (elements[i].ease == null || elements[i].ease.length == 0)
                    elements[i].ease = JuiceMath.DefaultEase;

                elements[i].curveAmplitude = Mathf.Max(0f, elements[i].curveAmplitude);
                elements[i].shakeMagnitude = Mathf.Max(0f, elements[i].shakeMagnitude);
                elements[i].shakeRotationMagnitude = Mathf.Max(0f, elements[i].shakeRotationMagnitude);
                elements[i].shakeFrequency = Mathf.Max(0f, elements[i].shakeFrequency);
            }

#if UNITY_EDITOR
            if (!Application.isPlaying && autoApplyPresetInEditMode)
                AutoApplyPresetIfNeeded();
#endif
        }

        private void OnEnable()
        {
            pendingAutoPlay = autoPlayOnEnable;

            if (autoPlayOnEnable && (Application.isPlaying || allowAutoPlayInEditMode))
                RequestPlay(true, true);
            else if (!Application.isPlaying)
                pendingAutoPlay = false;

            LogDebug($"OnEnable autoPlayOnEnable={autoPlayOnEnable} elements={(elements == null ? 0 : elements.Count)}");
        }

        private void Start()
        {
            // If auto-play was requested but nothing is running yet, try again after Start.
            if (Application.isPlaying && autoPlayOnEnable && activeAnimations.Count == 0)
            {
                LogDebug("Start retrying auto-play.");
                RequestPlay(true, true);
            }
        }

        private void OnDisable()
        {
            Stop();
        }

        public void Play()
        {
            if (!isActiveAndEnabled)
                return;

            RequestPlay(true, false);
        }

        public void Stop()
        {
            activeAnimations.Clear();
            CleanupAddedCanvasGroups();
#if UNITY_EDITOR
            StopEditorUpdateLoop();
#endif
        }

        public void PlayIn()
        {
            // Common UX: calling PlayIn should "show" the UI even if it was hidden.
            // If the GameObject/component is disabled, re-enable it first, then play IN.
            bool wasInactive = !gameObject.activeSelf;
            bool wasDisabled = !enabled;

            if (wasInactive)
                gameObject.SetActive(true);
            if (wasDisabled)
                enabled = true;

            // If we just enabled this object in Play Mode and Auto Play On Enable is on,
            // OnEnable will already have started Play IN. Avoid re-triggering.
            if ((wasInactive || wasDisabled) && Application.isPlaying && autoPlayOnEnable)
                return;

            Play();
        }

        public void PlayOut()
        {
            if (!isActiveAndEnabled)
                return;

            RequestPlay(false, false);
        }

        public void RandomizeElements()
        {
            ApplyFeelingPreset(preset);
        }

        public void ApplyFeelingPreset()
        {
            ApplyFeelingPreset(preset);
        }

        public void ApplyFeelingPreset(FeelingPreset feeling)
        {
            if (elements == null || elements.Count == 0)
                return;

            // NOTE: UnityEngine.Random is a global RNG. In Edit Mode (and across domain reloads),
            // leaving it unseeded can feel "not random" because the global state may be reset/reused.
            // We also don't want to disturb other systems' randomness, so we save/restore Random.state.
            var previousRandomState = Random.state;
            int seedToUse = GetEphemeralRandomSeed();
            Random.InitState(seedToUse);

            try
            {
                for (int i = 0; i < elements.Count; i++)
                {
                    var e = elements[i];
                    if (e == null || e.target == null)
                        continue;

                    // In edit mode, designers expect the "initial" state to track the current layout.
                    CaptureInitialState(e, force: !Application.isPlaying);
                    JuicePresetLibrary.ApplyPreset(e, feeling, durationMin, durationMax);

                    ApplyVariation(e);
                    ApplyIntensity(e, strength);
                }
            }
            finally
            {
                Random.state = previousRandomState;
            }
        }

        private int GetEphemeralRandomSeed()
        {
            unchecked
            {
                // Combine time + instance id + a counter to avoid collisions when called multiple times quickly (e.g., OnValidate).
                autoRandomSeedCounter++;
                int t = (int)System.DateTime.UtcNow.Ticks;
                return t ^ (GetInstanceID() * 486187739) ^ (autoRandomSeedCounter * 16777619);
            }
        }

        public void RebuildFromHierarchy(bool includeInactive)
        {
            elements.Clear();

            var children = GetComponentsInChildren<RectTransform>(includeInactive);
            foreach (var child in children)
            {
                if (child == transform)
                    continue;

                elements.Add(JuiceElement.CreateDefault(child));
            }
        }

        private void RequestPlay(bool playIn, bool fromAutoPlay)
        {
            if (!isActiveAndEnabled)
            {
                LogDebug("RequestPlay aborted: component not active/enabled.");
                return;
            }

            lastRequestedPlayIn = playIn;

#if UNITY_EDITOR
            if (!Application.isPlaying && !allowPreviewInEditMode)
            {
                LogDebug("RequestPlay aborted: edit-mode preview disabled.");
                return;
            }
#endif

            // Only randomize before Play IN, so we don't change presets while playing OUT.
            if (playIn && autoRandomizeOnPlay)
                RandomizeElements();

            Stop();
            bool prepared = PrepareAnimations(playIn);

            if (!prepared && fromAutoPlay)
            {
                pendingAutoPlay = true;
                LogDebug("Play request deferred: no elements ready yet (auto play).");
            }
            else if (!prepared)
            {
                LogDebug("Play request ignored: no elements ready.");
            }
            else
            {
                pendingAutoPlay = false;
                LogDebug($"Play {(playIn ? "IN" : "OUT")} started with {activeAnimations.Count} element(s).");
            }

#if UNITY_EDITOR
            if (!Application.isPlaying && activeAnimations.Count > 0)
                StartEditorUpdateLoop();
#endif
        }

        private bool PrepareAnimations(bool playIn)
        {
            activeAnimations.Clear();
            CleanupAddedCanvasGroups();

            if (elements == null || elements.Count == 0)
            {
                LogDebug("PrepareAnimations: no elements list.");
                return false;
            }

            float now = GetNow();
            int added = 0;
            int skippedNull = 0;
            int skippedInactive = 0;

            for (int i = 0; i < elements.Count; i++)
            {
                var element = elements[i];
                if (element == null || element.target == null)
                {
                    skippedNull++;
                    continue;
                }

                if (!element.target.gameObject.activeInHierarchy)
                {
                    skippedInactive++;
                    continue;
                }

                var rect = element.target;
                CaptureInitialState(element);
                ResetToInitial(element);

                float startDelay = element.delay + i * Mathf.Max(0f, stagger);
                float startTime = now + startDelay;
                float duration = Mathf.Max(0.01f, element.duration);

                bool createdCanvasGroup = false;
                CanvasGroup group = element.animateFade ? GetOrAddCanvasGroup(rect.gameObject, out createdCanvasGroup) : rect.GetComponent<CanvasGroup>();
                Graphic graphic = element.animateColor
                    ? (element.graphicOverride ? element.graphicOverride : rect.GetComponent<Graphic>())
                    : null;

                var targetPos = element.initialPosition;
                var targetRot = element.initialRotation;
                var targetScale = element.initialScale;

                float startAlpha = playIn ? element.fadeFrom : element.initialAlpha;
                float endAlpha = playIn ? element.initialAlpha : 0f;

                Color startColor = playIn ? element.colorFrom : element.initialColor;
                Color endColor = playIn ? element.initialColor : element.colorFrom;

                Vector2 startPos = playIn ? targetPos + element.positionOffset : targetPos;
                Vector2 endPos = playIn ? targetPos : targetPos + element.positionOffset;

                Quaternion startRot = playIn ? targetRot * Quaternion.Euler(element.rotationOffset) : targetRot;
                Quaternion endRot = playIn ? targetRot : targetRot * Quaternion.Euler(element.rotationOffset);

                Vector3 startScale = playIn ? element.scaleFrom : targetScale;
                Vector3 endScale = playIn ? targetScale : element.scaleFrom;

                if (element.animatePosition)
                    rect.anchoredPosition = startPos;

                if (element.animateRotation)
                    rect.localRotation = startRot;

                if (element.animateScale)
                    rect.localScale = startScale;

                if (element.animateFade && group != null)
                    group.alpha = startAlpha;
                if (element.animateColor && graphic != null)
                    graphic.color = startColor;

                activeAnimations.Add(new ActiveAnimation
                {
                    element = element,
                    playIn = playIn,
                    startTime = startTime,
                    duration = duration,
                    startPos = startPos,
                    endPos = endPos,
                    startRot = startRot,
                    endRot = endRot,
                    startScale = startScale,
                    endScale = endScale,
                    startAlpha = startAlpha,
                    endAlpha = endAlpha,
                    startColor = startColor,
                    endColor = endColor,
                    group = group,
                    graphic = graphic,
                    createdCanvasGroup = createdCanvasGroup
                });

                added++;
            }

            LogDebug($"PrepareAnimations: added={added}, skippedNull={skippedNull}, skippedInactive={skippedInactive}");
            return added > 0;
        }

        private void Update()
        {
            if (!Application.isPlaying)
                return;

            Tick(GetNow());
        }

        private void Tick(float now)
        {
            if (pendingAutoPlay && activeAnimations.Count == 0 && elements != null && elements.Count > 0)
            {
                RequestPlay(true, true);
                if (activeAnimations.Count == 0)
                {
                    LogDebug("AutoPlay retry: elements present but still no animations prepared.");
                    return;
                }
            }

            if (activeAnimations.Count == 0)
                return;

            for (int i = activeAnimations.Count - 1; i >= 0; i--)
            {
                var anim = activeAnimations[i];
                var element = anim.element;
                var rect = element.target;

                if (rect == null || !rect.gameObject.activeInHierarchy)
                {
                    if (anim.createdCanvasGroup && anim.group != null)
                    {
                        addedCanvasGroups.Remove(anim.group);
                        Destroy(anim.group);
                    }
                    activeAnimations.RemoveAt(i);
                    continue;
                }

                float elapsed = now - anim.startTime;
                if (elapsed < 0f)
                    continue;

                float t = anim.duration <= 0.0001f ? 1f : Mathf.Clamp01(elapsed / anim.duration);
                var easeCurve = element.ease == null || element.ease.length == 0 ? JuiceMath.DefaultEase : element.ease;
                float eased = JuiceMath.Evaluate(easeCurve, t);
                JuiceMath.SampleShake(element, now, out var shakePos, out var shakeRot);

                if (element.animatePosition)
                {
                    var basePos = JuiceMath.EvaluateCurvePosition(anim.startPos, anim.endPos, eased, element.useCurvePath && element.positionOffset.sqrMagnitude > 0.001f, element.curveAmplitude);
                    rect.anchoredPosition = basePos + shakePos;
                }

                if (element.animateRotation)
                {
                    var baseRot = Quaternion.SlerpUnclamped(anim.startRot, anim.endRot, eased);
                    if (Mathf.Abs(shakeRot) > 0.001f)
                        baseRot = Quaternion.Euler(0f, 0f, baseRot.eulerAngles.z + shakeRot);
                    rect.localRotation = baseRot;
                }

                if (element.animateScale)
                    rect.localScale = Vector3.LerpUnclamped(anim.startScale, anim.endScale, eased);

                if (element.animateFade && anim.group != null)
                    anim.group.alpha = Mathf.LerpUnclamped(anim.startAlpha, anim.endAlpha, eased);
                if (element.animateColor && anim.graphic != null)
                    anim.graphic.color = Color.LerpUnclamped(anim.startColor, anim.endColor, eased);

                if (t >= 1f)
                {
                    if (element.animatePosition)
                        rect.anchoredPosition = anim.endPos;

                    if (element.animateRotation)
                        rect.localRotation = anim.endRot;

                    if (element.animateScale)
                        rect.localScale = anim.endScale;

                    if (element.animateFade && anim.group != null)
                        anim.group.alpha = anim.endAlpha;
                    if (element.animateColor && anim.graphic != null)
                        anim.graphic.color = anim.endColor;

                    if (anim.createdCanvasGroup && anim.group != null)
                    {
                        addedCanvasGroups.Remove(anim.group);
                        DestroyUnityObject(anim.group);
                    }

                    activeAnimations.RemoveAt(i);

                    if (activeAnimations.Count == 0)
                    {
                        OnAllAnimationsFinished();
#if UNITY_EDITOR
                        if (!Application.isPlaying)
                            StopEditorUpdateLoop();
#endif
                    }
                }
            }
        }

        private void OnAllAnimationsFinished()
        {
            LogDebug("All animations finished.");

            // Runtime-only completion actions for Play OUT.
            if (!Application.isPlaying)
                return;
            if (lastRequestedPlayIn)
                return;

            if (destroyGameObjectAfterPlayOut)
            {
                Destroy(gameObject);
                return;
            }

            if (deactivateGameObjectAfterPlayOut)
            {
                gameObject.SetActive(false);
            }
        }

        private CanvasGroup GetOrAddCanvasGroup(GameObject go, out bool created)
        {
            if (go.TryGetComponent<CanvasGroup>(out var cg))
            {
                created = false;
            }
            else
            {
                cg = go.AddComponent<CanvasGroup>();
                created = true;
                addedCanvasGroups.Add(cg);
            }

            return cg;
        }

        private void LogDebug(string message)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!verboseLogs)
                return;
            Debug.Log($"{LogPrefix}{message}", this);
#endif
        }

        private void ApplyIntensity(JuiceElement e, float intensity)
        {
            JuiceMath.ApplyIntensity(e, intensity, durationMin, durationMax);
        }

        private float RandomDuration(float min, float max)
        {
            return JuiceMath.RandomDuration(min, max, durationMin, durationMax);
        }

        private void ApplyVariation(JuiceElement e)
        {
            JuiceMath.ApplyVariation(e, durationMin, durationMax);
        }

        private void CaptureInitialState(JuiceElement e, bool force = false)
        {
            if (e == null || e.target == null)
                return;

            if (e.hasInitialState && !force)
                return;

            var rect = e.target;
            e.initialPosition = rect.anchoredPosition;
            e.initialRotation = rect.localRotation;
            e.initialScale = rect.localScale;

            var cg = rect.GetComponent<CanvasGroup>();
            e.initialAlpha = cg ? cg.alpha : 1f;
            var g = e.graphicOverride ? e.graphicOverride : rect.GetComponent<Graphic>();
            if (g != null)
                e.initialColor = g.color;
            e.hasInitialState = true;
        }

        private void ResetToInitial(JuiceElement e)
        {
            if (e == null || e.target == null || !e.hasInitialState)
                return;

            var rect = e.target;
            rect.anchoredPosition = e.initialPosition;
            rect.localRotation = e.initialRotation;
            rect.localScale = e.initialScale;

            bool created;
            var cg = e.animateFade ? GetOrAddCanvasGroup(rect.gameObject, out created) : rect.GetComponent<CanvasGroup>();
            if (cg != null)
                cg.alpha = e.initialAlpha;

            if (e.animateColor)
            {
                var g = e.graphicOverride ? e.graphicOverride : rect.GetComponent<Graphic>();
                if (g != null)
                    g.color = e.initialColor;
            }
        }

        private void CleanupAddedCanvasGroups()
        {
            foreach (var cg in addedCanvasGroups)
            {
                if (cg != null)
                    DestroyUnityObject(cg);
            }

            addedCanvasGroups.Clear();
        }

        private float GetNow()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return (float)EditorApplication.timeSinceStartup;
#endif
            return Time.time;
        }

        private void DestroyUnityObject(Object obj)
        {
            if (obj == null)
                return;

            if (Application.isPlaying)
                Destroy(obj);
            else
                DestroyImmediate(obj);
        }

        public void ResetToInitialState()
        {
            Stop();
            if (elements == null)
                return;
            for (int i = 0; i < elements.Count; i++)
            {
                var e = elements[i];
                if (e == null || e.target == null)
                    continue;
                CaptureInitialState(e);
                ResetToInitial(e);
            }
        }

#if UNITY_EDITOR
        private void AutoApplyPresetIfNeeded()
        {
            // Avoid re-applying on every OnValidate (which can be called very often).
            // Only re-apply when the preset-driving inputs changed.
            bool changed =
                !presetCacheValid ||
                cachedPreset != preset ||
                Mathf.Abs(cachedStrength - strength) > 0.0001f ||
                Mathf.Abs(cachedDurationMin - durationMin) > 0.0001f ||
                Mathf.Abs(cachedDurationMax - durationMax) > 0.0001f;

            if (!changed)
                return;

            ApplyFeelingPreset(preset);

            presetCacheValid = true;
            cachedPreset = preset;
            cachedStrength = strength;
            cachedDurationMin = durationMin;
            cachedDurationMax = durationMax;

            EditorUtility.SetDirty(this);
        }
#endif

#if UNITY_EDITOR
        private void StartEditorUpdateLoop()
        {
            if (editorUpdateHooked)
                return;
            if (Application.isPlaying)
                return;
            if (!allowPreviewInEditMode)
                return;

            editorUpdateHooked = true;
            EditorApplication.update += EditorUpdate;
        }

        private void StopEditorUpdateLoop()
        {
            if (!editorUpdateHooked)
                return;
            editorUpdateHooked = false;
            EditorApplication.update -= EditorUpdate;
        }

        private void EditorUpdate()
        {
            if (this == null)
            {
                StopEditorUpdateLoop();
                return;
            }

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Stop();
                StopEditorUpdateLoop();
                return;
            }

            if (!isActiveAndEnabled || !allowPreviewInEditMode)
            {
                StopEditorUpdateLoop();
                return;
            }

            if (activeAnimations.Count == 0 && !pendingAutoPlay)
            {
                StopEditorUpdateLoop();
                return;
            }

            Tick(GetNow());
            EditorApplication.QueuePlayerLoopUpdate();
            SceneView.RepaintAll();
        }
#endif
    }
}

