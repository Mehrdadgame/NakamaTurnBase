using System.Collections;
using DG.Tweening;
using Nakama.Helpers;
using NinjaBattle.Game;
using RTLTMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// وقتی نت خود بازیکن توی بازی قطع شد:
///   - بلافاصله NakamaDisconnectedChangeScene رو غیرفعال می‌کنه
///   - پاپ‌آپ "اینترنت ضعیف — ۲۰ ثانیه وقت داری" نشون میده
///   - هر ۲ ثانیه تلاش برای re-login
///   - اگه موفق شد یا تایمر تموم شد → wallet refresh → صفحه هوم
///
/// Inspector wiring:
///   popupPanel    — پنل پاپ‌آپ
///   canvasGroup   — CanvasGroup برای fade
///   countdownText — عدد تایمر
///   messageText   — متن توضیح
/// </summary>
public class SelfReconnectHandler : MonoBehaviour
{
    public static SelfReconnectHandler Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject     popupPanel;
    [SerializeField] private CanvasGroup    canvasGroup;
    [SerializeField] private RTLTextMeshPro countdownText;
    [SerializeField] private RTLTextMeshPro messageText;

    [Header("Settings")]
    [SerializeField] private int   graceSeconds  = 20;
    [SerializeField] private float retryInterval = 2f;
    [SerializeField] private string homeSceneName = "Home";

    private NakamaDisconnectedChangeScene _sceneChanger;
    private Coroutine _graceCoroutine;
    private bool      _isHandling;

    // ── Unity ─────────────────────────────────────────────────────────────────

    private void Awake()
    {
        Instance = this;
        // NakamaDisconnectedChangeScene رو پیدا کن و غیرفعال کن
        // تا در بازی بلافاصله صفحه رو عوض نکنه
        _sceneChanger = FindObjectOfType<NakamaDisconnectedChangeScene>();
        if (_sceneChanger != null) _sceneChanger.enabled = false;
    }

    private void Start()
    {
        if (popupPanel != null) popupPanel.SetActive(false);
        NakamaManager.Instance.onDisconnected += OnSelfDisconnected;
    }

    private void OnDestroy()
    {
        if (NakamaManager.Instance != null)
            NakamaManager.Instance.onDisconnected -= OnSelfDisconnected;

        // وقتی صحنه بازی بسته میشه، sceneChanger رو دوباره فعال کن
        if (_sceneChanger != null) _sceneChanger.enabled = true;
    }

    // ── Disconnect ────────────────────────────────────────────────────────────

    private void OnSelfDisconnected()
    {
        if (_isHandling) return;

        // فقط وقتی توی مچ بودیم handle کن
        if (MultiplayerManager.Instance == null || !MultiplayerManager.Instance.IsOnMatch)
        {
            GoHome();
            return;
        }

        _isHandling = true;

        if (TimerTurn.instance != null)
            TimerTurn.instance.TimerPause = true;

        ShowPopup();

        if (_graceCoroutine != null) StopCoroutine(_graceCoroutine);
        _graceCoroutine = StartCoroutine(GraceCoroutine());
    }

    // ── Grace coroutine ───────────────────────────────────────────────────────

    private IEnumerator GraceCoroutine()
    {
        int remaining = graceSeconds;
        float retryTimer = 0f;

        while (remaining > 0)
        {
            SetCountdown(remaining);
            yield return new WaitForSeconds(1f);
            remaining--;
            retryTimer += 1f;

            // هر retryInterval ثانیه یه بار تلاش برای re-login
            if (retryTimer >= retryInterval)
            {
                retryTimer = 0f;
                if (NakamaManager.Instance.IsLoggedIn)
                {
                    // نت برگشت
                    OnReconnected();
                    yield break;
                }
                else
                {
                    NakamaManager.Instance.LoginWithUdid();
                }
            }
        }

        // ۲۰ ثانیه تموم شد
        SetCountdown(0);
        yield return new WaitForSeconds(0.3f);
        OnTimeout();
    }

    private void OnReconnected()
    {
        _isHandling = false;
        HidePopup();
        StartCoroutine(RefreshAndGoHome());
    }

    private void OnTimeout()
    {
        _isHandling = false;
        HidePopup();
        StartCoroutine(RefreshAndGoHome());
    }

    private IEnumerator RefreshAndGoHome()
    {
        if (WalletManager.Instance != null)
        {
            var task = WalletManager.Instance.RefreshAsync();
            yield return new WaitUntil(() => task.IsCompleted);
        }
        GoHome();
    }

    private void GoHome()
    {
        SceneManager.LoadScene(homeSceneName);
    }

    // ── UI ────────────────────────────────────────────────────────────────────

    private void ShowPopup()
    {
        if (popupPanel == null) return;
        if (messageText != null)
            messageText.text = "اینترنت قطع شد!\nبرای ادامه بازی اتصال را برقرار کنید...";

        popupPanel.SetActive(true);
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.DOFade(1f, 0.25f);
        }
    }

    private void HidePopup()
    {
        if (popupPanel == null) return;
        if (canvasGroup != null)
            canvasGroup.DOFade(0f, 0.2f).OnComplete(() => popupPanel.SetActive(false));
        else
            popupPanel.SetActive(false);
    }

    private void SetCountdown(int seconds)
    {
        if (countdownText != null)
            countdownText.text = PersianTextUtils.ToPersianDigits(seconds.ToString());
    }
}
