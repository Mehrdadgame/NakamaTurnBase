using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using RTLTMPro;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nakama.Helpers
{
    [Serializable]
    public class LeaderboardRecord
    {
        public string leaderboardId;
        public string ownerId;
        public string username;
        public long   score;
        public long   rank;
        public string avatarId;
    }

    [Serializable]
    public class LeaderboardResponse
    {
        public List<LeaderboardRecord> records;
        public LeaderboardRecord       ownRecord;
        public long[]                  rewards;   // جدول جوایز از سرور — index 0 = رتبه ۱
    }

    public class LeaderboardManager : MonoBehaviour
    {
        private const string GetLeaderboardRpc = "GetLeaderboardRpc";

        // ── Top-3 Podium ──────────────────────────────────────────────────────────
        [Header("Top 3 Podium")]
        [SerializeField] private GameObject pod1Root, pod2Root, pod3Root;
        [SerializeField] private Image pod1Avatar, pod2Avatar, pod3Avatar;
        [SerializeField] private RTLTextMeshPro pod1Name, pod2Name, pod3Name;
        //  [SerializeField] private RTLTextMeshPro pod1Score, pod2Score, pod3Score;
        [SerializeField] private RTLTextMeshPro pod1Reward, pod2Reward, pod3Reward; // جایزه رتبه ۱/۲/۳

        // ── Context list ──────────────────────────────────────────────────────────
        [Header("Context List  (4 above + self + 4 below)")]
        [SerializeField] private Transform  rowContainer;
        [SerializeField] private GameObject rowPrefab;      // must have LeaderboardRowUI
        [SerializeField] private ScrollRect leaderboardScroll;

        // ── Own info bar ──────────────────────────────────────────────────────────
        [Header("Own Info")]
        [SerializeField] private RTLTextMeshPro myRankText;
        [SerializeField] private RTLTextMeshPro myScoreText;

        [Header("Load State")]
        [SerializeField] private GameObject statePanel;
        [SerializeField] private RTLTextMeshPro stateText;

        // ── Avatar sprites ────────────────────────────────────────────────────────
        [Header("Avatar Library")]
        [SerializeField] private AvatarLibrary avatarLibrary;

        // ── Tab buttons ───────────────────────────────────────────────────────────
        [Header("Tabs")]
        [SerializeField] private Button weeklyButton;
        [SerializeField] private Button monthlyButton;
        [SerializeField] private Color tabActiveColor = new Color(1f, 0.85f, 0.2f, 1f);
        [SerializeField] private Color tabInactiveColor = new Color(0.55f, 0.55f, 0.55f, 1f);

        // ── Reset timer ───────────────────────────────────────────────────────────
        [Header("Reset Timer ")]
        [SerializeField] private RTLTextMeshPro resetTimerText;  // تایمر ریست — هفتگی یا ماهانه بسته به تب فعال

        // جدول جوایز از سرور دریافت میشه — اینجا نگه داشته میشه تا بین BuildPodium و BuildContextList مشترک باشه
        private long[] _currentRewards = System.Array.Empty<long>();

        private string _currentType = "weekly";
        private Coroutine _timerCo;
        private Coroutine _loadWhenReadyCo;
        private int _loadVersion;

        // ─────────────────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (weeklyButton != null) weeklyButton.onClick.AddListener(SelectWeekly);
            if (monthlyButton != null) monthlyButton.onClick.AddListener(SelectMonthly);
            SetTabColorImmediate(weeklyButton, true);
            SetTabColorImmediate(monthlyButton, false);
        }

        private void OnDisable()
        {
            if (_timerCo != null) { StopCoroutine(_timerCo); _timerCo = null; }
            if (_loadWhenReadyCo != null) { StopCoroutine(_loadWhenReadyCo); _loadWhenReadyCo = null; }
            _loadVersion++;
        }

        private void SetTabColorImmediate(Button btn, bool active)
        {
            if (btn == null) return;
            var img = btn.GetComponent<Image>();
            if (img != null) img.color = active ? tabActiveColor : tabInactiveColor;
            var rect = btn.GetComponent<RectTransform>();
            if (rect != null) rect.localScale = Vector3.one;
        }

        private void OnEnable()
        {
            if (_timerCo != null) StopCoroutine(_timerCo);
            _timerCo = StartCoroutine(UpdateResetTimers());
            RefreshCurrent();
        }

        private void OnDestroy()
        {
            if (weeklyButton != null) weeklyButton.onClick.RemoveListener(SelectWeekly);
            if (monthlyButton != null) monthlyButton.onClick.RemoveListener(SelectMonthly);
        }

        public void SelectWeekly() => RequestLeaderboard("weekly");

        public void SelectMonthly() => RequestLeaderboard("monthly");

        public void RefreshCurrent() => RequestLeaderboard(_currentType);

        private void RequestLeaderboard(string type)
        {
            _currentType = type == "monthly" ? "monthly" : "weekly";
            UpdateTabVisual(_currentType);
            UpdateResetTimerText();

            if (!isActiveAndEnabled)
                return;

            if (_loadWhenReadyCo != null)
                StopCoroutine(_loadWhenReadyCo);
            ShowState("در حال دریافت رتبه‌ها...", true);
            _loadWhenReadyCo = StartCoroutine(LoadWhenReady(_currentType));
        }

        private IEnumerator LoadWhenReady(string type)
        {
            float timeoutAt = Time.realtimeSinceStartup + 12f;
            while ((NakamaManager.Instance == null || NakamaManager.Instance.Session == null) &&
                   Time.realtimeSinceStartup < timeoutAt)
                yield return null;

            _loadWhenReadyCo = null;
            if (NakamaManager.Instance == null || NakamaManager.Instance.Session == null)
            {
                Debug.LogWarning("[LeaderboardManager] Nakama session was not ready before timeout.");
                ShowState("اتصال به سرور برقرار نشد\nبرای تلاش دوباره لمس کنید", true);
                yield break;
            }

            LoadLeaderboard(type);
        }

        // ── Load ──────────────────────────────────────────────────────────────────

        private async void LoadLeaderboard(string type)
        {
            int requestVersion = ++_loadVersion;
            _currentType = type;
            UpdateTabVisual(type);
            UpdateResetTimerText(); // فوری تایمر رو با تب جدید آپدیت کن

            try
            {
                if (NakamaManager.Instance == null)
                {
                    ShowState("اتصال به سرور در دسترس نیست", true);
                    return;
                }

                var result = await NakamaManager.Instance.SendRPC(
                    GetLeaderboardRpc,
                    "{\"type\":\"" + type + "\",\"limit\":100}"
                );
                if (requestVersion != _loadVersion || !isActiveAndEnabled)
                    return;
                if (result == null || string.IsNullOrEmpty(result.Payload))
                {
                    Debug.LogWarning("[LeaderboardManager] Empty response for " + type + " leaderboard.");
                    ShowState("پاسخی از سرور دریافت نشد\nبرای تلاش دوباره لمس کنید", true);
                    return;
                }

                var data = result.Payload.Deserialize<LeaderboardResponse>();
                if (data == null)
                {
                    ShowState("اطلاعات جدول قابل خواندن نیست", true);
                    return;
                }

                // جدول جوایز رو از سرور ذخیره کن
                _currentRewards = (data.rewards != null && data.rewards.Length > 0)
                    ? data.rewards
                    : System.Array.Empty<long>();

                var records = data.records ?? new List<LeaderboardRecord>();
                BuildPodium(records);
                BuildContextList(records, data.ownRecord);
                UpdateOwnBar(data.ownRecord);
                ShowState(records.Count == 0 ? "هنوز رتبه‌ای ثبت نشده است" : "", records.Count == 0);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[LeaderboardManager] " + e.Message);
                ShowState("دریافت جدول ناموفق بود\nبرای تلاش دوباره لمس کنید", true);
            }
        }

        private void UpdateTabVisual(string activeType)
        {
            SetTabColor(weeklyButton, activeType == "weekly");
            SetTabColor(monthlyButton, activeType == "monthly");
        }

        private void SetTabColor(Button btn, bool active)
        {
            if (btn == null) return;
            var img = btn.GetComponent<Image>();
            if (img == null) return;
            img.DOColor(active ? tabActiveColor : tabInactiveColor, 0.2f);

            var rect = btn.GetComponent<RectTransform>();
            if (rect == null) return;
            rect.DOPunchScale(Vector3.one * 0.035f, 0.18f, 4, 0.45f);
        }

        // ── Podium (top 3) ────────────────────────────────────────────────────────

        private void BuildPodium(List<LeaderboardRecord> records)
        {
            // Layout: 2nd left, 1st centre, 3rd right  (matches screenshot)
            SetPodiumSlot(pod2Root, pod2Avatar, pod2Name, pod2Reward, records, 1, _currentRewards);
            SetPodiumSlot(pod1Root, pod1Avatar, pod1Name, pod1Reward, records, 0, _currentRewards);
            SetPodiumSlot(pod3Root, pod3Avatar, pod3Name, pod3Reward, records, 2, _currentRewards);
        }

        private void AnimatePodiumSlot(GameObject root, float delay)
        {
            if (root == null || !root.activeSelf) return;
            root.transform.localScale = Vector3.zero;
            root.transform.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack).SetDelay(delay);
        }

        private void SetPodiumSlot(
            GameObject root, Image avatarImg,
            RTLTextMeshPro nameText, RTLTextMeshPro rewardText,
            List<LeaderboardRecord> records, int index, long[] rewards)
        {
            if (root == null) return;
            bool hasPlayer = records != null && records.Count > index;
            // Keep all three podiums on screen, even while the server returns a
            // short/partial table. This avoids the empty top section seen before.
            root.SetActive(true);
            root.transform.localScale = Vector3.one;
            if (!hasPlayer)
            {
                if (avatarImg != null)
                {
                    avatarImg.sprite = null;
                    avatarImg.color = Color.clear;
                }
                if (nameText != null) nameText.text = "رتبه " + (index + 1);
                if (rewardText != null) rewardText.gameObject.SetActive(false);
                return;
            }

            var rec = records[index];
            if (avatarImg != null)
            {
                avatarImg.sprite = GetSprite(rec.avatarId);
                avatarImg.color = Color.white;
            }
            if (nameText != null) nameText.text = rec.username ?? "???";
            //  if (scoreText != null) scoreText.text = PersianTextUtils.FormatNumber(rec.score) + " دایسو";
            // جایزه رتبه — index همون رتبه است (0=رتبه۱, 1=رتبه۲, 2=رتبه۳)
            if (rewardText != null)
            {
                bool hasReward = rewards != null && index >= 0 && index < rewards.Length;
                rewardText.gameObject.SetActive(hasReward);
                if (hasReward)
                    rewardText.text = "تاسی :" + PersianTextUtils.FormatNumber(rewards[index]);
            }
        }

        // ── Context list ──────────────────────────────────────────────────────────

        private void BuildContextList(List<LeaderboardRecord> all, LeaderboardRecord own)
        {
            if (rowContainer == null || rowPrefab == null) return;
            foreach (Transform child in rowContainer) Destroy(child.gameObject);
            if (all == null || all.Count == 0) return;

            // The viewport decides how many entries are visible. Keep all server
            // records in the content so the player can scroll the full ranking.
            var slice = new List<LeaderboardRecord>(all);
            // Some server implementations return a context record separately when
            // the player is outside the requested range. Keep that player visible
            // in the same scrollable list instead of dropping the highlighted row.
            if (own != null && !slice.Exists(record => record.ownerId == own.ownerId))
            {
                int insertAt = Mathf.Clamp((int)own.rank - 1, 0, slice.Count);
                slice.Insert(insertAt, own);
            }
            string myId = NakamaUserManager.Instance != null && NakamaUserManager.Instance.User != null
                ? NakamaUserManager.Instance.User.Id : "";

            int index = 0;
            foreach (var rec in slice)
            {
                var go  = Instantiate(rowPrefab, rowContainer);
                go.SetActive(true);
                var row = go.GetComponent<LeaderboardRowUI>();

                if (row != null)
                {
                    if (row.avatarImage  != null) row.avatarImage.sprite = GetSprite(rec.avatarId);
                    if (row.rankText     != null) row.rankText.text      = "#" + rec.rank;
                    if (row.nameText     != null) row.nameText.text      = rec.username ?? "???";
                    if (row.scoreText != null) row.scoreText.text = PersianTextUtils.FormatNumber(rec.score) + " دایسو";

                    // نمایش جایزه برای رتبه‌های ۱ تا ۱۰ (از سرور)
                    if (row.rewardText != null)
                    {
                        int rankIndex = (int)rec.rank - 1; // rank شروع از 1
                        if (rankIndex >= 0 && rankIndex < _currentRewards.Length)
                        {
                            row.rewardText.gameObject.SetActive(true);
                            row.rewardText.text = "تاسی: " + PersianTextUtils.FormatNumber(_currentRewards[rankIndex]);
                        }
                        else
                        {
                            row.rewardText.gameObject.SetActive(false);
                        }
                    }

                    bool isMe = (own != null && rec.ownerId == own.ownerId) ||
                                (!string.IsNullOrEmpty(myId) && rec.ownerId == myId);
                    if (row.rowBackground != null)
                        row.rowBackground.color = isMe
                            ? new Color32(239, 186, 106, 255)
                            : new Color32(242, 206, 151, 255);
                }
                else
                {
                    // Fallback for prefabs without LeaderboardRowUI — try RTL first, then TMP
                    var rtlTexts = go.GetComponentsInChildren<RTLTextMeshPro>(true);
                    if (rtlTexts.Length >= 1) rtlTexts[0].text = "#" + rec.rank;
                    if (rtlTexts.Length >= 2) rtlTexts[1].text = rec.username ?? "???";
                    if (rtlTexts.Length >= 3) rtlTexts[2].text = PersianTextUtils.FormatNumber(rec.score) + " دایسو";

                    if (rtlTexts.Length == 0)
                    {
                        var tmpTexts = go.GetComponentsInChildren<TextMeshProUGUI>(true);
                        if (tmpTexts.Length >= 1) tmpTexts[0].text = "#" + rec.rank;
                        if (tmpTexts.Length >= 2) tmpTexts[1].text = rec.username ?? "???";
                        if (tmpTexts.Length >= 3) tmpTexts[2].text = rec.score + " دایسو";
                    }
                }

                AnimateRowIn(go, index);
                index++;
            }

            Canvas.ForceUpdateCanvases();
            if (leaderboardScroll != null)
                leaderboardScroll.verticalNormalizedPosition = 1f;
        }

        /// <summary>
        /// Returns up to 9 records centred on the player:
        ///   4 above, self, 4 below — clamped at list boundaries.
        /// Edge cases: rank 1/2/3/4 always starts from index 0.
        /// </summary>
        private List<LeaderboardRecord> GetContextSlice(
            List<LeaderboardRecord> all, LeaderboardRecord own)
        {
            const int visibleCount = 5;
            if (own == null) return all.Count <= visibleCount ? all : all.GetRange(0, visibleCount);

            // Find the player's index in the sorted list
            int myIndex = -1;
            for (int i = 0; i < all.Count; i++)
            {
                if (all[i].ownerId == own.ownerId) { myIndex = i; break; }
            }
            if (myIndex < 0) return all.Count <= 9 ? all : all.GetRange(0, 9);

            int above = 2;
            int below = 2;

            int start = Mathf.Max(0, myIndex - above);
            int end   = Mathf.Min(all.Count - 1, myIndex + below);

            // Expand to always fill the five visible Figma rows when possible.
            int count = end - start + 1;
            if (count < visibleCount)
            {
                if (start == 0)
                    end = Mathf.Min(all.Count - 1, visibleCount - 1);
                else
                    start = Mathf.Max(0, end - (visibleCount - 1));
            }

            return all.GetRange(start, end - start + 1);
        }

        private void AnimateRowIn(GameObject go, int index)
        {
            var cg = go.GetComponent<CanvasGroup>();
            if (cg == null) cg = go.AddComponent<CanvasGroup>();

            cg.alpha = 0f;
            cg.DOFade(1f, 0.25f).SetDelay(Mathf.Min(index, 6) * 0.045f);
        }

        private void ShowState(string message, bool visible)
        {
            if (stateText != null)
                stateText.text = message;
            if (statePanel != null && statePanel.activeSelf != visible)
                statePanel.SetActive(visible);
        }

        // ── Own info bar ──────────────────────────────────────────────────────────

        private void UpdateOwnBar(LeaderboardRecord own)
        {
            if (own == null)
            {
                if (myRankText  != null) myRankText.text  = "شما: -";
                if (myScoreText != null) myScoreText.text = "۰ دایسو";
                return;
            }
            if (myRankText  != null) myRankText.text  = "#" + PersianTextUtils.ToPersianDigits(own.rank.ToString());
            if (myScoreText != null) myScoreText.text = PersianTextUtils.FormatNumber(own.score) + " دایسو";
        }

        // ── Reset Timers ──────────────────────────────────────────────────────────

        /// <summary>
        /// هر ثانیه تایمر ریست هفتگی و ماهانه رو آپدیت می‌کنه.
        /// هفتگی: دوشنبه ۰۰:۰۰ UTC  |  ماهانه: اول ماه ۰۰:۰۰ UTC
        /// </summary>
        private IEnumerator UpdateResetTimers()
        {
            var wait = new WaitForSecondsRealtime(1f);
            while (true)
            {
                UpdateResetTimerText();
                yield return wait;
            }
        }

        /// تایمر تکست رو بر اساس تب فعال آپدیت می‌کنه — هم از coroutine هم موقع تعویض تب
        private void UpdateResetTimerText()
        {
            if (resetTimerText == null) return;
            var now = DateTime.UtcNow;
            if (_currentType == "monthly")
                resetTimerText.text = "ریست ماهانه\n" + FormatTimeLeft(GetNextMonthlyReset(now));
            else
                resetTimerText.text = "ریست هفتگی\n" + FormatTimeLeft(GetNextWeeklyReset(now));
        }

        /// دوشنبه بعدی ساعت ۰۰:۰۰ UTC
        private static DateTime GetNextWeeklyReset(DateTime utcNow)
        {
            // DayOfWeek.Monday = 1
            int daysUntilMonday = ((int)DayOfWeek.Monday - (int)utcNow.DayOfWeek + 7) % 7;
            if (daysUntilMonday == 0 && utcNow.TimeOfDay == TimeSpan.Zero)
                daysUntilMonday = 7; // همین لحظه ریست شده — هفته بعد
            else if (daysUntilMonday == 0)
                daysUntilMonday = 7;
            return utcNow.Date.AddDays(daysUntilMonday);
        }

        /// اول ماه بعدی ساعت ۰۰:۰۰ UTC
        private static DateTime GetNextMonthlyReset(DateTime utcNow)
        {
            return new DateTime(utcNow.Year, utcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1);
        }

        private static string FormatTimeLeft(DateTime target)
        {
            var ts = target - DateTime.UtcNow;
            if (ts.TotalSeconds <= 0) return "به زودی";
            if (ts.TotalDays >= 1)
                return $"{(int)ts.TotalDays}روز {ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
            return $"{ts.Hours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private Sprite GetSprite(string avatarId)
        {
            var lib = avatarLibrary != null ? avatarLibrary : ProfileService.Instance?.AvatarLibrary;
            if (lib == null) return null;
            return lib.GetSprite(string.IsNullOrEmpty(avatarId) ? "avatar_0" : avatarId);
        }
    }
}
