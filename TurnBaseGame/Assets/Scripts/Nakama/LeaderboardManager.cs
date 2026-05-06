using System;
using System.Collections.Generic;
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
    }

    public class LeaderboardManager : MonoBehaviour
    {
        private const string GetLeaderboardRpc = "GetLeaderboardRpc";

        // ── Top-3 Podium ──────────────────────────────────────────────────────────
        [Header("Top 3 Podium")]
        [SerializeField] private GameObject      pod1Root,   pod2Root,   pod3Root;
        [SerializeField] private Image           pod1Avatar, pod2Avatar, pod3Avatar;
        [SerializeField] private RTLTextMeshPro  pod1Name,   pod2Name,   pod3Name;
        [SerializeField] private TextMeshProUGUI pod1Score,  pod2Score,  pod3Score;

        // ── Context list ──────────────────────────────────────────────────────────
        [Header("Context List  (4 above + self + 4 below)")]
        [SerializeField] private Transform  rowContainer;
        [SerializeField] private GameObject rowPrefab;      // must have LeaderboardRowUI

        // ── Own info bar ──────────────────────────────────────────────────────────
        [Header("Own Info")]
        [SerializeField] private RTLTextMeshPro  myRankText;
        [SerializeField] private TextMeshProUGUI myScoreText;

        // ── Avatar sprites ────────────────────────────────────────────────────────
        [Header("Avatar Library")]
        [SerializeField] private AvatarLibrary avatarLibrary;

        // ── Tab buttons ───────────────────────────────────────────────────────────
        [Header("Tabs")]
        [SerializeField] private Button weeklyButton;
        [SerializeField] private Button monthlyButton;

        private string _currentType = "weekly";

        // ─────────────────────────────────────────────────────────────────────────

        private void Start()
        {
            if (weeklyButton  != null) weeklyButton.onClick.AddListener(()  => LoadLeaderboard("weekly"));
            if (monthlyButton != null) monthlyButton.onClick.AddListener(() => LoadLeaderboard("monthly"));
            LoadLeaderboard("weekly");
        }

        private void OnEnable()
        {
            LoadLeaderboard(_currentType);
        }

        // ── Load ──────────────────────────────────────────────────────────────────

        private async void LoadLeaderboard(string type)
        {
            _currentType = type;

            try
            {
                var result = await NakamaManager.Instance.SendRPC(
                    GetLeaderboardRpc,
                    "{\"type\":\"" + type + "\",\"limit\":100}"
                );
                var data = result.Payload.Deserialize<LeaderboardResponse>();
                if (data == null) return;

                BuildPodium(data.records);
                BuildContextList(data.records, data.ownRecord);
                UpdateOwnBar(data.ownRecord);
            }
            catch (Exception e)
            {
                Debug.LogWarning("[LeaderboardManager] " + e.Message);
            }
        }

        // ── Podium (top 3) ────────────────────────────────────────────────────────

        private void BuildPodium(List<LeaderboardRecord> records)
        {
            // Layout: 2nd left, 1st centre, 3rd right  (matches screenshot)
            SetPodiumSlot(pod2Root, pod2Avatar, pod2Name, pod2Score, records, 1); // rank 2
            SetPodiumSlot(pod1Root, pod1Avatar, pod1Name, pod1Score, records, 0); // rank 1
            SetPodiumSlot(pod3Root, pod3Avatar, pod3Name, pod3Score, records, 2); // rank 3
        }

        private void SetPodiumSlot(
            GameObject root, Image avatarImg,
            RTLTextMeshPro nameText, TextMeshProUGUI scoreText,
            List<LeaderboardRecord> records, int index)
        {
            if (root == null) return;
            bool hasPlayer = records != null && records.Count > index;
            root.SetActive(hasPlayer);
            if (!hasPlayer) return;

            var rec = records[index];
            if (avatarImg != null) avatarImg.sprite = GetSprite(rec.avatarId);
            if (nameText  != null) nameText.text    = rec.username ?? "???";
            if (scoreText != null) scoreText.text   = rec.score + " RP";
        }

        // ── Context list ──────────────────────────────────────────────────────────

        private void BuildContextList(List<LeaderboardRecord> all, LeaderboardRecord own)
        {
            if (rowContainer == null || rowPrefab == null) return;
            foreach (Transform child in rowContainer) Destroy(child.gameObject);
            if (all == null || all.Count == 0) return;

            var slice = GetContextSlice(all, own);
            string myId = NakamaUserManager.Instance != null
                ? NakamaUserManager.Instance.User.Id : "";

            foreach (var rec in slice)
            {
                var go  = Instantiate(rowPrefab, rowContainer);
                var row = go.GetComponent<LeaderboardRowUI>();

                if (row != null)
                {
                    if (row.avatarImage  != null) row.avatarImage.sprite = GetSprite(rec.avatarId);
                    if (row.rankText     != null) row.rankText.text      = "#" + rec.rank;
                    if (row.nameText     != null) row.nameText.text      = rec.username ?? "???";
                    if (row.scoreText    != null) row.scoreText.text     = rec.score + " RP";

                    bool isMe = !string.IsNullOrEmpty(myId) && rec.ownerId == myId;
                    if (row.rowBackground != null)
                        row.rowBackground.color = isMe
                            ? new Color(1f, 0.92f, 0.4f, 0.55f)
                            : Color.clear;
                }
                else
                {
                    // Fallback for old prefabs without LeaderboardRowUI
                    var texts = go.GetComponentsInChildren<RTLTextMeshPro>();
                    if (texts.Length >= 1) texts[0].text = "#" + rec.rank;
                    if (texts.Length >= 2) texts[1].text = rec.username ?? "???";
                }
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

        // ── Own info bar ──────────────────────────────────────────────────────────

        private void UpdateOwnBar(LeaderboardRecord own)
        {
            if (own == null)
            {
                if (myRankText  != null) myRankText.text  = "شما: -";
                if (myScoreText != null) myScoreText.text = "0 RP";
                return;
            }
            if (myRankText  != null) myRankText.text  = "#" + own.rank;
            if (myScoreText != null) myScoreText.text = own.score + " RP";
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
