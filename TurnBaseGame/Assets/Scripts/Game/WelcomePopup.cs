using DG.Tweening;
using Nakama.Helpers;
using NinjaBattle.Game;
using RTLTMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// نمایش پاپ‌آپ خوش‌آمدگویی در صحنه Home برای بازیکن جدید.
/// اگر tutorial قبلاً skip یا تکمیل شده باشد نمایش داده نمی‌شود.
///
/// Inspector:
///   popupPanel   — پنل اصلی پاپ‌آپ (RectTransform + CanvasGroup)
///   startButton  — دکمه "شروع آموزش"
///   skipButton   — دکمه "رد کردن"
///   titleText    — عنوان
///   descText     — توضیحات
///   statusText   — متن وضعیت (در حال پیدا کردن حریف...)
/// </summary>
public class WelcomePopup : MonoBehaviour
{
    public const string TutorialDoneKey = "TutorialDone";
    public const string TutorialModeKey = "TutorialMode";   // 1 = در حال tutorial
    public const string ForcedModeKey = "ThreeByThree"; // "ThreeByThree"

    [Header("Popup UI")]
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Button startButton;
    [SerializeField] private Button skipButton;
    [SerializeField] private RTLTextMeshPro titleText;
    [SerializeField] private RTLTextMeshPro descText;
    [SerializeField] private RTLTextMeshPro statusText;   // "در حال پیدا کردن حریف..."

    // ─────────────────────────────────────────────────────────────────────────

    private void Start()
    {
        if (popupPanel != null) popupPanel.SetActive(false);

        if (PlayerPrefs.GetInt(TutorialDoneKey, 0) == 0)
            ShowPopup();
    }

    private void ShowPopup()
    {
        if (popupPanel == null) return;

        if (titleText != null) titleText.text = "به بازی تاس زن خوش اومدی! ";
        if (descText != null) descText.text =
            "یه بازی هیجان‌انگیز تاس که هم به سرعت نیاز داره هم استراتژی!\n\n" +
            "می‌خوای اول آموزش ببینی؟";
        if (statusText != null) statusText.gameObject.SetActive(false);

        popupPanel.SetActive(true);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, 0.3f);
        }

        var rect = popupPanel.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.localScale = Vector3.one * 0.7f;
            rect.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack);
        }

        if (startButton != null) startButton.onClick.AddListener(OnStartTutorial);
        if (skipButton != null) skipButton.onClick.AddListener(OnSkip);
    }

    private void OnStartTutorial()
    {
        // جلوگیری از کلیک مجدد
        if (startButton != null) startButton.interactable = false;
        if (skipButton != null) skipButton.interactable = false;

        // نمایش متن «در حال پیدا کردن حریف»
        if (statusText != null)
        {
            statusText.gameObject.SetActive(true);
            statusText.text = "در حال پیدا کردن حریف برای آموزش... 🤖";
        }

        // ذخیره وضعیت tutorial
        PlayerPrefs.SetInt(TutorialModeKey, 1);
        PlayerPrefs.SetString(ForcedModeKey, "ThreeByThree");
        PlayerPrefs.Save();

        // تنظیم mode و شروع matchmaking با بات
        GameManager.Instance.modeGame = ModeGame.VerticalAndHorizontal;
        MultiplayerManager.Instance.JoinTutorialMatchAsync();
        // از اینجا به بعد GameManager.JoinedMatch() → GoToLobby() → ThreeByThree scene
    }

    private void OnSkip()
    {
        PlayerPrefs.SetInt(TutorialDoneKey, 1);
        PlayerPrefs.Save();
        HideAndDo(() => { });
    }

    private void HideAndDo(System.Action callback)
    {
        if (canvasGroup != null)
            canvasGroup.DOFade(0f, 0.2f).OnComplete(() =>
            {
                if (popupPanel != null) popupPanel.SetActive(false);
                callback?.Invoke();
            });
        else
        {
            if (popupPanel != null) popupPanel.SetActive(false);
            callback?.Invoke();
        }
    }
}
