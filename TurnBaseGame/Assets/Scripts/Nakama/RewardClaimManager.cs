using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nakama.Helpers
{
    [Serializable]
    public class PendingRewardData
    {
        public int    rank;
        public int    reward;
        public string type;      // "weekly" | "monthly"
        public bool   claimed;
    }

    [Serializable]
    public class PendingRewardsResponse
    {
        public PendingRewardData weekly;
        public PendingRewardData monthly;
    }

    /// <summary>
    /// On login, checks for unclaimed leaderboard rewards and shows a popup.
    /// Coins are already in the wallet (awarded during server-side leaderboard reset).
    /// This popup only informs the player of their reward.
    ///
    /// Inspector: assign popupPanel, rankText, rewardText, typeText, closeButton.
    /// </summary>
    public class RewardClaimManager : MonoBehaviour
    {
        #region FIELDS

        private const string CheckPendingRewardsRpc = "CheckPendingRewardsRpc";

        [SerializeField] private GameObject      popupPanel;
        [SerializeField] private TextMeshProUGUI typeText;     // "Weekly Leaderboard" / "Monthly..."
        [SerializeField] private TextMeshProUGUI rankText;
        [SerializeField] private TextMeshProUGUI rewardText;
        [SerializeField] private Button          closeButton;

        private PendingRewardData[] pendingQueue = null;
        private int                 queueIndex   = 0;

        #endregion

        #region UNITY

        private void Start()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(OnClose);

            if (popupPanel != null)
                popupPanel.SetActive(false);

            StartCoroutine(CheckAfterLogin());
        }

        #endregion

        #region FLOW

        private IEnumerator CheckAfterLogin()
        {
            // Wait for NakamaUserManager to finish loading
            while (NakamaUserManager.Instance == null || !NakamaUserManager.Instance.LoadingFinished)
                yield return null;

            CheckRewardsAsync();
        }

        private async void CheckRewardsAsync()
        {
            try
            {
                var result = await NakamaManager.Instance.SendRPC(CheckPendingRewardsRpc, "{}");
                var data   = result.Payload.Deserialize<PendingRewardsResponse>();
                if (data == null) return;

                // Build queue of unclaimed rewards to show one at a time
                var queue = new System.Collections.Generic.List<PendingRewardData>();
                if (data.weekly  != null && !data.weekly.claimed)  queue.Add(data.weekly);
                if (data.monthly != null && !data.monthly.claimed) queue.Add(data.monthly);

                if (queue.Count == 0) return;

                pendingQueue = queue.ToArray();
                queueIndex   = 0;

                // Refresh wallet to show updated coins
                if (WalletManager.Instance != null)
                    await WalletManager.Instance.RefreshAsync();

                ShowNext();
            }
            catch (Exception e)
            {
                Debug.LogWarning("CheckPendingRewardsRpc failed: " + e.Message);
            }
        }

        private void ShowNext()
        {
            if (pendingQueue == null || queueIndex >= pendingQueue.Length)
            {
                if (popupPanel != null) popupPanel.SetActive(false);
                return;
            }

            var reward = pendingQueue[queueIndex];

            string leagueName = reward.type == "monthly" ? "Monthly Leaderboard" : "Weekly Leaderboard";
            if (typeText   != null) typeText.text   = leagueName + " Finished!";
            if (rankText   != null) rankText.text   = "Rank: #" + reward.rank;
            if (rewardText != null) rewardText.text = "Reward: " + reward.reward + " HXD";

            if (popupPanel != null) popupPanel.SetActive(true);
        }

        private void OnClose()
        {
            queueIndex++;
            ShowNext();
        }

        #endregion
    }
}
