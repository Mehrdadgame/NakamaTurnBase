using System;
using System.Collections.Generic;
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
    }

    [Serializable]
    public class LeaderboardResponse
    {
        public List<LeaderboardRecord> records;
        public LeaderboardRecord       ownRecord;
    }

    /// <summary>
    /// Fetches and displays Weekly/Monthly leaderboards.
    /// Inspector: weeklyButton, monthlyButton, rowContainer, rowPrefab (needs
    ///   rankText, nameText, scoreText children), myRankText.
    /// </summary>
    public class LeaderboardManager : MonoBehaviour
    {
        #region FIELDS

        private const string GetLeaderboardRpc = "GetLeaderboardRpc";

        [SerializeField] private Button          weeklyButton;
        [SerializeField] private Button          monthlyButton;
        [SerializeField] private Transform       rowContainer;
        [SerializeField] private GameObject      rowPrefab;
        [SerializeField] private TextMeshProUGUI myRankText;
        [SerializeField] private TextMeshProUGUI myScoreText;

        private string currentType = "weekly";

        #endregion

        #region UNITY

        private void Start()
        {
            if (weeklyButton  != null) weeklyButton.onClick.AddListener(()  => LoadLeaderboard("weekly"));
            if (monthlyButton != null) monthlyButton.onClick.AddListener(() => LoadLeaderboard("monthly"));

            LoadLeaderboard("weekly");
        }

        private void OnEnable()
        {
            LoadLeaderboard(currentType);
        }

        #endregion

        #region LOAD

        private async void LoadLeaderboard(string type)
        {
            currentType = type;

            var payload = "{\"type\":\"" + type + "\",\"limit\":100}";
            try
            {
                var result = await NakamaManager.Instance.SendRPC(GetLeaderboardRpc, payload);
                var data   = result.Payload.Deserialize<LeaderboardResponse>();
                if (data == null) return;

                BuildUI(data.records);

                if (myRankText  != null && data.ownRecord != null)
                    myRankText.text = "My Rank: #" + data.ownRecord.rank;
                if (myScoreText != null && data.ownRecord != null)
                    myScoreText.text = data.ownRecord.score + " RP";
            }
            catch (Exception e)
            {
                Debug.LogWarning("GetLeaderboardRpc failed: " + e.Message);
            }
        }

        private void BuildUI(List<LeaderboardRecord> records)
        {
            if (rowContainer == null || rowPrefab == null) return;

            foreach (Transform child in rowContainer) Destroy(child.gameObject);
            if (records == null) return;

            for (int i = 0; i < records.Count; i++)
            {
                var rec  = records[i];
                var row  = Instantiate(rowPrefab, rowContainer);
                var texts = row.GetComponentsInChildren<TextMeshProUGUI>();

                // Expects 3 TextMeshProUGUI children: rank, name, score
                if (texts.Length >= 1) texts[0].text = "#" + (i + 1);
                if (texts.Length >= 2) texts[1].text = rec.username;
                if (texts.Length >= 3) texts[2].text = rec.score + " RP";

                // Highlight caller's own row
                if (NakamaUserManager.Instance != null &&
                    rec.ownerId == NakamaUserManager.Instance.User.Id)
                {
                    var img = row.GetComponent<Image>();
                    if (img != null) img.color = new Color(1f, 0.92f, 0.4f, 0.5f);
                }
            }
        }

        #endregion
    }
}
