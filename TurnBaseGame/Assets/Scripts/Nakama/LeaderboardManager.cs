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

        // ── Own info bar ──────────────────────────────────────────────────────────
        [Header("Own Info")]
        [SerializeField] private RTLTextMeshPro myRankText;
        [SerializeField] private RTLTextMeshPro myScoreText;

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

        // ─────────────────────────────────────────────────────────────────────────

        private void Start()
        {
            if (weeklyButton  != null) weeklyButton.onClick.AddListener(()  => LoadLeaderboard("weekly"));
            if (monthlyButton != null) monthlyButton.onClick.AddListener(() => LoadLeaderboard("monthly"));

            // Set initial tab color immediately (no tween)
            SetTabColorImmediate(weeklyButton, true);
            SetTabColorImmediate(monthlyButton, false);

            // شروع تایمر reset
            if (_timerCo != null) StopCoroutine(_timerCo);
            _timerCo = StartCoroutine(UpdateResetTimers());

            LoadLeaderboard("weekly");
        }

        private void OnDisable()
        {
            if (_timerCo != null) { StopCoroutine(_timerCo); _timerCo = null; }
        }

        private void SetTabColorImmediate(Button btn, bool active)
        {
            if (btn == null) return;
            var img = btn.GetComponent<Image>();
            if (img != null) img.color = active ? tabActiveColor : tabInactiveColor;
            var rect = btn.GetComponent<RectTransform>();
            if (rect != null) rect.localScale = active ? Vector3.one * 1.08f : Vector3.one;
        }

        private void OnEnable()
        {
            LoadLeaderboard(_currentType);
        }

        // ── Load ──────────────────────────────────────────────────────────────────

        private async void LoadLeaderboard(string type)
        {
            _currentType = type;
            UpdateTabVisual(type);
            UpdateResetTimerText(); // فوری تایمر رو با تب جدید آپدیت کن

            try
            {
                var result = await NakamaManager.Instance.SendRPC(
                    GetLeaderboardRpc,
                    "{\"type\":\"" + type + "\",\"limit\":100}"
                );
                var data = result.Payload.Deserialize<LeaderboardResponse>();
                if (data == null) return;

                // جدول جوایز رو از سرور ذخیره کن
                _currentRewards = (data.rewards != null && data.rewards.Length > 0)
                    ? data.rewards
                    : System.Array.Empty<long>();

                BuildPodium(data.records);
                BuildContextList(data.records, data.ownRecord);
                UpdateOwnBar(data.ownRecord);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[LeaderboardManager] " + e.Message);
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
            rect.DOScale(active ? 1.08f : 1f, 0.18f).SetEase(Ease.OutBack);
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
            root.SetActive(hasPlayer);
            if (!hasPlayer) return;

            var rec = records[index];
            if (avatarImg != null) avatarImg.sprite = GetSprite(rec.avatarId);
            if (nameText != null) nameText.text = rec.username ?? "???";
            //  if (scoreText != null) scoreText.text = PersianTextUtils.FormatNumber(rec.score) + " دایسو";
            // جایزه رتبه — index همون رتبه است (0=رتبه۱, 1=رتبه۲, 2=رتبه۳)
            if (rewardText != null)
                rewardText.text = "تاسی :" + PersianTextUtils.FormatNumber(rewards[index]);
        }

        // ── Context list ──────────────────────────────────────────────────────────

        private void BuildContextList(List<LeaderboardRecord> all, LeaderboardRecord own)
        {
            if (rowContainer == null || rowPrefab == null) return;
            foreach (Transform child in rowContainer) Destroy(child.gameObject);
            if (all == null || all.Count == 0) return;

            var slice = GetContextSlice(all, own);
            string myId = NakamaUserManager.Instance != null && NakamaUserManager.Instance.User != null
                ? NakamaUserManager.Instance.User.Id : "";

            int index = 0;
            foreach (var rec in slice)
            {
                var go  = Instantiate(rowPrefab, rowContainer);
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

                    bool isMe = !string.IsNullOrEmpty(myId) && rec.ownerId == myId;
                    if (row.rowBackground != null)
                        row.rowBackground.color = isMe
                            ? new Color(1f, 0.92f, 0.4f, 0.55f)
                            : Color.white;
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
        }

        /// <summary>
        /// Returns up to 9 records centred on the player:
        ///   4 above, self, 4 below — clamped at list boundaries.
        /// Edge cases: rank 1/2/3/4 always starts from index 0.
        /// </summary>
        private List<LeaderboardRecord> GetContextSlice(
            List<LeaderboardRecord> all, LeaderboardRecord own)
        {
            if (own == null) return all.Count <= 9 ? all : all.GetRange(0, 9);

            // Find the player's index in the sorted list
            int myIndex = -1;
            for (int i = 0; i < all.Count; i++)
            {
                if (all[i].ownerId == own.ownerId) { myIndex = i; break; }
            }
            if (myIndex < 0) return all.Count <= 9 ? all : all.GetRange(0, 9);

            int above = 4;
            int below = 4;

            int start = Mathf.Max(0, myIndex - above);
            int end   = Mathf.Min(all.Count - 1, myIndex + below);

            // Expand to always show 9 records when possible
            int count = end - start + 1;
            if (count < 9)
            {
                if (start == 0)
                    end = Mathf.Min(all.Count - 1, 8);
                else
                    start = Mathf.Max(0, end - 8);
            }

            return all.GetRange(start, end - start + 1);
        }

        private void AnimateRowIn(GameObject go, int index)
        {
            var cg = go.GetComponent<CanvasGroup>();
            if (cg == null) cg = go.AddComponent<CanvasGroup>();

            cg.alpha = 0f;
            cg.DOFade(1f, 0.25f).SetDelay(index * 0.06f);
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
            var wait = new WaitForSeconds(1f);
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
                resetTimerText.text = "ریست ماهانه: " + FormatTimeLeft(GetNextMonthlyReset(now));
            else
                resetTimerText.text = "ریست هفتگی: " + FormatTimeLeft(GetNextWeeklyReset(now));
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
