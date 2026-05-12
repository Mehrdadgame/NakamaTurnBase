using UnityEngine;

namespace JuiceUp
{
    /// <summary>
    /// Central place to configure feeling presets.
    /// </summary>
    public static class JuicePresetLibrary
    {
        public static void ApplyPreset(UiJuiceAnimator.JuiceElement e, UiJuiceAnimator.FeelingPreset feeling, float durationMin, float durationMax)
        {
            if (e == null)
                return;

            SetDefaults(e);

            switch (feeling)
            {
                case UiJuiceAnimator.FeelingPreset.Happy:
                    e.animatePosition = true;
                    e.positionOffset = new Vector2(Random.Range(-40f, 40f), Random.Range(-90f, -40f));
                    e.animateRotation = false;
                    e.rotationOffset = Vector3.zero;
                    e.animateScale = true;
                    e.scaleFrom = Vector3.one * Random.Range(0.94f, 1.04f);
                    e.animateFade = true;
                    e.fadeFrom = 0f;
                    e.duration = RD(0.38f, 0.6f, durationMin, durationMax);
                    e.delay = Random.Range(0f, 0.07f);
                    e.ease = JuiceMath.GetPreset(JuiceMath.CurvePreset.EaseOutBack);
                    break;

                case UiJuiceAnimator.FeelingPreset.Scary:
                    e.animatePosition = true;
                    e.positionOffset = new Vector2(Random.Range(-70f, 70f), Random.Range(20f, 60f));
                    e.animateRotation = true;
                    e.rotationOffset = new Vector3(0f, 0f, Random.Range(-35f, 35f));
                    e.animateScale = true;
                    e.scaleFrom = Vector3.one * Random.Range(0.65f, 0.85f);
                    e.animateFade = true;
                    e.fadeFrom = 0f;
                    e.duration = RD(0.22f, 0.35f, durationMin, durationMax);
                    e.delay = Random.Range(0f, 0.05f);
                    e.ease = JuiceMath.GetPreset(JuiceMath.CurvePreset.EaseOutElastic);
                    e.useCurvePath = false;
                    e.curveAmplitude = 0f;
                    e.useShakeJitter = true;
                    e.shakeMagnitude = Random.Range(10f, 18f);
                    e.shakeRotationMagnitude = Random.Range(5f, 10f);
                    e.shakeFrequency = Random.Range(20f, 30f);
                    e.shakeSeed = Random.value * 1000f;
                    break;

                case UiJuiceAnimator.FeelingPreset.Slow:
                    e.animatePosition = true;
                    e.positionOffset = new Vector2(Random.Range(-20f, 20f), Random.Range(-60f, -20f));
                    e.animateRotation = false;
                    e.rotationOffset = Vector3.zero;
                    e.animateScale = true;
                    e.scaleFrom = Vector3.one * Random.Range(0.85f, 0.95f);
                    e.animateFade = true;
                    e.fadeFrom = 0f;
                    e.duration = RD(0.85f, 1.2f, durationMin, durationMax);
                    e.delay = Random.Range(0f, 0.12f);
                    e.ease = JuiceMath.DefaultEase;
                    break;

                case UiJuiceAnimator.FeelingPreset.Shaky:
                    e.animatePosition = true;
                    e.positionOffset = new Vector2(Random.Range(-14f, 14f), Random.Range(-14f, 14f));
                    e.animateRotation = true;
                    e.rotationOffset = new Vector3(0f, 0f, Random.Range(-12f, 12f));
                    e.animateScale = true;
                    e.scaleFrom = Vector3.one * Random.Range(0.97f, 1.03f);
                    e.animateFade = true;
                    e.fadeFrom = Random.Range(0f, 0.12f);
                    e.duration = RD(0.18f, 0.32f, durationMin, durationMax);
                    e.delay = Random.Range(0f, 0.04f);
                    e.ease = JuiceMath.GetPreset(JuiceMath.CurvePreset.EaseOutQuad);
                    e.useCurvePath = false;
                    e.curveAmplitude = 0f;
                    e.useShakeJitter = true;
                    e.shakeMagnitude = Random.Range(8f, 14f);
                    e.shakeRotationMagnitude = Random.Range(3f, 6f);
                    e.shakeFrequency = Random.Range(18f, 26f);
                    e.shakeSeed = Random.value * 1000f;
                    break;

                case UiJuiceAnimator.FeelingPreset.Calm:
                    e.animatePosition = true;
                    e.positionOffset = new Vector2(Random.Range(-15f, 15f), Random.Range(-45f, -15f));
                    e.animateRotation = false;
                    e.rotationOffset = Vector3.zero;
                    e.animateScale = true;
                    e.scaleFrom = Vector3.one * Random.Range(0.95f, 1.02f);
                    e.animateFade = true;
                    e.fadeFrom = 0.05f;
                    e.duration = RD(0.9f, 1.3f, durationMin, durationMax);
                    e.delay = Random.Range(0f, 0.12f);
                    e.ease = JuiceMath.DefaultEase;
                    break;

                case UiJuiceAnimator.FeelingPreset.Energetic:
                    e.animatePosition = true;
                    e.positionOffset = new Vector2(Random.Range(-80f, 80f), Random.Range(-100f, 20f));
                    e.animateRotation = true;
                    e.rotationOffset = new Vector3(0f, 0f, Random.Range(-40f, 40f));
                    e.animateScale = true;
                    e.scaleFrom = Vector3.one * Random.Range(0.85f, 0.95f);
                    e.animateFade = true;
                    e.fadeFrom = 0f;
                    e.duration = RD(0.32f, 0.55f, durationMin, durationMax);
                    e.delay = Random.Range(0f, 0.09f);
                    e.ease = JuiceMath.GetPreset(JuiceMath.CurvePreset.EaseOutElastic);
                    e.useCurvePath = true;
                    e.curveAmplitude = Random.Range(15f, 35f);
                    break;

                case UiJuiceAnimator.FeelingPreset.Bounce:
                    e.animatePosition = true;
                    e.positionOffset = new Vector2(0f, Random.Range(-130f, -90f));
                    e.animateRotation = false;
                    e.rotationOffset = Vector3.zero;
                    e.animateScale = true;
                    e.scaleFrom = Vector3.one * Random.Range(0.65f, 0.85f);
                    e.animateFade = true;
                    e.fadeFrom = 0f;
                    e.duration = RD(0.55f, 0.85f, durationMin, durationMax);
                    e.delay = Random.Range(0f, 0.1f);
                    e.ease = JuiceMath.GetPreset(JuiceMath.CurvePreset.EaseOutElastic);
                    e.useCurvePath = true;
                    e.curveAmplitude = Random.Range(22f, 38f);
                    break;

                case UiJuiceAnimator.FeelingPreset.Gentle:
                    e.animatePosition = true;
                    e.positionOffset = new Vector2(Random.Range(-25f, 25f), Random.Range(-55f, -25f));
                    e.animateRotation = false;
                    e.rotationOffset = Vector3.zero;
                    e.animateScale = true;
                    e.scaleFrom = Vector3.one * Random.Range(0.94f, 1.0f);
                    e.animateFade = true;
                    e.fadeFrom = 0f;
                    e.duration = RD(0.7f, 1.0f, durationMin, durationMax);
                    e.delay = Random.Range(0f, 0.12f);
                    e.ease = JuiceMath.GetPreset(JuiceMath.CurvePreset.EaseInOut);
                    break;

                case UiJuiceAnimator.FeelingPreset.Dramatic:
                    e.animatePosition = true;
                    e.positionOffset = new Vector2(Random.Range(-90f, 90f), Random.Range(40f, 120f));
                    e.animateRotation = true;
                    e.rotationOffset = new Vector3(0f, 0f, Random.Range(-60f, 60f));
                    e.animateScale = true;
                    e.scaleFrom = Vector3.one * Random.Range(0.7f, 0.9f);
                    e.animateFade = true;
                    e.fadeFrom = 0f;
                    e.duration = RD(0.45f, 0.7f, durationMin, durationMax);
                    e.delay = Random.Range(0f, 0.1f);
                    e.ease = JuiceMath.GetPreset(JuiceMath.CurvePreset.EaseOutExpo);
                    e.useCurvePath = true;
                    e.curveAmplitude = Random.Range(20f, 45f);
                    break;

                case UiJuiceAnimator.FeelingPreset.Snappy:
                    e.animatePosition = true;
                    e.positionOffset = new Vector2(Random.Range(-25f, 25f), Random.Range(-70f, -30f));
                    e.animateRotation = false;
                    e.rotationOffset = Vector3.zero;
                    e.animateScale = true;
                    e.scaleFrom = Vector3.one * Random.Range(0.9f, 0.98f);
                    e.animateFade = true;
                    e.fadeFrom = 0f;
                    e.duration = RD(0.28f, 0.42f, durationMin, durationMax);
                    e.delay = Random.Range(0f, 0.06f);
                    e.ease = JuiceMath.GetPreset(JuiceMath.CurvePreset.EaseOutQuad);
                    e.useCurvePath = false;
                    e.curveAmplitude = 0f;
                    break;

                case UiJuiceAnimator.FeelingPreset.Heavy:
                    e.animatePosition = true;
                    e.positionOffset = new Vector2(Random.Range(-40f, 40f), Random.Range(-140f, -80f));
                    e.animateRotation = false;
                    e.rotationOffset = Vector3.zero;
                    e.animateScale = true;
                    e.scaleFrom = Vector3.one * Random.Range(0.8f, 0.9f);
                    e.animateFade = true;
                    e.fadeFrom = 0f;
                    e.duration = RD(0.65f, 0.9f, durationMin, durationMax);
                    e.delay = Random.Range(0f, 0.1f);
                    e.ease = JuiceMath.GetPreset(JuiceMath.CurvePreset.EaseOutExpo);
                    e.useCurvePath = false;
                    e.curveAmplitude = 0f;
                    break;

                case UiJuiceAnimator.FeelingPreset.Pop:
                    e.animatePosition = true;
                    e.positionOffset = new Vector2(Random.Range(-15f, 15f), Random.Range(-85f, -45f));
                    e.animateRotation = false;
                    e.rotationOffset = Vector3.zero;
                    e.animateScale = true;
                    e.scaleFrom = Vector3.one * Random.Range(0.65f, 0.8f);
                    e.animateFade = true;
                    e.fadeFrom = 0f;
                    e.duration = RD(0.32f, 0.5f, durationMin, durationMax);
                    e.delay = Random.Range(0f, 0.08f);
                    e.ease = JuiceMath.GetPreset(JuiceMath.CurvePreset.EaseOutBack);
                    e.useCurvePath = false;
                    e.curveAmplitude = 0f;
                    break;

                case UiJuiceAnimator.FeelingPreset.Drift:
                    e.animatePosition = true;
                    e.positionOffset = new Vector2(Random.Range(-60f, 60f), Random.Range(40f, 90f));
                    e.animateRotation = true;
                    e.rotationOffset = new Vector3(0f, 0f, Random.Range(-25f, 25f));
                    e.animateScale = true;
                    e.scaleFrom = Vector3.one * Random.Range(0.95f, 1.05f);
                    e.animateFade = true;
                    e.fadeFrom = 0f;
                    e.duration = RD(0.7f, 1.1f, durationMin, durationMax);
                    e.delay = Random.Range(0f, 0.12f);
                    e.ease = JuiceMath.GetPreset(JuiceMath.CurvePreset.EaseInOut);
                    e.useCurvePath = true;
                    e.curveAmplitude = Random.Range(20f, 40f);
                    break;

                case UiJuiceAnimator.FeelingPreset.Punchy:
                    e.animatePosition = true;
                    e.positionOffset = new Vector2(Random.Range(-50f, 50f), Random.Range(-110f, -60f));
                    e.animateRotation = true;
                    e.rotationOffset = new Vector3(0f, 0f, Random.Range(-35f, 35f));
                    e.animateScale = true;
                    e.scaleFrom = Vector3.one * Random.Range(0.8f, 0.92f);
                    e.animateFade = true;
                    e.fadeFrom = 0f;
                    e.duration = RD(0.35f, 0.55f, durationMin, durationMax);
                    e.delay = Random.Range(0f, 0.08f);
                    e.ease = JuiceMath.GetPreset(JuiceMath.CurvePreset.EaseOutElastic);
                    e.useCurvePath = false;
                    e.curveAmplitude = 0f;
                    break;

                case UiJuiceAnimator.FeelingPreset.Rubber:
                    e.animatePosition = true;
                    e.positionOffset = new Vector2(Random.Range(-20f, 20f), Random.Range(-90f, -40f));
                    e.animateRotation = true;
                    e.rotationOffset = new Vector3(0f, 0f, Random.Range(-20f, 20f));
                    e.animateScale = true;
                    e.scaleFrom = Vector3.one * Random.Range(0.75f, 0.9f);
                    e.animateFade = true;
                    e.fadeFrom = 0f;
                    e.duration = RD(0.4f, 0.65f, durationMin, durationMax);
                    e.delay = Random.Range(0f, 0.1f);
                    e.ease = JuiceMath.GetPreset(JuiceMath.CurvePreset.EaseOutElastic);
                    e.useCurvePath = true;
                    e.curveAmplitude = Random.Range(10f, 25f);
                    break;

                case UiJuiceAnimator.FeelingPreset.SoftRise:
                    e.animatePosition = true;
                    e.positionOffset = new Vector2(Random.Range(-20f, 20f), Random.Range(-80f, -50f));
                    e.animateRotation = false;
                    e.rotationOffset = Vector3.zero;
                    e.animateScale = true;
                    e.scaleFrom = Vector3.one * Random.Range(0.9f, 0.98f);
                    e.animateFade = true;
                    e.fadeFrom = 0f;
                    e.duration = RD(0.6f, 0.9f, durationMin, durationMax);
                    e.delay = Random.Range(0f, 0.1f);
                    e.ease = JuiceMath.GetPreset(JuiceMath.CurvePreset.EaseOutQuad);
                    e.useCurvePath = true;
                    e.curveAmplitude = Random.Range(8f, 18f);
                    break;

                case UiJuiceAnimator.FeelingPreset.Flashy:
                    e.animatePosition = true;
                    e.positionOffset = new Vector2(Random.Range(-25f, 25f), Random.Range(-70f, -30f));
                    e.animateRotation = true;
                    e.rotationOffset = new Vector3(0f, 0f, Random.Range(-30f, 30f));
                    e.animateScale = true;
                    e.scaleFrom = Vector3.one * Random.Range(0.6f, 0.85f);
                    e.animateFade = true;
                    e.fadeFrom = Random.Range(0f, 0.2f);
                    e.animateColor = true;
                    e.colorFrom = new Color(1.1f, 1.0f, 0.6f, 0.55f);
                    e.duration = RD(0.28f, 0.48f, durationMin, durationMax);
                    e.delay = Random.Range(0f, 0.06f);
                    e.ease = JuiceMath.GetPreset(JuiceMath.CurvePreset.EaseOutBack);
                    e.useCurvePath = false;
                    e.curveAmplitude = 0f;
                    break;

                case UiJuiceAnimator.FeelingPreset.SlideLeft:
                    e.animatePosition = true;
                    e.positionOffset = new Vector2(Random.Range(140f, 220f), Random.Range(-30f, 30f));
                    e.animateRotation = false;
                    e.rotationOffset = Vector3.zero;
                    e.animateScale = false;
                    e.scaleFrom = Vector3.one;
                    e.animateFade = true;
                    e.fadeFrom = Random.Range(0f, 0.3f);
                    e.duration = RD(0.45f, 0.8f, durationMin, durationMax);
                    e.delay = Random.Range(0f, 0.1f);
                    e.ease = JuiceMath.DefaultEase;
                    break;

                case UiJuiceAnimator.FeelingPreset.SlideRight:
                    e.animatePosition = true;
                    e.positionOffset = new Vector2(Random.Range(-220f, -140f), Random.Range(-30f, 30f));
                    e.animateRotation = false;
                    e.rotationOffset = Vector3.zero;
                    e.animateScale = false;
                    e.scaleFrom = Vector3.one;
                    e.animateFade = true;
                    e.fadeFrom = Random.Range(0f, 0.3f);
                    e.duration = RD(0.45f, 0.8f, durationMin, durationMax);
                    e.delay = Random.Range(0f, 0.1f);
                    e.ease = JuiceMath.DefaultEase;
                    break;

                case UiJuiceAnimator.FeelingPreset.Flip:
                    e.animatePosition = true;
                    e.positionOffset = new Vector2(Random.Range(-40f, 40f), Random.Range(-90f, -40f));
                    e.animateRotation = true;
                    e.rotationOffset = new Vector3(0f, 0f, Random.Range(-120f, 120f));
                    e.animateScale = true;
                    e.scaleFrom = Vector3.one * Random.Range(0.8f, 0.95f);
                    e.animateFade = true;
                    e.fadeFrom = 0f;
                    e.duration = RD(0.5f, 0.8f, durationMin, durationMax);
                    e.delay = Random.Range(0f, 0.1f);
                    e.ease = JuiceMath.GetPreset(JuiceMath.CurvePreset.EaseOutElastic);
                    break;

                case UiJuiceAnimator.FeelingPreset.ZoomPop:
                    e.animatePosition = true;
                    e.positionOffset = new Vector2(Random.Range(-20f, 20f), Random.Range(-120f, -80f));
                    e.animateRotation = false;
                    e.rotationOffset = Vector3.zero;
                    e.animateScale = true;
                    e.scaleFrom = Vector3.one * Random.Range(0.55f, 0.75f);
                    e.animateFade = true;
                    e.fadeFrom = 0f;
                    e.duration = RD(0.35f, 0.55f, durationMin, durationMax);
                    e.delay = Random.Range(0f, 0.08f);
                    e.ease = JuiceMath.GetPreset(JuiceMath.CurvePreset.EaseOutBack);
                    e.useCurvePath = false;
                    e.curveAmplitude = 0f;
                    break;

                case UiJuiceAnimator.FeelingPreset.Wobble:
                    e.animatePosition = true;
                    e.positionOffset = new Vector2(Random.Range(-35f, 35f), Random.Range(-50f, 10f));
                    e.animateRotation = true;
                    e.rotationOffset = new Vector3(0f, 0f, Random.Range(-70f, 70f));
                    e.animateScale = true;
                    e.scaleFrom = Vector3.one * Random.Range(0.85f, 1.05f);
                    e.animateFade = true;
                    e.fadeFrom = Random.Range(0f, 0.25f);
                    e.duration = RD(0.45f, 0.75f, durationMin, durationMax);
                    e.delay = Random.Range(0f, 0.08f);
                    e.ease = JuiceMath.GetPreset(JuiceMath.CurvePreset.EaseOutElastic);
                    e.useCurvePath = true;
                    e.curveAmplitude = Random.Range(10f, 18f);
                    break;

                case UiJuiceAnimator.FeelingPreset.Ripple:
                    e.animatePosition = true;
                    e.positionOffset = new Vector2(Random.Range(-18f, 18f), Random.Range(-35f, -10f));
                    e.animateRotation = false;
                    e.rotationOffset = Vector3.zero;
                    e.animateScale = true;
                    e.scaleFrom = Vector3.one * Random.Range(0.85f, 0.95f);
                    e.animateFade = true;
                    e.fadeFrom = Random.Range(0.15f, 0.35f);
                    e.duration = RD(0.55f, 0.85f, durationMin, durationMax);
                    e.delay = Random.Range(0f, 0.12f);
                    e.ease = JuiceMath.GetPreset(JuiceMath.CurvePreset.EaseInOut);
                    e.useCurvePath = true;
                    e.curveAmplitude = Random.Range(6f, 14f);
                    break;

                case UiJuiceAnimator.FeelingPreset.Stomp:
                    e.animatePosition = true;
                    e.positionOffset = new Vector2(Random.Range(-25f, 25f), Random.Range(-180f, -120f));
                    e.animateRotation = false;
                    e.rotationOffset = Vector3.zero;
                    e.animateScale = true;
                    e.scaleFrom = new Vector3(Random.Range(0.78f, 0.88f), Random.Range(0.78f, 0.88f), 1f);
                    e.animateFade = true;
                    e.fadeFrom = Random.Range(0f, 0.15f);
                    e.duration = RD(0.28f, 0.48f, durationMin, durationMax);
                    e.delay = Random.Range(0f, 0.06f);
                    e.ease = JuiceMath.GetPreset(JuiceMath.CurvePreset.EaseOutBack);
                    e.useCurvePath = false;
                    e.curveAmplitude = 0f;
                    break;

                case UiJuiceAnimator.FeelingPreset.FloatUp:
                    e.animatePosition = true;
                    e.positionOffset = new Vector2(Random.Range(-25f, 25f), Random.Range(-110f, -70f));
                    e.animateRotation = true;
                    e.rotationOffset = new Vector3(0f, 0f, Random.Range(-20f, 20f));
                    e.animateScale = true;
                    e.scaleFrom = Vector3.one * Random.Range(0.92f, 1.0f);
                    e.animateFade = true;
                    e.fadeFrom = Random.Range(0f, 0.2f);
                    e.duration = RD(0.8f, 1.2f, durationMin, durationMax);
                    e.delay = Random.Range(0f, 0.12f);
                    e.ease = JuiceMath.GetPreset(JuiceMath.CurvePreset.EaseOutQuad);
                    e.useCurvePath = true;
                    e.curveAmplitude = Random.Range(12f, 24f);
                    break;

                case UiJuiceAnimator.FeelingPreset.SwoopIn:
                    e.animatePosition = true;
                    e.positionOffset = new Vector2(Random.Range(-160f, -90f), Random.Range(80f, 140f));
                    e.animateRotation = true;
                    e.rotationOffset = new Vector3(0f, 0f, Random.Range(-45f, 45f));
                    e.animateScale = true;
                    e.scaleFrom = Vector3.one * Random.Range(0.85f, 0.95f);
                    e.animateFade = true;
                    e.fadeFrom = 0f;
                    e.duration = RD(0.42f, 0.65f, durationMin, durationMax);
                    e.delay = Random.Range(0f, 0.08f);
                    e.ease = JuiceMath.GetPreset(JuiceMath.CurvePreset.EaseOutExpo);
                    e.useCurvePath = true;
                    e.curveAmplitude = Random.Range(18f, 32f);
                    break;

                case UiJuiceAnimator.FeelingPreset.Blink:
                    e.animatePosition = false;
                    e.positionOffset = Vector2.zero;
                    e.animateRotation = false;
                    e.rotationOffset = Vector3.zero;
                    e.animateScale = true;
                    e.scaleFrom = Vector3.one * Random.Range(0.98f, 1.02f);
                    e.animateFade = true;
                    e.fadeFrom = 0f;
                    e.duration = RD(0.12f, 0.24f, durationMin, durationMax);
                    e.delay = Random.Range(0f, 0.05f);
                    e.ease = JuiceMath.GetPreset(JuiceMath.CurvePreset.EaseOutQuad);
                    e.useCurvePath = false;
                    e.curveAmplitude = 0f;
                    break;

                case UiJuiceAnimator.FeelingPreset.Cascade:
                    e.animatePosition = true;
                    e.positionOffset = new Vector2(Random.Range(-30f, 30f), Random.Range(-80f, -40f));
                    e.animateRotation = false;
                    e.rotationOffset = Vector3.zero;
                    e.animateScale = true;
                    e.scaleFrom = Vector3.one * Random.Range(0.9f, 0.98f);
                    e.animateFade = true;
                    e.fadeFrom = Random.Range(0f, 0.15f);
                    e.duration = RD(0.6f, 0.95f, durationMin, durationMax);
                    e.delay = Random.Range(0.02f, 0.14f);
                    e.ease = JuiceMath.GetPreset(JuiceMath.CurvePreset.EaseInOut);
                    e.useCurvePath = true;
                    e.curveAmplitude = Random.Range(6f, 14f);
                    break;

                case UiJuiceAnimator.FeelingPreset.Jitter:
                    e.animatePosition = true;
                    e.positionOffset = new Vector2(Random.Range(-22f, 22f), Random.Range(-22f, 22f));
                    e.animateRotation = false;
                    e.rotationOffset = Vector3.zero;
                    e.animateScale = false;
                    e.scaleFrom = Vector3.one;
                    e.animateFade = true;
                    e.fadeFrom = Random.Range(0.1f, 0.3f);
                    e.duration = RD(0.14f, 0.26f, durationMin, durationMax);
                    e.delay = Random.Range(0f, 0.04f);
                    e.ease = JuiceMath.GetPreset(JuiceMath.CurvePreset.EaseOutQuad);
                    e.useCurvePath = false;
                    e.curveAmplitude = 0f;
                    e.useShakeJitter = true;
                    e.shakeMagnitude = Random.Range(4f, 9f);
                    e.shakeRotationMagnitude = Random.Range(0f, 2f);
                    e.shakeFrequency = Random.Range(16f, 24f);
                    e.shakeSeed = Random.value * 1000f;
                    break;

                case UiJuiceAnimator.FeelingPreset.PulseGlow:
                    e.animatePosition = true;
                    e.positionOffset = new Vector2(Random.Range(-8f, 8f), Random.Range(-25f, -5f));
                    e.animateRotation = false;
                    e.rotationOffset = Vector3.zero;
                    e.animateScale = true;
                    e.scaleFrom = Vector3.one * Random.Range(0.6f, 0.8f);
                    e.animateFade = true;
                    e.fadeFrom = Random.Range(0f, 0.1f);
                    e.animateColor = true;
                    e.colorFrom = new Color(0.8f, 1.15f, 1.25f, 1f);
                    e.duration = RD(0.28f, 0.5f, durationMin, durationMax);
                    e.delay = Random.Range(0f, 0.06f);
                    e.ease = JuiceMath.GetPreset(JuiceMath.CurvePreset.EaseOutBack);
                    e.useCurvePath = false;
                    e.curveAmplitude = 0f;
                    break;

                case UiJuiceAnimator.FeelingPreset.TiltDrop:
                    e.animatePosition = true;
                    e.positionOffset = new Vector2(Random.Range(-30f, 30f), Random.Range(120f, 180f));
                    e.animateRotation = true;
                    e.rotationOffset = new Vector3(0f, 0f, Random.Range(-65f, 65f));
                    e.animateScale = true;
                    e.scaleFrom = Vector3.one * Random.Range(0.85f, 0.95f);
                    e.animateFade = true;
                    e.fadeFrom = Random.Range(0f, 0.25f);
                    e.duration = RD(0.38f, 0.6f, durationMin, durationMax);
                    e.delay = Random.Range(0f, 0.08f);
                    e.ease = JuiceMath.GetPreset(JuiceMath.CurvePreset.EaseOutExpo);
                    e.useCurvePath = true;
                    e.curveAmplitude = Random.Range(12f, 24f);
                    break;

                case UiJuiceAnimator.FeelingPreset.SnapLeft:
                    e.animatePosition = true;
                    e.positionOffset = new Vector2(Random.Range(140f, 200f), Random.Range(-20f, 20f));
                    e.animateRotation = true;
                    e.rotationOffset = new Vector3(0f, 0f, Random.Range(-18f, 18f));
                    e.animateScale = false;
                    e.scaleFrom = Vector3.one;
                    e.animateFade = true;
                    e.fadeFrom = Random.Range(0f, 0.2f);
                    e.duration = RD(0.28f, 0.48f, durationMin, durationMax);
                    e.delay = Random.Range(0f, 0.06f);
                    e.ease = JuiceMath.GetPreset(JuiceMath.CurvePreset.EaseOutQuad);
                    break;

                case UiJuiceAnimator.FeelingPreset.SnapRight:
                    e.animatePosition = true;
                    e.positionOffset = new Vector2(Random.Range(-200f, -140f), Random.Range(-20f, 20f));
                    e.animateRotation = true;
                    e.rotationOffset = new Vector3(0f, 0f, Random.Range(-18f, 18f));
                    e.animateScale = false;
                    e.scaleFrom = Vector3.one;
                    e.animateFade = true;
                    e.fadeFrom = Random.Range(0f, 0.2f);
                    e.duration = RD(0.28f, 0.48f, durationMin, durationMax);
                    e.delay = Random.Range(0f, 0.06f);
                    e.ease = JuiceMath.GetPreset(JuiceMath.CurvePreset.EaseOutQuad);
                    break;

                case UiJuiceAnimator.FeelingPreset.BounceInPlace:
                    e.animatePosition = true;
                    e.positionOffset = new Vector2(Random.Range(-10f, 10f), Random.Range(-35f, -15f));
                    e.animateRotation = false;
                    e.rotationOffset = Vector3.zero;
                    e.animateScale = true;
                    e.scaleFrom = Vector3.one * Random.Range(0.65f, 0.8f);
                    e.animateFade = true;
                    e.fadeFrom = Random.Range(0f, 0.1f);
                    e.duration = RD(0.35f, 0.55f, durationMin, durationMax);
                    e.delay = Random.Range(0f, 0.06f);
                    e.ease = JuiceMath.GetPreset(JuiceMath.CurvePreset.EaseOutBack);
                    e.useCurvePath = false;
                    e.curveAmplitude = 0f;
                    break;

                case UiJuiceAnimator.FeelingPreset.DriftSlow:
                    e.animatePosition = true;
                    e.positionOffset = new Vector2(Random.Range(-70f, 70f), Random.Range(60f, 120f));
                    e.animateRotation = true;
                    e.rotationOffset = new Vector3(0f, 0f, Random.Range(-25f, 25f));
                    e.animateScale = true;
                    e.scaleFrom = Vector3.one * Random.Range(0.96f, 1.06f);
                    e.animateFade = true;
                    e.fadeFrom = Random.Range(0.1f, 0.35f);
                    e.duration = RD(1.0f, 1.5f, durationMin, durationMax);
                    e.delay = Random.Range(0f, 0.12f);
                    e.ease = JuiceMath.GetPreset(JuiceMath.CurvePreset.EaseInOut);
                    e.useCurvePath = true;
                    e.curveAmplitude = Random.Range(20f, 35f);
                    break;

                case UiJuiceAnimator.FeelingPreset.PopRotate:
                    e.animatePosition = true;
                    e.positionOffset = new Vector2(Random.Range(-20f, 20f), Random.Range(-75f, -45f));
                    e.animateRotation = true;
                    e.rotationOffset = new Vector3(0f, 0f, Random.Range(90f, 180f));
                    e.animateScale = true;
                    e.scaleFrom = Vector3.one * Random.Range(0.55f, 0.75f);
                    e.animateFade = true;
                    e.fadeFrom = 0f;
                    e.duration = RD(0.35f, 0.55f, durationMin, durationMax);
                    e.delay = Random.Range(0f, 0.08f);
                    e.ease = JuiceMath.GetPreset(JuiceMath.CurvePreset.EaseOutBack);
                    e.useCurvePath = false;
                    e.curveAmplitude = 0f;
                    break;

                case UiJuiceAnimator.FeelingPreset.Whip:
                    e.animatePosition = true;
                    e.positionOffset = new Vector2(Random.Range(-160f, 160f), Random.Range(-40f, 40f));
                    e.animateRotation = true;
                    e.rotationOffset = new Vector3(0f, 0f, Random.Range(-55f, 55f));
                    e.animateScale = true;
                    e.scaleFrom = Vector3.one * Random.Range(0.85f, 0.95f);
                    e.animateFade = true;
                    e.fadeFrom = Random.Range(0f, 0.25f);
                    e.duration = RD(0.28f, 0.48f, durationMin, durationMax);
                    e.delay = Random.Range(0f, 0.06f);
                    e.ease = JuiceMath.GetPreset(JuiceMath.CurvePreset.EaseOutExpo);
                    e.useCurvePath = true;
                    e.curveAmplitude = Random.Range(10f, 22f);
                    break;

                case UiJuiceAnimator.FeelingPreset.Wave:
                    e.animatePosition = true;
                    e.positionOffset = new Vector2(Random.Range(-60f, 60f), Random.Range(-60f, 0f));
                    e.animateRotation = false;
                    e.rotationOffset = Vector3.zero;
                    e.animateScale = true;
                    e.scaleFrom = Vector3.one * Random.Range(0.96f, 1.04f);
                    e.animateFade = true;
                    e.fadeFrom = Random.Range(0.05f, 0.25f);
                    e.duration = RD(0.6f, 0.9f, durationMin, durationMax);
                    e.delay = Random.Range(0f, 0.12f);
                    e.ease = JuiceMath.GetPreset(JuiceMath.CurvePreset.EaseInOut);
                    e.useCurvePath = true;
                    e.curveAmplitude = Random.Range(12f, 24f);
                    break;

                case UiJuiceAnimator.FeelingPreset.Breath:
                    e.animatePosition = true;
                    e.positionOffset = new Vector2(Random.Range(-10f, 10f), Random.Range(-20f, -5f));
                    e.animateRotation = false;
                    e.rotationOffset = Vector3.zero;
                    e.animateScale = true;
                    e.scaleFrom = Vector3.one * Random.Range(0.94f, 0.99f);
                    e.animateFade = false;
                    e.fadeFrom = 0f;
                    e.duration = RD(0.9f, 1.4f, durationMin, durationMax);
                    e.delay = Random.Range(0f, 0.1f);
                    e.ease = JuiceMath.DefaultEase;
                    e.useCurvePath = true;
                    e.curveAmplitude = Random.Range(6f, 12f);
                    break;

                case UiJuiceAnimator.FeelingPreset.FlashSlideDown:
                    e.animatePosition = true;
                    e.positionOffset = new Vector2(Random.Range(-15f, 15f), Random.Range(140f, 220f));
                    e.animateRotation = false;
                    e.rotationOffset = Vector3.zero;
                    e.animateScale = false;
                    e.scaleFrom = Vector3.one;
                    e.animateFade = true;
                    e.fadeFrom = 0f;
                    e.animateColor = true;
                    e.colorFrom = new Color(1.05f, 1.05f, 1.15f, 0.9f);
                    e.duration = RD(0.28f, 0.48f, durationMin, durationMax);
                    e.delay = Random.Range(0f, 0.06f);
                    e.ease = JuiceMath.GetPreset(JuiceMath.CurvePreset.EaseOutQuad);
                    e.useCurvePath = false;
                    e.curveAmplitude = 0f;
                    break;

                case UiJuiceAnimator.FeelingPreset.SlideOutLeft:
                    e.animatePosition = true;
                    e.positionOffset = new Vector2(Random.Range(-240f, -160f), Random.Range(-20f, 20f));
                    e.animateRotation = false;
                    e.rotationOffset = Vector3.zero;
                    e.animateScale = false;
                    e.scaleFrom = Vector3.one;
                    e.animateFade = true;
                    e.fadeFrom = 0f;
                    e.duration = RD(0.32f, 0.55f, durationMin, durationMax);
                    e.delay = Random.Range(0f, 0.08f);
                    e.ease = JuiceMath.GetPreset(JuiceMath.CurvePreset.EaseOutQuad);
                    e.useCurvePath = false;
                    e.curveAmplitude = 0f;
                    break;

                case UiJuiceAnimator.FeelingPreset.SlideOutRight:
                    e.animatePosition = true;
                    e.positionOffset = new Vector2(Random.Range(160f, 240f), Random.Range(-20f, 20f));
                    e.animateRotation = false;
                    e.rotationOffset = Vector3.zero;
                    e.animateScale = false;
                    e.scaleFrom = Vector3.one;
                    e.animateFade = true;
                    e.fadeFrom = 0f;
                    e.duration = RD(0.32f, 0.55f, durationMin, durationMax);
                    e.delay = Random.Range(0f, 0.08f);
                    e.ease = JuiceMath.GetPreset(JuiceMath.CurvePreset.EaseOutQuad);
                    e.useCurvePath = false;
                    e.curveAmplitude = 0f;
                    break;

                case UiJuiceAnimator.FeelingPreset.FadeOut:
                    e.animatePosition = false;
                    e.positionOffset = Vector2.zero;
                    e.animateRotation = false;
                    e.rotationOffset = Vector3.zero;
                    e.animateScale = true;
                    e.scaleFrom = Vector3.one * Random.Range(0.78f, 0.9f);
                    e.animateFade = true;
                    e.fadeFrom = 0f;
                    e.duration = RD(0.28f, 0.42f, durationMin, durationMax);
                    e.delay = Random.Range(0f, 0.05f);
                    e.ease = JuiceMath.GetPreset(JuiceMath.CurvePreset.EaseOutQuad);
                    e.useCurvePath = false;
                    e.curveAmplitude = 0f;
                    break;

                case UiJuiceAnimator.FeelingPreset.FlipOut:
                    e.animatePosition = true;
                    e.positionOffset = new Vector2(0f, Random.Range(-50f, 20f));
                    e.animateRotation = true;
                    e.rotationOffset = new Vector3(0f, 0f, Random.Range(160f, 240f));
                    e.animateScale = true;
                    e.scaleFrom = Vector3.one * Random.Range(0.78f, 0.9f);
                    e.animateFade = true;
                    e.fadeFrom = 0f;
                    e.duration = RD(0.35f, 0.55f, durationMin, durationMax);
                    e.delay = Random.Range(0f, 0.08f);
                    e.ease = JuiceMath.GetPreset(JuiceMath.CurvePreset.EaseOutExpo);
                    e.useCurvePath = false;
                    e.curveAmplitude = 0f;
                    break;

                case UiJuiceAnimator.FeelingPreset.PopFlash:
                    e.animatePosition = true;
                    e.positionOffset = new Vector2(Random.Range(-12f, 12f), Random.Range(-45f, -20f));
                    e.animateRotation = false;
                    e.rotationOffset = Vector3.zero;
                    e.animateScale = true;
                    e.scaleFrom = Vector3.one * Random.Range(0.55f, 0.72f);
                    e.animateFade = true;
                    e.fadeFrom = Random.Range(0f, 0.1f);
                    e.animateColor = true;
                    e.colorFrom = new Color(1f, 0.86f, 0.35f, 0.45f);
                    e.duration = RD(0.22f, 0.35f, durationMin, durationMax);
                    e.delay = Random.Range(0f, 0.06f);
                    e.ease = JuiceMath.GetPreset(JuiceMath.CurvePreset.EaseOutBack);
                    e.useCurvePath = false;
                    e.curveAmplitude = 0f;
                    break;

                case UiJuiceAnimator.FeelingPreset.GlowPulse:
                    e.animatePosition = false;
                    e.positionOffset = Vector2.zero;
                    e.animateRotation = false;
                    e.rotationOffset = Vector3.zero;
                    e.animateScale = true;
                    e.scaleFrom = Vector3.one * Random.Range(0.92f, 0.98f);
                    e.animateFade = true;
                    e.fadeFrom = Random.Range(0.08f, 0.16f);
                    e.animateColor = true;
                    e.colorFrom = new Color(1.1f, 1.1f, 1.25f, 1f);
                    e.duration = RD(0.85f, 1.2f, durationMin, durationMax);
                    e.delay = Random.Range(0f, 0.08f);
                    e.ease = JuiceMath.DefaultEase;
                    e.useCurvePath = true;
                    e.curveAmplitude = Random.Range(6f, 12f);
                    break;

                case UiJuiceAnimator.FeelingPreset.RippleMask:
                    e.animatePosition = true;
                    e.positionOffset = new Vector2(0f, Random.Range(-25f, 0f));
                    e.animateRotation = false;
                    e.rotationOffset = Vector3.zero;
                    e.animateScale = true;
                    e.scaleFrom = new Vector3(Random.Range(0.25f, 0.35f), Random.Range(0.25f, 0.35f), 1f);
                    e.animateFade = true;
                    e.fadeFrom = Random.Range(0.1f, 0.2f);
                    e.animateColor = false;
                    e.duration = RD(0.65f, 0.95f, durationMin, durationMax);
                    e.delay = Random.Range(0f, 0.08f);
                    e.ease = JuiceMath.GetPreset(JuiceMath.CurvePreset.EaseOutQuad);
                    e.useCurvePath = false;
                    e.curveAmplitude = 0f;
                    break;

                case UiJuiceAnimator.FeelingPreset.SpiralIn:
                    e.animatePosition = true;
                    e.positionOffset = new Vector2(Random.Range(-120f, 120f), Random.Range(-120f, -40f));
                    e.animateRotation = true;
                    e.rotationOffset = new Vector3(0f, 0f, Random.Range(220f, 320f));
                    e.animateScale = true;
                    e.scaleFrom = Vector3.one * Random.Range(0.65f, 0.85f);
                    e.animateFade = true;
                    e.fadeFrom = Random.Range(0f, 0.2f);
                    e.duration = RD(0.55f, 0.85f, durationMin, durationMax);
                    e.delay = Random.Range(0f, 0.1f);
                    e.ease = JuiceMath.GetPreset(JuiceMath.CurvePreset.EaseOutBack);
                    e.useCurvePath = true;
                    e.curveAmplitude = Random.Range(24f, 42f);
                    break;

                case UiJuiceAnimator.FeelingPreset.GlitchPop:
                    e.animatePosition = true;
                    e.positionOffset = new Vector2(Random.Range(-18f, 18f), Random.Range(-18f, 18f));
                    e.animateRotation = true;
                    e.rotationOffset = new Vector3(0f, 0f, Random.Range(-25f, 25f));
                    e.animateScale = true;
                    e.scaleFrom = Vector3.one * Random.Range(0.6f, 0.78f);
                    e.animateFade = true;
                    e.fadeFrom = Random.Range(0.15f, 0.3f);
                    e.animateColor = false;
                    e.duration = RD(0.16f, 0.26f, durationMin, durationMax);
                    e.delay = Random.Range(0f, 0.04f);
                    e.ease = JuiceMath.GetPreset(JuiceMath.CurvePreset.EaseOutQuad);
                    e.useCurvePath = false;
                    e.curveAmplitude = 0f;
                    e.useShakeJitter = true;
                    e.shakeMagnitude = Random.Range(12f, 18f);
                    e.shakeRotationMagnitude = Random.Range(4f, 10f);
                    e.shakeFrequency = Random.Range(28f, 42f);
                    e.shakeSeed = Random.value * 1000f;
                    break;

                case UiJuiceAnimator.FeelingPreset.BreathingIdle:
                    e.animatePosition = true;
                    e.positionOffset = new Vector2(0f, Random.Range(-12f, 12f));
                    e.animateRotation = false;
                    e.rotationOffset = Vector3.zero;
                    e.animateScale = true;
                    e.scaleFrom = Vector3.one * Random.Range(0.94f, 0.98f);
                    e.animateFade = false;
                    e.fadeFrom = 0f;
                    e.animateColor = false;
                    e.duration = RD(1.4f, 1.9f, durationMin, durationMax);
                    e.delay = Random.Range(0f, 0.08f);
                    e.ease = JuiceMath.DefaultEase;
                    e.useCurvePath = true;
                    e.curveAmplitude = Random.Range(6f, 12f);
                    break;

                case UiJuiceAnimator.FeelingPreset.Darker:
                    e.animatePosition = false;
                    e.positionOffset = Vector2.zero;
                    e.animateRotation = false;
                    e.rotationOffset = Vector3.zero;
                    e.animateScale = true;
                    e.scaleFrom = Vector3.one * Random.Range(0.92f, 0.98f);
                    e.animateFade = true;
                    e.fadeFrom = Random.Range(0.05f, 0.12f);
                    e.animateColor = true;
                    e.colorFrom = new Color(0.05f, 0.05f, 0.05f, 1f);
                    e.duration = RD(0.32f, 0.55f, durationMin, durationMax);
                    e.delay = Random.Range(0f, 0.06f);
                    e.ease = JuiceMath.GetPreset(JuiceMath.CurvePreset.EaseInOut);
                    e.useCurvePath = false;
                    e.curveAmplitude = 0f;
                    break;

                case UiJuiceAnimator.FeelingPreset.RainbowFlash:
                    e.animatePosition = true;
                    e.positionOffset = new Vector2(Random.Range(-18f, 18f), Random.Range(-30f, 10f));
                    e.animateRotation = false;
                    e.rotationOffset = Vector3.zero;
                    e.animateScale = true;
                    e.scaleFrom = Vector3.one * Random.Range(0.8f, 0.92f);
                    e.animateFade = true;
                    e.fadeFrom = Random.Range(0f, 0.2f);
                    e.animateColor = true;
                    var hsv = Random.ColorHSV(0f, 1f, 0.8f, 1f, 0.8f, 1f, 1f, 1f);
                    e.colorFrom = hsv;
                    e.duration = RD(0.26f, 0.42f, durationMin, durationMax);
                    e.delay = Random.Range(0f, 0.05f);
                    e.ease = JuiceMath.GetPreset(JuiceMath.CurvePreset.EaseOutElastic);
                    e.useCurvePath = false;
                    e.curveAmplitude = 0f;
                    break;

                case UiJuiceAnimator.FeelingPreset.WarmFlash:
                    e.animatePosition = true;
                    e.positionOffset = new Vector2(Random.Range(-14f, 14f), Random.Range(-40f, -10f));
                    e.animateRotation = false;
                    e.rotationOffset = Vector3.zero;
                    e.animateScale = true;
                    e.scaleFrom = Vector3.one * Random.Range(0.68f, 0.82f);
                    e.animateFade = true;
                    e.fadeFrom = Random.Range(0f, 0.12f);
                    e.animateColor = true;
                    e.colorFrom = new Color(1.08f, 0.74f, 0.42f, 0.85f);
                    e.duration = RD(0.24f, 0.4f, durationMin, durationMax);
                    e.delay = Random.Range(0f, 0.05f);
                    e.ease = JuiceMath.GetPreset(JuiceMath.CurvePreset.EaseOutBack);
                    e.useCurvePath = false;
                    e.curveAmplitude = 0f;
                    break;

                case UiJuiceAnimator.FeelingPreset.CoolFlash:
                    e.animatePosition = true;
                    e.positionOffset = new Vector2(Random.Range(-14f, 14f), Random.Range(-35f, -5f));
                    e.animateRotation = false;
                    e.rotationOffset = Vector3.zero;
                    e.animateScale = true;
                    e.scaleFrom = Vector3.one * Random.Range(0.7f, 0.86f);
                    e.animateFade = true;
                    e.fadeFrom = Random.Range(0f, 0.14f);
                    e.animateColor = true;
                    e.colorFrom = new Color(0.55f, 0.9f, 1.15f, 0.85f);
                    e.duration = RD(0.24f, 0.4f, durationMin, durationMax);
                    e.delay = Random.Range(0f, 0.05f);
                    e.ease = JuiceMath.GetPreset(JuiceMath.CurvePreset.EaseOutBack);
                    e.useCurvePath = false;
                    e.curveAmplitude = 0f;
                    break;

                case UiJuiceAnimator.FeelingPreset.Desaturate:
                    e.animatePosition = false;
                    e.positionOffset = Vector2.zero;
                    e.animateRotation = false;
                    e.rotationOffset = Vector3.zero;
                    e.animateScale = true;
                    e.scaleFrom = Vector3.one * Random.Range(0.94f, 1.0f);
                    e.animateFade = true;
                    e.fadeFrom = Random.Range(0.05f, 0.12f);
                    e.animateColor = true;
                    e.colorFrom = new Color(0.35f, 0.35f, 0.35f, 1f);
                    e.duration = RD(0.55f, 0.9f, durationMin, durationMax);
                    e.delay = Random.Range(0f, 0.08f);
                    e.ease = JuiceMath.DefaultEase;
                    e.useCurvePath = false;
                    e.curveAmplitude = 0f;
                    break;

                case UiJuiceAnimator.FeelingPreset.NeonPulse:
                    e.animatePosition = true;
                    e.positionOffset = new Vector2(Random.Range(-10f, 10f), Random.Range(-20f, 0f));
                    e.animateRotation = false;
                    e.rotationOffset = Vector3.zero;
                    e.animateScale = true;
                    e.scaleFrom = Vector3.one * Random.Range(0.8f, 0.92f);
                    e.animateFade = true;
                    e.fadeFrom = Random.Range(0.08f, 0.2f);
                    e.animateColor = true;
                    e.colorFrom = new Color(1.2f, 0.2f, 1.1f, 1f);
                    e.duration = RD(0.45f, 0.75f, durationMin, durationMax);
                    e.delay = Random.Range(0f, 0.08f);
                    e.ease = JuiceMath.GetPreset(JuiceMath.CurvePreset.EaseOutElastic);
                    e.useCurvePath = true;
                    e.curveAmplitude = Random.Range(8f, 16f);
                    break;

                case UiJuiceAnimator.FeelingPreset.ColorCycle:
                    e.animatePosition = true;
                    e.positionOffset = new Vector2(Random.Range(-12f, 12f), Random.Range(-30f, 10f));
                    e.animateRotation = false;
                    e.rotationOffset = Vector3.zero;
                    e.animateScale = true;
                    e.scaleFrom = Vector3.one * Random.Range(0.88f, 0.96f);
                    e.animateFade = true;
                    e.fadeFrom = Random.Range(0.05f, 0.18f);
                    e.animateColor = true;
                    var hsvCycle = Random.ColorHSV(0f, 1f, 0.7f, 1f, 0.85f, 1f, 1f, 1f);
                    e.colorFrom = hsvCycle;
                    e.duration = RD(0.6f, 0.9f, durationMin, durationMax);
                    e.delay = Random.Range(0f, 0.08f);
                    e.ease = JuiceMath.DefaultEase;
                    e.useCurvePath = false;
                    e.curveAmplitude = 0f;
                    break;
            }
        }

        private static void SetDefaults(UiJuiceAnimator.JuiceElement e)
        {
            e.animatePosition = false;
            e.positionOffset = Vector2.zero;
            e.useCurvePath = true;
            e.curveAmplitude = 0f;

            e.animateRotation = false;
            e.rotationOffset = Vector3.zero;

            e.animateScale = false;
            e.scaleFrom = Vector3.one;

            e.animateFade = false;
            e.fadeFrom = 0f;
            e.animateColor = false;
            e.colorFrom = Color.white;

            e.duration = 0.45f;
            e.delay = 0f;
            e.ease = JuiceMath.DefaultEase;

            e.useShakeJitter = false;
            e.shakeMagnitude = 0f;
            e.shakeRotationMagnitude = 0f;
            e.shakeFrequency = 0f;
            e.shakeSeed = 0f;
        }

        private static float RD(float min, float max, float clampMin, float clampMax)
        {
            return JuiceMath.RandomDuration(min, max, clampMin, clampMax);
        }
    }
}

