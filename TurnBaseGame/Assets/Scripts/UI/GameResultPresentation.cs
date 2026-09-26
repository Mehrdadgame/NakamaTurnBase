using System;
using System.Collections;
using DG.Tweening;
using Nakama.Helpers;
using NinjaBattle.Game;
using RTLTMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NinjaBattle.UI
{
    /// <summary>
    /// Deluxe Clash Royale-style Win/Lose Result Presentation matching the Figma design.
    /// Features:
    /// - Two player profile headers with avatars, names, and scores
    /// - Green circular 'R' rematch badge
    /// - Animated Win state: Golden glow rays, crowned dice trophy bounce, laurel float, celebration fanfare
    /// - Animated Lose state: Dizzy cartoon stone dice squash, hypnotic head sway, spinning stars
    /// - Score numeric count-up roll
    /// - Orange 3D CTA button with iris-out exit transition back to Home
    /// </summary>
    public sealed class GameResultPresentation : MonoBehaviour
    {
        [Header("Modal & Backdrop")]
        [SerializeField] private CanvasGroup backdropGroup;
        [SerializeField] private RectTransform modalCard;
        [SerializeField] private Image cardBackground;

        [Header("Header Elements")]
        [SerializeField] private Image player1Avatar;
        [SerializeField] private Image player2Avatar;
        [SerializeField] private RTLTextMeshPro player1Name;
        [SerializeField] private RTLTextMeshPro player2Name;
        [SerializeField] private RTLTextMeshPro player1Score;
        [SerializeField] private RTLTextMeshPro player2Score;

        [Header("Center Visuals")]
        [SerializeField] private Image shadowFloor;
        [SerializeField] private RectTransform winContainer;
        [SerializeField] private Image goldenGlowAura;
        [SerializeField] private Image winTrophy;
        [SerializeField] private RectTransform loseContainer;
        [SerializeField] private Image loseDice;
        [SerializeField] private RectTransform loseStars;

        [Header("Result Label & Button")]
        [SerializeField] private RTLTextMeshPro resultTitle;
        [SerializeField] private Button returnHomeButton;
        [SerializeField] private RTLTextMeshPro returnHomeLabel;

        [Header("Audio")]
        [SerializeField] private AudioClip winFanfareClip;
        [SerializeField] private AudioClip loseSoundClip;
        [SerializeField] private AudioSource audioSource;

        private Sequence activeSequence;
        private Tween glowPulseTween;
        private Tween glowRotateTween;
        private Tween trophyFloatTween;
        private Tween loseWobbleTween;
        private Tween starsRotateTween;
        private Tween buttonBreatheTween;
        private Tween winnerAvatarTween;

        private float initialTrophyY = 0f;
        private float initialDiceY = 0f;

        private Vector2 player1AvatarHome;
        private Vector2 player2AvatarHome;
        private bool avatarHomesCaptured;

        private void Awake()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                    audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }

            if (winTrophy != null)
                initialTrophyY = winTrophy.rectTransform.anchoredPosition.y;

            if (loseDice != null)
                initialDiceY = loseDice.rectTransform.anchoredPosition.y;

            if (returnHomeButton != null)
            {
                returnHomeButton.onClick.RemoveListener(ReturnHome);
                returnHomeButton.onClick.AddListener(ReturnHome);
            }
        }

        private void OnDestroy()
        {
            KillAllTweens();
        }

        private void KillAllTweens()
        {
            activeSequence?.Kill();
            glowPulseTween?.Kill();
            glowRotateTween?.Kill();
            trophyFloatTween?.Kill();
            loseWobbleTween?.Kill();
            starsRotateTween?.Kill();
            buttonBreatheTween?.Kill();
            winnerAvatarTween?.Kill();

            if (player1Avatar != null) player1Avatar.rectTransform.DOKill();
            if (player2Avatar != null) player2Avatar.rectTransform.DOKill();

            if (modalCard != null) modalCard.DOKill();
            if (winTrophy != null) winTrophy.rectTransform.DOKill();
            if (loseDice != null) loseDice.rectTransform.DOKill();
            if (loseStars != null) loseStars.DOKill();
            if (goldenGlowAura != null) goldenGlowAura.rectTransform.DOKill();
            if (resultTitle != null) resultTitle.rectTransform.DOKill();
            if (returnHomeButton != null) returnHomeButton.transform.DOKill();
            if (backdropGroup != null) backdropGroup.DOKill();
        }

        // Legacy overload for compatibility with older authoring scripts
        public void Configure(RTLTextMeshPro title, Image trophy, Component panel, Sprite winSprite, Sprite loseSprite)
        {
            resultTitle = title;
            winTrophy = trophy;
            if (panel is Image img) cardBackground = img;
            else if (panel is RectTransform rt) modalCard = rt;
        }

        public void Configure(
            CanvasGroup backdrop,
            RectTransform modal,
            Image cardBg,
            Image p1Avatar,
            Image p2Avatar,
            RTLTextMeshPro p1Name,
            RTLTextMeshPro p2Name,
            RTLTextMeshPro p1Score,
            RTLTextMeshPro p2Score,
            Image rBadge,
            Image shadow,
            RectTransform winRoot,
            Image glowAura,
            Image trophy,
            RectTransform loseRoot,
            Image dice,
            RectTransform stars,
            RTLTextMeshPro title,
            Button returnBtn,
            RTLTextMeshPro returnLbl,
            AudioClip winClip = null,
            AudioClip loseClip = null)
        {
            backdropGroup = backdrop;
            modalCard = modal;
            cardBackground = cardBg;
            player1Avatar = p1Avatar;
            player2Avatar = p2Avatar;
            player1Name = p1Name;
            player2Name = p2Name;
            player1Score = p1Score;
            player2Score = p2Score;
            shadowFloor = shadow;
            winContainer = winRoot;
            goldenGlowAura = glowAura;
            winTrophy = trophy;
            loseContainer = loseRoot;
            loseDice = dice;
            loseStars = stars;
            resultTitle = title;
            returnHomeButton = returnBtn;
            returnHomeLabel = returnLbl;
            winFanfareClip = winClip;
            loseSoundClip = loseClip;

            if (returnHomeButton != null)
            {
                returnHomeButton.onClick.RemoveListener(ReturnHome);
                returnHomeButton.onClick.AddListener(ReturnHome);
            }
        }

        private static string ToPersianDigits(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            if (!Localization.IsPersian) return input;
            return input.Replace('0', '۰')
                        .Replace('1', '۱')
                        .Replace('2', '۲')
                        .Replace('3', '۳')
                        .Replace('4', '۴')
                        .Replace('5', '۵')
                        .Replace('6', '۶')
                        .Replace('7', '۷')
                        .Replace('8', '۸')
                        .Replace('9', '۹');
        }

        public void ShowPreview(bool isWin, int score1, int score2, string p1 = "shahin ۲۲", string p2 = "sohrab ۱")
        {
            KillAllTweens();

            if (backdropGroup != null) backdropGroup.alpha = 0.72f;
            if (modalCard != null)
            {
                modalCard.localScale = Vector3.one;
                modalCard.anchoredPosition = Vector2.zero;
            }

            if (player1Name != null) player1Name.text = p1;
            if (player2Name != null) player2Name.text = p2;
            if (player1Score != null)
            {
                player1Score.text = ToPersianDigits(score1.ToString());
                player1Score.color = new Color32(74, 46, 8, 255);
            }
            if (player2Score != null)
            {
                player2Score.text = ToPersianDigits(score2.ToString());
                player2Score.color = new Color32(74, 46, 8, 255);
            }

            if (resultTitle != null)
            {
                resultTitle.text = isWin ? Localization.L("برنده شدی!", "You Win!") : Localization.L("باختی!", "You Lost!");
                resultTitle.color = isWin ? new Color32(109, 62, 12, 255) : new Color32(80, 42, 22, 255);
                resultTitle.rectTransform.localScale = Vector3.one;
            }

            if (winContainer != null) winContainer.gameObject.SetActive(isWin);
            if (loseContainer != null) loseContainer.gameObject.SetActive(!isWin);

            if (shadowFloor != null) shadowFloor.gameObject.SetActive(true);

            if (isWin)
            {
                if (winTrophy != null)
                {
                    winTrophy.rectTransform.anchoredPosition = new Vector2(0f, 60f);
                    winTrophy.rectTransform.localScale = Vector3.one;
                }
                if (goldenGlowAura != null)
                {
                    goldenGlowAura.rectTransform.localScale = Vector3.one;
                }
            }
            else
            {
                if (loseDice != null)
                {
                    loseDice.rectTransform.anchoredPosition = new Vector2(0f, 35f);
                    loseDice.rectTransform.localScale = Vector3.one;
                    loseDice.rectTransform.localEulerAngles = Vector3.zero;
                }
                if (loseStars != null)
                {
                    loseStars.localScale = Vector3.one;
                }
            }

            if (returnHomeButton != null)
            {
                returnHomeButton.transform.localScale = Vector3.one;
            }
        }

        public void Refresh(string result)
        {
            KillAllTweens();

            bool isWin = result != null && (result.Contains("برد") || result.Contains("VICTORY") || result.Contains("برنده") || result.Contains("Win"));
            bool isDraw = result != null && (result.Contains("مساوی") || result.Contains("DRAW") || result.Contains("Draw"));
            bool isLose = !isWin && !isDraw;

            // Update title text and color
            if (resultTitle != null)
            {
                if (isWin)
                {
                    resultTitle.text = Localization.L("برنده شدی!", "You Win!");
                    resultTitle.color = new Color32(109, 62, 12, 255);
                }
                else if (isDraw)
                {
                    resultTitle.text = Localization.L("بازی مساوی شد", "It's a Draw");
                    resultTitle.color = new Color32(90, 50, 10, 255);
                }
                else
                {
                    resultTitle.text = Localization.L("باختی!", "You Lost!");
                    resultTitle.color = new Color32(80, 42, 22, 255);
                }
            }

            // Sync scores from ActionEndGame. Read OriginalText — the .text getter
            // returns reshaped glyphs (and Persian digits never parse as int anyway).
            int targetMeScore = 0;
            int targetOppScore = 0;
            if (ActionEndGame.instance != null)
            {
                if (ActionEndGame.instance.ScoreMe != null && int.TryParse(ActionEndGame.instance.ScoreMe.OriginalText, out int s1))
                    targetMeScore = s1;
                if (ActionEndGame.instance.ScoreOpp != null && int.TryParse(ActionEndGame.instance.ScoreOpp.OriginalText, out int s2))
                    targetOppScore = s2;
            }

            ApplyPlayerIdentities();

            // Display containers
            if (winContainer != null) winContainer.gameObject.SetActive(isWin || isDraw);
            if (loseContainer != null) loseContainer.gameObject.SetActive(isLose);

            // Animate Modal Sequence
            Sequence seq = DOTween.Sequence().SetUpdate(true);

            // 1. Backdrop Fade In
            if (backdropGroup != null)
            {
                backdropGroup.alpha = 0f;
                seq.Join(backdropGroup.DOFade(0.72f, 0.35f).SetEase(Ease.OutQuad));
            }

            // 2. Modal Card Elastic Pop
            if (modalCard != null)
            {
                modalCard.localScale = Vector3.one * 0.45f;
                modalCard.anchoredPosition = new Vector2(0f, 80f);
                seq.Join(modalCard.DOScale(1f, 0.45f).SetEase(Ease.OutBack, 1.4f));
                seq.Join(modalCard.DOAnchorPosY(0f, 0.42f).SetEase(Ease.OutCubic));
            }

            // 2.5 Avatars fly in from the top of the screen into their header slots,
            //     then the winner's avatar celebrates and the loser's dims.
            AnimateAvatars(seq, isWin, isDraw);

            // 3. Audio Trigger
            seq.InsertCallback(0.12f, () =>
            {
                if (audioSource != null)
                {
                    if (isWin && winFanfareClip != null)
                        audioSource.PlayOneShot(winFanfareClip, 0.9f);
                    else if (isLose && loseSoundClip != null)
                        audioSource.PlayOneShot(loseSoundClip, 0.85f);
                }
            });

            // 4. Score numeric rolling count-up
            float countUpDuration = 0.55f;
            int currentMe = 0;
            int currentOpp = 0;
            if (player1Score != null)
            {
                player1Score.text = ToPersianDigits("0");
                player1Score.color = new Color32(74, 46, 8, 255);
                seq.Insert(0.25f, DOTween.To(() => currentMe, x =>
                {
                    currentMe = x;
                    player1Score.text = ToPersianDigits(x.ToString());
                }, targetMeScore, countUpDuration).SetEase(Ease.OutCubic));
            }
            if (player2Score != null)
            {
                player2Score.text = ToPersianDigits("0");
                player2Score.color = new Color32(74, 46, 8, 255);
                seq.Insert(0.25f, DOTween.To(() => currentOpp, x =>
                {
                    currentOpp = x;
                    player2Score.text = ToPersianDigits(x.ToString());
                }, targetOppScore, countUpDuration).SetEase(Ease.OutCubic));
            }

            // 5. Win Character / Trophy Animation
            if (isWin || isDraw)
            {
                if (winTrophy != null)
                {
                    winTrophy.rectTransform.localScale = Vector3.one * 0.2f;
                    winTrophy.rectTransform.anchoredPosition = new Vector2(0f, initialTrophyY - 40f);
                    seq.Insert(0.18f, winTrophy.rectTransform.DOScale(1f, 0.52f).SetEase(Ease.OutBack, 1.45f));
                    seq.Insert(0.18f, winTrophy.rectTransform.DOAnchorPosY(initialTrophyY, 0.48f).SetEase(Ease.OutBack, 1.3f));

                    seq.InsertCallback(0.70f, () =>
                    {
                        trophyFloatTween = winTrophy.rectTransform.DOAnchorPosY(initialTrophyY + 14f, 1.4f)
                            .SetLoops(-1, LoopType.Yoyo)
                            .SetEase(Ease.InOutSine)
                            .SetUpdate(true);
                    });
                }

                if (goldenGlowAura != null)
                {
                    Color gc = goldenGlowAura.color;
                    gc.a = 0f;
                    goldenGlowAura.color = gc;
                    goldenGlowAura.rectTransform.localScale = Vector3.one * 0.7f;
                    seq.Insert(0.22f, goldenGlowAura.DOFade(0.75f, 0.45f));
                    seq.Insert(0.22f, goldenGlowAura.rectTransform.DOScale(1.05f, 0.45f).SetEase(Ease.OutQuad));

                    seq.InsertCallback(0.70f, () =>
                    {
                        glowPulseTween = goldenGlowAura.rectTransform.DOScale(1.18f, 1.25f)
                            .SetLoops(-1, LoopType.Yoyo)
                            .SetEase(Ease.InOutSine)
                            .SetUpdate(true);

                        glowRotateTween = goldenGlowAura.rectTransform.DOLocalRotate(new Vector3(0, 0, 360f), 14f, RotateMode.FastBeyond360)
                            .SetLoops(-1, LoopType.Restart)
                            .SetEase(Ease.Linear)
                            .SetUpdate(true);
                    });
                }
            }
            // 6. Lose Character Animation (Squash drop, dizzy wobble & rotating stars)
            else
            {
                if (loseDice != null)
                {
                    loseDice.rectTransform.localScale = Vector3.one * 0.25f;
                    loseDice.rectTransform.anchoredPosition = new Vector2(0f, initialDiceY + 60f);
                    seq.Insert(0.15f, loseDice.rectTransform.DOScale(1f, 0.38f).SetEase(Ease.OutBounce));
                    seq.Insert(0.15f, loseDice.rectTransform.DOAnchorPosY(initialDiceY, 0.38f).SetEase(Ease.OutBounce));

                    // Heavy cartoon squish upon landing
                    seq.Insert(0.48f, loseDice.rectTransform.DOPunchScale(new Vector3(0.22f, -0.22f, 0f), 0.42f, 6, 0.5f));

                    // Hypnotic dizzy sway
                    seq.InsertCallback(0.85f, () =>
                    {
                        loseWobbleTween = loseDice.rectTransform.DOLocalRotate(new Vector3(0, 0, 7.5f), 0.72f)
                            .From(new Vector3(0, 0, -7.5f))
                            .SetLoops(-1, LoopType.Yoyo)
                            .SetEase(Ease.InOutSine)
                            .SetUpdate(true);
                    });
                }

                if (loseStars != null)
                {
                    loseStars.localScale = Vector3.zero;
                    seq.Insert(0.35f, loseStars.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack, 1.2f));

                    seq.InsertCallback(0.70f, () =>
                    {
                        starsRotateTween = loseStars.DOLocalRotate(new Vector3(0, 0, 360f), 2.6f, RotateMode.FastBeyond360)
                            .SetLoops(-1, LoopType.Restart)
                            .SetEase(Ease.Linear)
                            .SetUpdate(true);
                    });
                }
            }

            // 7. Result Title Scale Pop
            if (resultTitle != null)
            {
                resultTitle.rectTransform.localScale = Vector3.zero;
                seq.Insert(0.36f, resultTitle.rectTransform.DOScale(Vector3.one, 0.34f).SetEase(Ease.OutBack, 1.35f));
            }

            // 8. Return Home Button Pop & Gentle Breathe
            if (returnHomeButton != null)
            {
                returnHomeButton.transform.localScale = Vector3.zero;
                seq.Insert(0.46f, returnHomeButton.transform.DOScale(Vector3.one, 0.38f).SetEase(Ease.OutBack, 1.25f));

                seq.InsertCallback(0.95f, () =>
                {
                    buttonBreatheTween = returnHomeButton.transform.DOScale(1.035f, 0.95f)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetEase(Ease.InOutSine)
                        .SetUpdate(true);
                });
            }

            activeSequence = seq;
        }

        /// <summary>
        /// Fills names and avatar sprites from real player data instead of the
        /// authoring placeholders ("shahin ۲۲" / "sohrab ۱").
        /// player1 = the local player, player2 = the opponent.
        /// </summary>
        private void ApplyPlayerIdentities()
        {
            // ── Local player ────────────────────────────────────────────────
            string myName = null;
            var profile = ProfileService.Instance;
            if (profile != null)
                myName = profile.ResolveDisplayNameOrUsername(null);
            if (string.IsNullOrWhiteSpace(myName) &&
                NakamaUserManager.Instance != null && NakamaUserManager.Instance.LoadingFinished)
                myName = NakamaUserManager.Instance.User?.Username;
            if (player1Name != null && !string.IsNullOrWhiteSpace(myName))
                player1Name.text = myName.Trim();

            if (player1Avatar != null && profile != null)
            {
                Sprite mySprite = profile.GetSprite(profile.CurrentAvatarId);
                if (mySprite != null) player1Avatar.sprite = mySprite;
            }

            // ── Opponent ────────────────────────────────────────────────────
            // PlayerPrefs "Opp" is kept fresh by PlayersManager.UpdateOpponentNameCache.
            string oppName = PlayerPrefs.GetString("Opp", "");
            if (player2Name != null && !string.IsNullOrWhiteSpace(oppName))
                player2Name.text = oppName;

            if (player2Avatar != null && profile != null)
            {
                string mySessionId = MultiplayerManager.Instance?.Self?.SessionId;
                var players = PlayersManager.Instance?.Players;
                if (players != null && !string.IsNullOrEmpty(mySessionId))
                {
                    var opponent = players.Find(p => p != null && p.Presence != null &&
                                                     p.Presence.SessionId != mySessionId);
                    if (opponent != null)
                    {
                        Sprite oppSprite = profile.GetSprite(
                            string.IsNullOrEmpty(opponent.AvatarId) ? "avatar_0" : opponent.AvatarId);
                        if (oppSprite != null) player2Avatar.sprite = oppSprite;
                    }
                }
            }
        }

        /// <summary>
        /// Drops both avatars in from above the screen, then celebrates the winner
        /// (bounce loop + golden tint) and dims the loser. Draw keeps both neutral.
        /// </summary>
        private void AnimateAvatars(Sequence seq, bool isWin, bool isDraw)
        {
            if (player1Avatar == null || player2Avatar == null)
                return;

            RectTransform mine = player1Avatar.rectTransform;
            RectTransform theirs = player2Avatar.rectTransform;

            // Remember the authored slots once; Refresh can run again on rematch.
            if (!avatarHomesCaptured)
            {
                player1AvatarHome = mine.anchoredPosition;
                player2AvatarHome = theirs.anchoredPosition;
                avatarHomesCaptured = true;
            }

            float dropHeight = Screen.height;
            mine.anchoredPosition = player1AvatarHome + new Vector2(0f, dropHeight);
            theirs.anchoredPosition = player2AvatarHome + new Vector2(0f, dropHeight);
            mine.localScale = Vector3.one;
            theirs.localScale = Vector3.one;
            mine.localEulerAngles = Vector3.zero;
            theirs.localEulerAngles = Vector3.zero;
            player1Avatar.color = Color.white;
            player2Avatar.color = Color.white;

            seq.Insert(0.20f, mine.DOAnchorPos(player1AvatarHome, 0.5f).SetEase(Ease.OutBack, 1.1f));
            seq.Insert(0.32f, theirs.DOAnchorPos(player2AvatarHome, 0.5f).SetEase(Ease.OutBack, 1.1f));

            if (isDraw)
                return;

            Image winner = isWin ? player1Avatar : player2Avatar;
            Image loser = isWin ? player2Avatar : player1Avatar;
            RectTransform winnerRect = winner.rectTransform;

            // Landing beat, then the celebration.
            seq.Insert(0.88f, winnerRect.DOPunchScale(new Vector3(0.28f, 0.28f, 0f), 0.45f, 7, 0.6f));
            seq.Insert(0.88f, winner.DOColor(new Color(1f, 0.92f, 0.72f), 0.3f));
            seq.Insert(0.90f, loser.DOColor(new Color(0.62f, 0.62f, 0.62f), 0.35f));

            seq.InsertCallback(1.40f, () =>
            {
                winnerAvatarTween = winnerRect.DOScale(1.1f, 0.55f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine)
                    .SetUpdate(true);
            });
        }

        public void ReturnHome()
        {
            buttonBreatheTween?.Kill();
            if (returnHomeButton != null)
            {
                returnHomeButton.interactable = false;
                returnHomeButton.transform.DOKill();
                returnHomeButton.transform.DOPunchScale(new Vector3(-0.08f, -0.08f, 0f), 0.2f).SetUpdate(true);
            }

            if (General.AudioManager.Instance != null)
                General.AudioManager.Instance.PlayClickSound();

            if (CartoonIrisTransitionOverlay.Instance != null)
            {
                CartoonIrisTransitionOverlay.Instance.PlayIrisOut(0.55f, new Vector2(0.5f, 0.46f), () =>
                {
                    SceneManager.LoadScene("2-Home");
                });
            }
            else
            {
                DOVirtual.DelayedCall(0.18f, () =>
                {
                    SceneManager.LoadScene("2-Home");
                }).SetUpdate(true);
            }
        }
    }
}
