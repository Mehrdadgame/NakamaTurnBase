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
    [SerializeField] private string homeSceneName;

    private NakamaDisconnectedChangeScene _sceneChanger;
    private Coroutine _graceCoroutine;
    private bool      _isHandling;
    private bool _wasInBattle;
    private string _savedMatchId;  // cached so we can rejoin after reconnect

    // ── Unity ─────────────────────────────────────────────────────────────────

    private void Awake()
    {
        Instance = this;
        _sceneChanger = FindObjectOfType<NakamaDisconnectedChangeScene>();
        if (_sceneChanger != null) _sceneChanger.enabled = false;
    }

    private void Start()
    {
        if (popupPanel != null) popupPanel.SetActive(false);
        NakamaManager.Instance.onDisconnected += OnSelfDisconnected;

        if (MultiplayerManager.Instance != null)
        {
            _wasInBattle = MultiplayerManager.Instance.IsOnMatch;
            _savedMatchId = MultiplayerManager.Instance.CurrentMatchId;
            MultiplayerManager.Instance.onMatchJoin += OnMatchJoined;
            MultiplayerManager.Instance.onMatchLeave += OnMatchLeft;
        }
    }

    private void OnMatchJoined()
    {
        _wasInBattle = true;
        if (MultiplayerManager.Instance != null)
            _savedMatchId = MultiplayerManager.Instance.CurrentMatchId;
    }

    private void OnMatchLeft()
    {
        // Only clear when WE intentionally leave — not on disconnect
        // (disconnect sets match=null before our handler runs, so we ignore it here)
        if (!_isHandling) _wasInBattle = false;
    }

    private void OnDestroy()
    {
        if (NakamaManager.Instance != null)
            NakamaManager.Instance.onDisconnected -= OnSelfDisconnected;

        if (MultiplayerManager.Instance != null)
            MultiplayerManager.Instance.onMatchLeave -= OnMatchLeft;

        if (_sceneChanger != null) _sceneChanger.enabled = true;
    }

    // ── Disconnect ────────────────────────────────────────────────────────────

    private void OnSelfDisconnected()
    {
        if (_isHandling) return;

        // اگه این کامپوننت در صحنه‌ی بازی هست، یعنی توی مسابقه‌ایم — همیشه پاپ‌آپ نشون بده
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

            if (retryTimer >= retryInterval)
            {
                retryTimer = 0f;
                if (NakamaManager.Instance.IsLoggedIn)
                {
                    // نت برگشت → سعی کن به match قبلی برگردی
                    yield return StartCoroutine(TryRejoinAndContinue());
                    yield break;
                }
                else
                {
                    NakamaManager.Instance.LoginWithUdid();
                }
            }
        }

        // ۲۰ ثانیه تموم شد → سرور برنده رو اعلام کرده
        SetCountdown(0);
        yield return new WaitForSeconds(0.3f);
        OnTimeout();
    }

    private IEnumerator TryRejoinAndContinue()
    {
        if (string.IsNullOrEmpty(_savedMatchId) || MultiplayerManager.Instance == null)
        {
            OnTimeout();
            yield break;
        }

        var task = MultiplayerManager.Instance.RejoinMatchAsync(_savedMatchId);
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Result)
        {
            // موفقیت — ادامه بازی
            _isHandling = false;
            HidePopup();
            if (TimerTurn.instance != null)
                TimerTurn.instance.TimerPause = false;
        }
        else
        {
            // rejoin شکست خورد (احتمالاً grace تموم شده)
            OnTimeout();
        }
    }

    private void OnTimeout()
    {
        _isHandling = false;
        if (messageText != null)
            messageText.text = "زمان تموم شد!\nباختی...";
        SetCountdown(0);
        StartCoroutine(ShowLossAndGoHome());
    }

    private IEnumerator ShowLossAndGoHome()
    {
        // ۲ ثانیه پیام "باختی" رو نشون بده
        yield return new WaitForSeconds(2f);
        HidePopup();

        if (WalletManager.Instance != null)
        {
            var task = WalletManager.Instance.RefreshAsync();
            yield return new WaitUntil(() => task.IsCompleted);
        }
        GoHome();
    }

    private void GoHome()
    {
        _wasInBattle = false;
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
