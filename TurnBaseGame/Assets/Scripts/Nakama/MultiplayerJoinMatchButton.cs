using DG.Tweening;
using Game;
using NinjaBattle.Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nakama.Helpers
{
    public class MultiplayerJoinMatchButton : MonoBehaviour
    {
        #region FIELDS

        [SerializeField] private Button button;

        [Header("Coin Animation")]
        [SerializeField] private TextMeshProUGUI costPopupText;   // optional: shows "-50 Coin"
        [SerializeField] private RectTransform   costPopupRect;   // optional: same object, floats up

        #endregion

        #region UNITY

        private void Awake()
        {
            button.onClick.AddListener(OnClick);
        }

        #endregion

        #region CLICK

        private void OnClick()
        {
            var mode = GetComponent<SetModeGame>().modeGame;
            GameManager.Instance.modeGame = mode;
            button.interactable = false;

            int fee = ClientLeagues.Get(mode).entryFee;
            PlayCoinAnimation(fee, () => MultiplayerManager.Instance.JoinMatchAsync(mode));
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
