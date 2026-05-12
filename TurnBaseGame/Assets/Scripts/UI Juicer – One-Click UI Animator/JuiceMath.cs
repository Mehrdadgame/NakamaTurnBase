using UnityEngine;

namespace JuiceUp
{
    /// <summary>
    /// Utility helpers used by the UI juicing runtime.
    /// </summary>
    public static class JuiceMath
    {
        public enum CurvePreset
        {
            EaseInOut,
            EaseOutBack,
            EaseOutElastic,
            EaseOutQuad,
            EaseInQuad,
            EaseOutExpo
        }

        /// <summary>
        /// Common default ease when none is provided.
        /// </summary>
        public static AnimationCurve DefaultEase => AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        /// <summary>
        /// Named curve presets for quick selection.
        /// </summary>
        public static AnimationCurve GetPreset(CurvePreset preset)
        {
            switch (preset)
            {
                case CurvePreset.EaseOutBack:
                    return new AnimationCurve(
                        new Keyframe(0f, 0f, 0f, 3f),
                        new Keyframe(0.7f, 1.1f),
                        new Keyframe(1f, 1f));
                case CurvePreset.EaseOutElastic:
                    return new AnimationCurve(
                        new Keyframe(0f, 0f, 0f, 5f),
                        new Keyframe(0.4f, 1.15f),
                        new Keyframe(0.7f, 0.95f),
                        new Keyframe(1f, 1f));
                case CurvePreset.EaseOutQuad:
                    return new AnimationCurve(
                        new Keyframe(0f, 0f, 0f, 2f),
                        new Keyframe(1f, 1f));
                case CurvePreset.EaseInQuad:
                    return new AnimationCurve(
                        new Keyframe(0f, 0f, 2f, 0f),
                        new Keyframe(1f, 1f));
                case CurvePreset.EaseOutExpo:
                    return new AnimationCurve(
                        new Keyframe(0f, 0f, 0f, 5f),
                        new Keyframe(1f, 1f));
                default:
                    return DefaultEase;
            }
        }

        /// <summary>
        /// Random preset helper for quick experimentation.
        /// </summary>
        public static AnimationCurve RandomPresetCurve()
        {
            var values = (CurvePreset[])System.Enum.GetValues(typeof(CurvePreset));
            var pick = values[Random.Range(0, values.Length)];
            return GetPreset(pick);
        }

        /// <summary>
        /// Safely evaluate an AnimationCurve with clamped input.
        /// </summary>
        public static float Evaluate(AnimationCurve curve, float t)
        {
            if (curve == null || curve.length == 0)
                return Mathf.Clamp01(t);

            return curve.Evaluate(Mathf.Clamp01(t));
        }

        /// <summary>
        /// Simple critically damped spring-like response in [0,1].
        /// </summary>
        public static float Spring01(float t, float damping = 0.35f, float frequency = 8f)
        {
            t = Mathf.Clamp01(t);
            var omega = Mathf.PI * 2f * frequency;
            var decay = Mathf.Exp(-damping * omega * t);
            return 1f - decay * Mathf.Cos((1f - damping) * omega * t);
        }

        /// <summary>
        /// Clamp and randomize a duration within provided min/max bounds.
        /// </summary>
        public static float RandomDuration(float min, float max, float clampMin, float clampMax)
        {
            var d = Random.Range(min, max);
            return Mathf.Clamp(d, clampMin, clampMax);
        }

        /// <summary>
        /// Applies variation jitter to an element's offsets and timings.
        /// </summary>
        public static void ApplyVariation(UiJuiceAnimator.JuiceElement e, float durationMin, float durationMax)
        {
            if (e == null)
                return;

            float posVar = Random.Range(0.85f, 1.25f);
            float rotVar = Random.Range(0.85f, 1.25f);
            float scaleVar = Random.Range(0.85f, 1.2f);
            float fadeVar = Random.Range(0.7f, 1.2f);
            float delayVar = Random.Range(0.7f, 1.2f);
            float durVar = Random.Range(0.9f, 1.15f);
            float curveVar = Random.Range(0.85f, 1.25f);

            e.positionOffset *= posVar;
            e.rotationOffset *= rotVar;
            e.scaleFrom = Vector3.one + (e.scaleFrom - Vector3.one) * scaleVar;
            e.fadeFrom *= fadeVar;
            e.delay *= delayVar;
            e.duration = Mathf.Clamp(e.duration * durVar, durationMin, durationMax);
            e.curveAmplitude *= curveVar;

            if (Random.value < 0.25f)
                e.ease = RandomPresetCurve();
        }

        /// <summary>
        /// Apply intensity scaling to offsets, scale deltas, fade, curve amplitude and duration.
        /// </summary>
        public static void ApplyIntensity(UiJuiceAnimator.JuiceElement e, float intensity, float durationMin, float durationMax)
        {
            if (e == null)
                return;

            float power = Mathf.Max(0.1f, intensity);

            e.positionOffset *= power;
            e.rotationOffset *= power;
            e.scaleFrom = Vector3.one + (e.scaleFrom - Vector3.one) * power;
            e.fadeFrom *= Mathf.Clamp01(power);
            e.curveAmplitude *= power;

            float durationScale = power >= 1f
                ? Mathf.Lerp(1f, 0.7f, Mathf.Clamp01(power - 1f))
                : Mathf.Lerp(1f, 1.3f, Mathf.Clamp01(1f - power));

            e.duration = Mathf.Clamp(e.duration * durationScale, durationMin, durationMax);
        }

        /// <summary>
        /// Evaluate a curved path between two positions.
        /// </summary>
        public static Vector2 EvaluateCurvePosition(Vector2 startPos, Vector2 endPos, float eased, bool useCurve, float curveAmplitude)
        {
            var basePos = Vector2.LerpUnclamped(startPos, endPos, eased);
            if (!useCurve || Mathf.Abs(curveAmplitude) < 0.001f)
                return basePos;

            var dir = endPos - startPos;
            if (dir.sqrMagnitude < 0.0001f)
                return basePos;

            var perp = new Vector2(-dir.y, dir.x).normalized;
            return basePos + perp * (curveAmplitude * Mathf.Sin(Mathf.PI * eased));
        }

        /// <summary>
        /// Compute shake jitter offsets (position and rotation) using Perlin noise.
        /// </summary>
        public static void SampleShake(UiJuiceAnimator.JuiceElement e, float time, out Vector2 posOffset, out float rotOffset)
        {
            posOffset = Vector2.zero;
            rotOffset = 0f;
            if (e == null || !e.useShakeJitter || (e.shakeMagnitude <= 0f && e.shakeRotationMagnitude <= 0f))
                return;

            float tShake = time * e.shakeFrequency + e.shakeSeed;
            posOffset.x = (Mathf.PerlinNoise(tShake, 0.123f) - 0.5f) * 2f * e.shakeMagnitude;
            posOffset.y = (Mathf.PerlinNoise(0.456f, tShake) - 0.5f) * 2f * e.shakeMagnitude;

            if (e.shakeRotationMagnitude > 0f)
            {
                rotOffset = (Mathf.PerlinNoise(tShake, tShake * 0.37f) - 0.5f) * 2f * e.shakeRotationMagnitude;
            }
        }
    }
}
