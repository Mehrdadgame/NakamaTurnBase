using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace NinjaBattle.UI
{
    /// <summary>
    /// Implements a classic 60s cartoon Iris Wipe (circle wipe) transition using a dedicated UI shader.
    /// Provides vector-smooth, aspect-ratio-corrected circular iris out/in transitions with a stylized rim.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public sealed class CartoonIrisTransitionOverlay : MonoBehaviour
    {
        public static CartoonIrisTransitionOverlay Instance { get; private set; }

        [SerializeField] private Shader irisShader;
        [SerializeField] private Color overlayColor = Color.black;
        [SerializeField] private Color rimColor = new Color(0.95f, 0.78f, 0.25f, 0.85f);
        [SerializeField] private float feather = 0.025f;
        [SerializeField] private float rimWidth = 0.018f;

        public static Vector2 LastIrisCenter { get; set; } = new Vector2(0.5f, 0.46f);

        [Header("Auto Play On Start (For Gameplay Scenes)")]
        [SerializeField] private bool autoPlayIrisInOnStart = false;
        [SerializeField] private float autoPlayDelay = 0.08f;
        [SerializeField] private float autoPlayDuration = 0.65f;
        [SerializeField] private Vector2 autoPlayCenter = new Vector2(0.5f, 0.46f);
        [SerializeField] private AudioClip openWhooshSound;

        public bool AutoPlayIrisInOnStart
        {
            get => autoPlayIrisInOnStart;
            set => autoPlayIrisInOnStart = value;
        }

        private Image overlayImage;
        private RectTransform rectTransform;
        private Material runtimeMaterial;
        private Tween activeTween;

        private static readonly int ProgressProp = Shader.PropertyToID("_Progress");
        private static readonly int CenterProp = Shader.PropertyToID("_Center");
        private static readonly int AspectProp = Shader.PropertyToID("_AspectRatio");
        private static readonly int ColorProp = Shader.PropertyToID("_Color");
        private static readonly int RimColorProp = Shader.PropertyToID("_RimColor");
        private static readonly int FeatherProp = Shader.PropertyToID("_Feather");
        private static readonly int RimWidthProp = Shader.PropertyToID("_RimWidth");

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            InitComponents();

            if (autoPlayIrisInOnStart)
            {
                if (autoPlayCenter == new Vector2(0.5f, 0.5f) || autoPlayCenter == Vector2.zero)
                    autoPlayCenter = LastIrisCenter;

                UpdateAspectRatio();
                if (runtimeMaterial != null)
                {
                    runtimeMaterial.SetVector(CenterProp, new Vector4(autoPlayCenter.x, autoPlayCenter.y, 0f, 0f));
                    runtimeMaterial.SetFloat(ProgressProp, 1f);
                }
                if (overlayImage != null)
                {
                    overlayImage.enabled = true;
                    overlayImage.raycastTarget = true;
                }
            }
            else
            {
                ResetInstant();
            }
        }

        private void Start()
        {
            if (autoPlayIrisInOnStart)
            {
                if (autoPlayDelay > 0f)
                {
                    DOVirtual.DelayedCall(autoPlayDelay, () =>
                    {
                        PlayIrisIn(autoPlayDuration, autoPlayCenter);
                    }).SetUpdate(true);
                }
                else
                {
                    PlayIrisIn(autoPlayDuration, autoPlayCenter);
                }
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;

            activeTween?.Kill();
            if (runtimeMaterial != null)
            {
                Destroy(runtimeMaterial);
                runtimeMaterial = null;
            }
        }

        private void InitComponents()
        {
            if (overlayImage == null)
                overlayImage = GetComponent<Image>();

            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();

            if (irisShader == null)
                irisShader = Shader.Find("UI/CartoonIrisWipe");

            if (runtimeMaterial == null && irisShader != null)
            {
                runtimeMaterial = new Material(irisShader);
                runtimeMaterial.name = "CartoonIrisWipe_Runtime";
                overlayImage.material = runtimeMaterial;
            }

            UpdateMaterialStaticProperties();
        }

        private void UpdateAspectRatio()
        {
            if (runtimeMaterial == null) return;
            // Set to 0 so the shader uses hardware _ScreenParams to ensure a perfect circle on any resolution/aspect
            runtimeMaterial.SetFloat(AspectProp, 0f);
        }

        private void UpdateMaterialStaticProperties()
        {
            if (runtimeMaterial == null)
                return;

            runtimeMaterial.SetColor(ColorProp, overlayColor);
            runtimeMaterial.SetColor(RimColorProp, rimColor);
            runtimeMaterial.SetFloat(FeatherProp, feather);
            runtimeMaterial.SetFloat(RimWidthProp, rimWidth);
            UpdateAspectRatio();
        }

        /// <summary>
        /// Closes the circular iris down to the center point (Iris Out / transition to black).
        /// </summary>
        public void PlayIrisOut(float duration, Vector2 normalizedCenter, Action onComplete = null)
        {
            LastIrisCenter = normalizedCenter;
            InitComponents();
            activeTween?.Kill();
            UpdateAspectRatio();

            if (runtimeMaterial != null)
            {
                runtimeMaterial.SetVector(CenterProp, new Vector4(normalizedCenter.x, normalizedCenter.y, 0f, 0f));
                runtimeMaterial.SetFloat(ProgressProp, 0f);
            }

            if (!gameObject.activeSelf)
                gameObject.SetActive(true);

            overlayImage.enabled = true;
            overlayImage.raycastTarget = true;

            float p = 0f;
            activeTween = DOTween.To(() => p, x =>
            {
                p = x;
                if (runtimeMaterial != null)
                    runtimeMaterial.SetFloat(ProgressProp, x);
            }, 1f, duration)
            .SetEase(Ease.InOutCubic)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                if (runtimeMaterial != null)
                    runtimeMaterial.SetFloat(ProgressProp, 1f);
                onComplete?.Invoke();
            });
        }

        /// <summary>
        /// Opens the circular iris from the center point outward (Iris In / reveal from black).
        /// </summary>
        public void PlayIrisIn(float duration, Vector2 normalizedCenter, Action onComplete = null)
        {
            if (normalizedCenter == new Vector2(0.5f, 0.5f) || normalizedCenter == Vector2.zero)
                normalizedCenter = LastIrisCenter;

            InitComponents();
            activeTween?.Kill();
            UpdateAspectRatio();

            if (runtimeMaterial != null)
            {
                runtimeMaterial.SetVector(CenterProp, new Vector4(normalizedCenter.x, normalizedCenter.y, 0f, 0f));
                runtimeMaterial.SetFloat(ProgressProp, 1f);
            }

            if (!gameObject.activeSelf)
                gameObject.SetActive(true);

            overlayImage.enabled = true;

            if (openWhooshSound != null)
            {
                var audioSrc = GetComponent<AudioSource>();
                if (audioSrc != null)
                    audioSrc.PlayOneShot(openWhooshSound, 0.75f);
            }

            float p = 1f;
            activeTween = DOTween.To(() => p, x =>
            {
                p = x;
                if (runtimeMaterial != null)
                    runtimeMaterial.SetFloat(ProgressProp, x);
            }, 0f, duration)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                ResetInstant();
                onComplete?.Invoke();
            });
        }

        /// <summary>
        /// Instantly hides the overlay and clears raycast blocking.
        /// </summary>
        public void ResetInstant()
        {
            activeTween?.Kill();
            if (runtimeMaterial != null)
            {
                runtimeMaterial.SetFloat(ProgressProp, 0f);
            }
            if (overlayImage != null)
            {
                overlayImage.raycastTarget = false;
                overlayImage.enabled = false;
            }
        }
    }
}
