using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace NinjaBattle.UI
{
    /// <summary>
    /// Handles the cinematic transition from Home view into the Castle entrance:
    /// - Smooth camera/background dolly zoom into the gateway
    /// - 3D door swing open/shut with warm golden hallway light bloom
    /// - Clash Royale juice popup for Game Mode selection
    /// - Seamless reverse transition back to default Home HUD
    /// - Robust anchor/pivot protection: preserves full-bleed screen coverage with zero offsets
    /// </summary>
    public sealed class CastleEntranceTransitionController : MonoBehaviour
    {
        public static CastleEntranceTransitionController Instance { get; private set; }

        [Header("Castle Gateway Elements")]
        [SerializeField] private RectTransform backgroundRect;
        [SerializeField] private RectTransform doorwayRect;
        [SerializeField] private RectTransform doorLeft;
        [SerializeField] private RectTransform doorRight;
        [SerializeField] private Image doorGlow;
        [SerializeField] private Image doorInterior;

        [Header("Home HUD Elements")]
        [SerializeField] private RectTransform startCta;
        [SerializeField] private RectTransform chestRow;
        [SerializeField] private RectTransform topBar;
        [SerializeField] private RectTransform footer;

        [Header("Popup Elements")]
        [SerializeField] private RectTransform modePopup;
        [SerializeField] private CanvasGroup modePopupCanvasGroup;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button backdropButton;

        [Header("Mode Buttons")]
        [SerializeField] private Button quickModeButton;
        [SerializeField] private Button professionalModeButton;
        [SerializeField] private Button masterModeButton;

        [Header("Battle Launch & 60s Cartoon Iris Transition")]
        [SerializeField] private CartoonIrisTransitionOverlay irisOverlay;
        [SerializeField] private AudioClip gateSlamSound;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private Vector2 irisCenterNormalized = new Vector2(0.5f, 0.46f);

        [Header("Transition Settings")]
        [SerializeField] private float targetZoomScale = 2.45f;
        [SerializeField] private float targetZoomOffsetY = 90f;
        [SerializeField] private float zoomDuration = 0.85f;
        [SerializeField] private float doorOpenAngle = 82f;
        [SerializeField] private float doorDuration = 0.52f;
        [SerializeField] private float returnDuration = 0.6f;

        private Vector2 initialStartCtaPos;
        private Vector2 initialChestRowPos;
        private Vector2 initialTopBarPos;
        private Vector2 initialFooterPos;

        private CanvasGroup startCtaCg;
        private CanvasGroup chestRowCg;
        private CanvasGroup topBarCg;
        private CanvasGroup footerCg;

        private Sequence activeSequence;
        private Tween glowPulseTween;
        private bool isTransitioning;
        private bool isEntranceOpen;

        public bool IsEntranceOpen => isEntranceOpen;
        public bool IsTransitioning => isTransitioning;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;

            ResetBackgroundToFullscreen();

            if (modePopup != null)
            {
                modePopup.gameObject.SetActive(false);
            }

            CacheInitialPositions();
            WireButtons();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;

            activeSequence?.Kill();
            glowPulseTween?.Kill();
        }

        public void AutoConfigure(
            RectTransform bg,
            RectTransform doorway,
            RectTransform left,
            RectTransform right,
            Image glow,
            Image interior,
            RectTransform start,
            RectTransform chest,
            RectTransform top,
            RectTransform foot,
            RectTransform popup,
            CanvasGroup popupCg,
            Button closeBtn,
            Button backdropBtn)
        {
            backgroundRect = bg;
            doorwayRect = doorway;
            doorLeft = left;
            doorRight = right;
            doorGlow = glow;
            doorInterior = interior;
            startCta = start;
            chestRow = chest;
            topBar = top;
            footer = foot;
            modePopup = popup;
            modePopupCanvasGroup = popupCg;
            closeButton = closeBtn;
            backdropButton = backdropBtn;

            ResetBackgroundToFullscreen();
            CacheInitialPositions();
            WireButtons();
        }

        private void ResetBackgroundToFullscreen()
        {
            if (backgroundRect != null)
            {
                backgroundRect.DOKill();
                backgroundRect.pivot = new Vector2(0.5f, 0.5f);
                backgroundRect.anchorMin = Vector2.zero;
                backgroundRect.anchorMax = Vector2.one;
                backgroundRect.offsetMin = Vector2.zero;
                backgroundRect.offsetMax = Vector2.zero;
                backgroundRect.anchoredPosition = Vector2.zero;
                backgroundRect.sizeDelta = Vector2.zero;
                backgroundRect.localScale = Vector3.one;
            }

            if (doorwayRect != null)
            {
                doorwayRect.anchoredPosition = new Vector2(0f, -90.75f);
            }
        }

        private void CacheInitialPositions()
        {
            if (startCta != null)
            {
                initialStartCtaPos = startCta.anchoredPosition;
                startCtaCg = GetOrAddCanvasGroup(startCta.gameObject);
            }

            if (chestRow != null)
            {
                initialChestRowPos = chestRow.anchoredPosition;
                chestRowCg = GetOrAddCanvasGroup(chestRow.gameObject);
            }

            if (topBar != null)
            {
                initialTopBarPos = topBar.anchoredPosition;
                topBarCg = GetOrAddCanvasGroup(topBar.gameObject);
            }

            if (footer != null)
            {
                initialFooterPos = footer.anchoredPosition;
                footerCg = GetOrAddCanvasGroup(footer.gameObject);
            }

            if (doorGlow != null)
            {
                Color c = doorGlow.color;
                c.a = 0f;
                doorGlow.color = c;
            }
        }

        public void ConfigureIrisAndAudio(CartoonIrisTransitionOverlay overlay, AudioClip slamClip, AudioSource audioSrc)
        {
            irisOverlay = overlay;
            gateSlamSound = slamClip;
            sfxSource = audioSrc;
        }

        public void ConfigureModeButtons(Button quick, Button professional, Button master)
        {
            quickModeButton = quick;
            professionalModeButton = professional;
            masterModeButton = master;
            WireModeButtons();
        }

        private void WireButtons()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(OnCloseClicked);
                closeButton.onClick.AddListener(OnCloseClicked);
            }

            if (backdropButton != null)
            {
                backdropButton.onClick.RemoveListener(OnCloseClicked);
                backdropButton.onClick.AddListener(OnCloseClicked);
            }

            WireModeButtons();
        }

        private void WireModeButtons()
        {
            if (quickModeButton == null && modePopup != null)
                quickModeButton = modePopup.transform.Find("back/QuickModeButton")?.GetComponent<Button>();
            if (professionalModeButton == null && modePopup != null)
                professionalModeButton = modePopup.transform.Find("back/ProfessionalModeButton")?.GetComponent<Button>();
            if (masterModeButton == null && modePopup != null)
                masterModeButton = modePopup.transform.Find("back/MasterModeButton")?.GetComponent<Button>();

            if (quickModeButton != null)
            {
                quickModeButton.onClick.RemoveListener(OnQuickModeClicked);
                quickModeButton.onClick.AddListener(OnQuickModeClicked);
            }
            if (professionalModeButton != null)
            {
                professionalModeButton.onClick.RemoveListener(OnProfessionalModeClicked);
                professionalModeButton.onClick.AddListener(OnProfessionalModeClicked);
            }
            if (masterModeButton != null)
            {
                masterModeButton.onClick.RemoveListener(OnMasterModeClicked);
                masterModeButton.onClick.AddListener(OnMasterModeClicked);
            }
        }

        private void OnQuickModeClicked()
        {
            var smg = quickModeButton != null ? quickModeButton.GetComponent<SetModeGame>() : null;
            ModeGame mode = smg != null ? smg.modeGame : ModeGame.VerticalAndHorizontal;
            PlayBattleLaunchSequence(mode, quickModeButton);
        }

        private void OnProfessionalModeClicked()
        {
            var smg = professionalModeButton != null ? professionalModeButton.GetComponent<SetModeGame>() : null;
            ModeGame mode = smg != null ? smg.modeGame : ModeGame.FourByThree;
            PlayBattleLaunchSequence(mode, professionalModeButton);
        }

        private void OnMasterModeClicked()
        {
            var smg = masterModeButton != null ? masterModeButton.GetComponent<SetModeGame>() : null;
            ModeGame mode = smg != null ? smg.modeGame : ModeGame.ThreeByThree;
            PlayBattleLaunchSequence(mode, masterModeButton);
        }

        private void OnCloseClicked()
        {
            ReturnToHomeSequence();
        }

        /// <summary>
        /// Plays the cinematic dolly zoom into the castle, swings doors open, and pops open the Game Mode selector.
        /// </summary>
        public void PlayEntranceSequence(Action onComplete = null)
        {
            if (isTransitioning || isEntranceOpen)
                return;

            isTransitioning = true;
            isEntranceOpen = true;

            activeSequence?.Kill();
            glowPulseTween?.Kill();

            Sequence seq = DOTween.Sequence().SetUpdate(true);

            // 1. HUD Exit Animations
            if (startCta != null)
            {
                startCta.DOKill();
                seq.Join(startCta.DOPunchScale(Vector3.one * -0.06f, 0.2f, 5, 0.5f));
                if (startCtaCg != null)
                    seq.Join(startCtaCg.DOFade(0f, 0.25f).SetEase(Ease.InQuad));
            }

            if (chestRow != null)
            {
                chestRow.DOKill();
                seq.Join(chestRow.DOAnchorPosY(initialChestRowPos.y - 300f, 0.35f).SetEase(Ease.InQuad));
                if (chestRowCg != null)
                    seq.Join(chestRowCg.DOFade(0f, 0.25f).SetEase(Ease.InQuad));
            }

            if (topBar != null)
            {
                topBar.DOKill();
                seq.Join(topBar.DOAnchorPosY(initialTopBarPos.y + 250f, 0.35f).SetEase(Ease.InQuad));
                if (topBarCg != null)
                    seq.Join(topBarCg.DOFade(0f, 0.25f).SetEase(Ease.InQuad));
            }

            if (footer != null)
            {
                footer.DOKill();
                seq.Join(footer.DOAnchorPosY(initialFooterPos.y - 350f, 0.35f).SetEase(Ease.InQuad));
                if (footerCg != null)
                    seq.Join(footerCg.DOFade(0f, 0.25f).SetEase(Ease.InQuad));
            }

            // 2. Camera / Background Dolly Zoom (pivot remains fixed at 0.5, 0.5; position translates cleanly)
            if (backgroundRect != null)
            {
                backgroundRect.DOKill();
                seq.Join(backgroundRect.DOScale(Vector3.one * targetZoomScale, zoomDuration)
                    .SetEase(Ease.InOutCubic));
                seq.Join(backgroundRect.DOAnchorPos(new Vector2(0f, targetZoomOffsetY), zoomDuration)
                    .SetEase(Ease.InOutCubic));
            }

            // 3. Castle Doors Swing Open
            float doorOpenDelay = 0.32f;
            if (doorLeft != null)
            {
                doorLeft.DOKill();
                doorLeft.localEulerAngles = Vector3.zero;
                seq.Insert(doorOpenDelay, doorLeft.DOLocalRotate(new Vector3(0f, -doorOpenAngle, 0f), doorDuration)
                    .SetEase(Ease.OutBack, 1.15f));
            }

            if (doorRight != null)
            {
                doorRight.DOKill();
                doorRight.localEulerAngles = Vector3.zero;
                seq.Insert(doorOpenDelay, doorRight.DOLocalRotate(new Vector3(0f, doorOpenAngle, 0f), doorDuration)
                    .SetEase(Ease.OutBack, 1.15f));
            }

            // 4. Hallway Warm Glow Bloom
            if (doorGlow != null)
            {
                doorGlow.DOKill();
                seq.Insert(doorOpenDelay + 0.1f, doorGlow.DOFade(0.85f, doorDuration - 0.1f).SetEase(Ease.OutQuad));
                seq.InsertCallback(doorOpenDelay + doorDuration, () =>
                {
                    glowPulseTween = doorGlow.DOFade(0.6f, 1.2f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
                });
            }

            // Play sound if AudioManager exists
            seq.InsertCallback(doorOpenDelay, () =>
            {
                if (General.AudioManager.Instance != null)
                {
                    General.AudioManager.Instance.PlayClickSound();
                }
            });

            // 5. GameModePopup Juicy Entrance
            if (modePopup != null)
            {
                seq.InsertCallback(zoomDuration - 0.05f, () =>
                {
                    modePopup.DOKill();
                    modePopup.gameObject.SetActive(true);
                    modePopup.localScale = Vector3.one * 0.7f;
                    if (modePopupCanvasGroup != null)
                        modePopupCanvasGroup.alpha = 0f;

                    modePopup.DOScale(Vector3.one, 0.32f).SetEase(Ease.OutBack, 1.25f).SetUpdate(true);
                    if (modePopupCanvasGroup != null)
                        modePopupCanvasGroup.DOFade(1f, 0.22f).SetEase(Ease.OutQuad).SetUpdate(true);
                });
            }

            seq.OnComplete(() =>
            {
                isTransitioning = false;
                onComplete?.Invoke();
            });

            activeSequence = seq;
        }

        /// <summary>
        /// Reverse animation: Closes the Game Mode popup, swings doors shut, zooms back out, and restores Home HUD.
        /// </summary>
        public void ReturnToHomeSequence(Action onComplete = null)
        {
            if (isTransitioning || !isEntranceOpen)
                return;

            isTransitioning = true;
            activeSequence?.Kill();
            glowPulseTween?.Kill();

            Sequence seq = DOTween.Sequence().SetUpdate(true);

            // 1. Popup Exit
            if (modePopup != null && modePopup.gameObject.activeSelf)
            {
                modePopup.DOKill();
                seq.Join(modePopup.DOScale(Vector3.one * 0.78f, 0.18f).SetEase(Ease.InBack));
                if (modePopupCanvasGroup != null)
                {
                    modePopupCanvasGroup.DOKill();
                    seq.Join(modePopupCanvasGroup.DOFade(0f, 0.16f).SetEase(Ease.InQuad));
                }
                seq.AppendCallback(() => modePopup.gameObject.SetActive(false));
            }

            // 2. Castle Doors Swing Shut
            if (doorLeft != null)
            {
                doorLeft.DOKill();
                seq.Join(doorLeft.DOLocalRotate(Vector3.zero, 0.42f).SetEase(Ease.InCubic));
            }

            if (doorRight != null)
            {
                doorRight.DOKill();
                seq.Join(doorRight.DOLocalRotate(Vector3.zero, 0.42f).SetEase(Ease.InCubic));
            }

            if (doorGlow != null)
            {
                doorGlow.DOKill();
                seq.Join(doorGlow.DOFade(0f, 0.35f).SetEase(Ease.InQuad));
            }

            // 3. Dolly Zoom Back Out & Return to (0, 0)
            if (backgroundRect != null)
            {
                backgroundRect.DOKill();
                seq.Join(backgroundRect.DOScale(Vector3.one, returnDuration).SetEase(Ease.OutCubic));
                seq.Join(backgroundRect.DOAnchorPos(Vector2.zero, returnDuration).SetEase(Ease.OutCubic));
            }

            // 4. Restore Home HUD Elements
            float hudReturnDelay = 0.22f;
            if (startCta != null)
            {
                startCta.DOKill();
                seq.Insert(hudReturnDelay, startCta.DOScale(Vector3.one, 0.32f).SetEase(Ease.OutBack, 1.2f));
                if (startCtaCg != null)
                    seq.Insert(hudReturnDelay, startCtaCg.DOFade(1f, 0.3f));
            }

            if (chestRow != null)
            {
                chestRow.DOKill();
                seq.Insert(hudReturnDelay, chestRow.DOAnchorPos(initialChestRowPos, 0.38f).SetEase(Ease.OutBack, 1.1f));
                if (chestRowCg != null)
                    seq.Insert(hudReturnDelay, chestRowCg.DOFade(1f, 0.3f));
            }

            if (topBar != null)
            {
                topBar.DOKill();
                seq.Insert(hudReturnDelay, topBar.DOAnchorPos(initialTopBarPos, 0.38f).SetEase(Ease.OutBack, 1.1f));
                if (topBarCg != null)
                    seq.Insert(hudReturnDelay, topBarCg.DOFade(1f, 0.3f));
            }

            if (footer != null)
            {
                footer.DOKill();
                seq.Insert(hudReturnDelay, footer.DOAnchorPos(initialFooterPos, 0.38f).SetEase(Ease.OutBack, 1.1f));
                if (footerCg != null)
                    seq.Insert(hudReturnDelay, footerCg.DOFade(1f, 0.3f));
            }

            seq.OnComplete(() =>
            {
                ResetBackgroundToFullscreen();
                isEntranceOpen = false;
                isTransitioning = false;
                onComplete?.Invoke();
            });

            activeSequence = seq;
        }

        /// <summary>
        /// Orchestrates the high-juice cinematic Battle Launch sequence:
        /// 1. Button click punch and juicy bounce (0.0s - 0.18s)
        /// 2. Mode popup elastic dismissal (0.08s - 0.28s)
        /// 3. Castle doors slam shut with heavy audio + screen impact shake (0.24s - 0.62s)
        /// 4. Background smooth dolly zoom back (0.24s - 0.85s)
        /// 5. Classic 60s cartoon Iris Wipe (circle wipe to black) closing in on the castle gate (0.64s - 1.45s)
        /// 6. Scene load / matchmaking launch at ~1.70s (well under 2s budget)
        /// </summary>
        public void PlayBattleLaunchSequence(ModeGame mode, Button clickedButton = null, Action onLaunchReady = null)
        {
            if (isTransitioning)
                return;

            isTransitioning = true;
            activeSequence?.Kill();
            glowPulseTween?.Kill();

            if (irisOverlay != null && !irisOverlay.gameObject.activeSelf)
            {
                irisOverlay.gameObject.SetActive(true);
            }

            // 1. Audio click
            if (General.AudioManager.Instance != null)
            {
                General.AudioManager.Instance.PlayClickSound();
            }

            // 2. Button punch bounce & dim others
            if (clickedButton != null)
            {
                clickedButton.transform.DOKill();
                clickedButton.transform.DOPunchScale(new Vector3(0.09f, -0.09f, 0f), 0.22f, 5, 0.4f).SetUpdate(true);
            }

            Button[] allModeBtns = new[] { quickModeButton, professionalModeButton, masterModeButton };
            foreach (var b in allModeBtns)
            {
                if (b != null && b != clickedButton)
                {
                    var cg = GetOrAddCanvasGroup(b.gameObject);
                    cg.DOKill();
                    cg.DOFade(0.35f, 0.15f).SetUpdate(true);
                }
            }

            Sequence seq = DOTween.Sequence().SetUpdate(true);

            // Phase 1: Popup Juicy Exit (0.08s -> 0.28s)
            float popupExitStart = 0.08f;
            float popupExitDuration = 0.20f;
            if (modePopup != null)
            {
                modePopup.DOKill();
                seq.Insert(popupExitStart, modePopup.DOScale(Vector3.one * 0.72f, popupExitDuration).SetEase(Ease.InBack, 1.3f));
                if (modePopupCanvasGroup != null)
                {
                    modePopupCanvasGroup.DOKill();
                    seq.Insert(popupExitStart, modePopupCanvasGroup.DOFade(0f, popupExitDuration * 0.85f).SetEase(Ease.InQuad));
                }
                seq.InsertCallback(popupExitStart + popupExitDuration, () =>
                {
                    modePopup.gameObject.SetActive(false);
                });
            }

            // Phase 2: Castle Doors Slam Shut & Background Zoom (0.24s -> 0.62s)
            float doorSlamStart = 0.24f;
            float doorSlamDuration = 0.36f;
            float doorImpactTime = doorSlamStart + doorSlamDuration;

            if (doorLeft != null)
            {
                doorLeft.DOKill();
                seq.Insert(doorSlamStart, doorLeft.DOLocalRotate(Vector3.zero, doorSlamDuration).SetEase(Ease.InCubic));
            }

            if (doorRight != null)
            {
                doorRight.DOKill();
                seq.Insert(doorSlamStart, doorRight.DOLocalRotate(Vector3.zero, doorSlamDuration).SetEase(Ease.InCubic));
            }

            if (doorGlow != null)
            {
                doorGlow.DOKill();
                seq.Insert(doorSlamStart, doorGlow.DOFade(0f, doorSlamDuration * 0.7f).SetEase(Ease.InQuad));
            }

            // Dolly zoom background back to 1.0 cleanly
            if (backgroundRect != null)
            {
                backgroundRect.DOKill();
                seq.Insert(doorSlamStart, backgroundRect.DOScale(Vector3.one, 0.65f).SetEase(Ease.OutCubic));
                seq.Insert(doorSlamStart, backgroundRect.DOAnchorPos(Vector2.zero, 0.65f).SetEase(Ease.OutCubic));
            }

            // Doors SLAM Impact Moment
            seq.InsertCallback(doorImpactTime, () =>
            {
                // Play heavy door slam SFX
                if (sfxSource != null && gateSlamSound != null)
                {
                    sfxSource.PlayOneShot(gateSlamSound, 1f);
                }
                else if (General.AudioManager.Instance != null)
                {
                    General.AudioManager.Instance.PlayClickSound();
                }

                // Micro screen punch shake on doorway
                if (doorwayRect != null)
                {
                    doorwayRect.DOKill();
                    doorwayRect.DOPunchPosition(new Vector3(0f, -14f, 0f), 0.25f, 12, 0.6f);
                }
            });

            // Phase 3: Classic 60s Cartoon Iris Wipe (0.64s -> 1.45s)
            float irisStart = 0.64f;
            float irisDuration = 0.76f;
            seq.InsertCallback(irisStart, () =>
            {
                if (irisOverlay != null)
                {
                    irisOverlay.PlayIrisOut(irisDuration, irisCenterNormalized);
                }
            });

            // Phase 4: Battle Scene Launch / Matchmaking (at 1.70s)
            float launchTime = 1.70f;
            seq.InsertCallback(launchTime, () =>
            {
                isTransitioning = false;
                isEntranceOpen = false;
                onLaunchReady?.Invoke();
                InitiateGameLaunch(mode);
            });

            activeSequence = seq;
        }

        private void InitiateGameLaunch(ModeGame mode)
        {
            if (NinjaBattle.Game.GameManager.Instance != null)
            {
                NinjaBattle.Game.GameManager.Instance.modeGame = mode;
            }

            var mp = Nakama.Helpers.MultiplayerManager.Instance;
            var nk = Nakama.Helpers.NakamaManager.Instance;
            bool isOnline = mp != null && nk != null && nk.Socket != null && nk.Socket.IsConnected;

            if (isOnline)
            {
                Debug.Log($"[CastleEntrance] Online match request initiated for mode {mode}");
                mp.JoinMatchAsync(mode);
            }
            else
            {
                Debug.Log($"[CastleEntrance] Loading scene directly for mode {mode}");
                int sceneIndex = (int)General.Scenes.ThreeByThree;
                switch (mode)
                {
                    case ModeGame.ThreeByThree:
                        sceneIndex = (int)General.Scenes.ThreeByThree;
                        break;
                    case ModeGame.FourByThree:
                        sceneIndex = (int)General.Scenes.FourByThree;
                        break;
                    case ModeGame.VerticalAndHorizontal:
                        sceneIndex = (int)General.Scenes.VerticalAndHorizontal;
                        break;
                }

                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex);
            }
        }

        private static CanvasGroup GetOrAddCanvasGroup(GameObject target)
        {
            CanvasGroup cg = target.GetComponent<CanvasGroup>();
            if (cg == null)
                cg = target.AddComponent<CanvasGroup>();
            return cg;
        }
    }
}
