using DG.Tweening;
using Game;
using NinjaBattle.Game;
using RTLTMPro;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nakama.Helpers
{
    public class MultiplayerJoinMatchButton : MonoBehaviour
    {
        #region FIELDS

        [SerializeField] private Button button;
        [SerializeField] private GameObject shopPanelToOpen;

        [Header("Coin Animation")]
        [SerializeField] private TextMeshProUGUI costPopupText;   // optional: shows "-50 Coin"
        [SerializeField] private RectTransform   costPopupRect;   // optional: same object, floats up

        [Header("Insufficient Coins Popup")]
        [SerializeField] private GameObject insufficientCoinsPopup;
        [SerializeField] private RTLTextMeshPro insufficientCoinsText;
        [SerializeField] private Button buyCoinsButton;
        [SerializeField] private Button watchAdButton;
        [SerializeField] private Button closePopupButton;

        private bool _isJoining;

        #endregion

        #region UNITY

        private void Awake()
        {
            if (button != null)
                button.onClick.AddListener(OnClick);

            if (buyCoinsButton != null)
                buyCoinsButton.onClick.AddListener(OnBuyCoinsClicked);
            if (watchAdButton != null)
                watchAdButton.onClick.AddListener(OnWatchAdClicked);
            if (closePopupButton != null)
                closePopupButton.onClick.AddListener(HideInsufficientCoinsPopup);

            if (insufficientCoinsPopup != null)
                insufficientCoinsPopup.SetActive(false);
        }

        private void OnEnable()
        {
            _isJoining = false;
            if (button != null)
                button.interactable = true;
            if (insufficientCoinsPopup != null)
                insufficientCoinsPopup.SetActive(false);
        }

        private void OnDestroy()
        {
            if (button != null)
                button.onClick.RemoveListener(OnClick);

            if (buyCoinsButton != null)
                buyCoinsButton.onClick.RemoveListener(OnBuyCoinsClicked);
            if (watchAdButton != null)
                watchAdButton.onClick.RemoveListener(OnWatchAdClicked);
            if (closePopupButton != null)
                closePopupButton.onClick.RemoveListener(HideInsufficientCoinsPopup);
        }

        #endregion

        #region CLICK

        private void OnClick()
        {
            if (_isJoining)
                return;

            var modeSetter = GetComponent<SetModeGame>();
            if (modeSetter == null)
            {
                Debug.LogWarning("[MultiplayerJoinMatchButton] SetModeGame component is missing.");
                return;
            }

            var mode = modeSetter.modeGame;
            int fee = ClientLeagues.Get(mode).entryFee;
            int coins = WalletManager.Instance != null ? WalletManager.Instance.Coins : 0;

            if (fee > 0 && coins < fee)
            {
                ShowInsufficientCoinsPopup(coins, fee);
                return;
            }

            GameManager.Instance.modeGame = mode;
            if (button != null)
                button.interactable = false;
            _isJoining = true;

            PlayCoinAnimation(fee, () => MultiplayerManager.Instance.JoinMatchAsync(mode));
        }

        private void ShowInsufficientCoinsPopup(int currentCoins, int requiredCoins)
        {
            if (insufficientCoinsText != null)
            {
                insufficientCoinsText.text =
                    $"مقدار کافی تاسی نداری {requiredCoins}, شما دارید {currentCoins} تاسی. یا تاسی بخر یا تبلغ ببین.";
            }

            if (insufficientCoinsPopup != null)
                insufficientCoinsPopup.SetActive(true);
        }

        private void HideInsufficientCoinsPopup()
        {
            if (insufficientCoinsPopup != null)
                insufficientCoinsPopup.SetActive(false);
        }

        private void OnBuyCoinsClicked()
        {
            Debug.Log("[MultiplayerJoinMatchButton] Buy Coins clicked. Hook your shop flow here.");
            if (shopPanelToOpen != null)
                shopPanelToOpen.SetActive(true);
            HideInsufficientCoinsPopup();
        }

        private void OnWatchAdClicked()
        {
            Debug.Log("[MultiplayerJoinMatchButton] Watch Ad clicked. Integrate rewarded ads SDK here.");
            HideInsufficientCoinsPopup();
        }

        #endregion

        #region ANIMATION

        private void PlayCoinAnimation(int fee, System.Action onDone)
        {
            var coinText   = UiManagerHome.instance != null ? UiManagerHome.instance.Cointext : null;
            int startCoins = WalletManager.Instance != null ? WalletManager.Instance.Coins : 0;
            int endCoins   = Mathf.Max(0, startCoins - fee);

            // ── Coin counter counts down over 1 second ────────────────────────
            float counter = startCoins;
            var countTween = DOTween.To(
                () => counter,
                x =>
                {
                    counter = x;
                    if (coinText != null)
                        coinText.text = Mathf.RoundToInt(x).ToString();
                },
                endCoins,
                1.0f
            ).SetEase(Ease.OutQuad);

            // ── Popup "-N Coin" pops in, floats up, fades out ─────────────────
            if (costPopupText != null && fee > 0)
            {
                costPopupText.text  = "-" + fee + " Coin";
                costPopupText.color = new Color(1f, 0.35f, 0.35f, 1f);
                costPopupText.gameObject.SetActive(true);

                Vector2 startPos = costPopupRect != null ? costPopupRect.anchoredPosition : Vector2.zero;

                costPopupText.transform.localScale = Vector3.one * 0.4f;

                var seq = DOTween.Sequence();
                seq.Append(costPopupText.transform.DOScale(1.2f, 0.18f).SetEase(Ease.OutBack));
                seq.Append(costPopupText.transform.DOScale(1f,   0.08f));
                if (costPopupRect != null)
                    seq.Join(costPopupRect.DOAnchorPosY(startPos.y + 60f, 0.6f).SetEase(Ease.OutQuad));
                seq.AppendInterval(0.1f);
                seq.Append(costPopupText.DOFade(0f, 0.25f));
                seq.OnComplete(() =>
                {
                    costPopupText.gameObject.SetActive(false);
                    if (costPopupRect != null)
                        costPopupRect.anchoredPosition = startPos;
                });
            }

            // ── After 1 second animation, join the match ──────────────────────
            countTween.OnComplete(() => onDone?.Invoke());
        }

        #endregion
    }
}
