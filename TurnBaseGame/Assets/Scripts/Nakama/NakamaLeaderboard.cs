using System;
using System.Linq;
using Game;
using Nakama.Snippets;
using UnityEngine;

namespace Nakama
{
    
    public class NakamaLeaderboard : MonoBehaviour
    {
        [SerializeField] private int recordsPerPage = 10;
        private const string LeaderboardId = "b7c182b36521Win";

        public static NakamaLeaderboard instance;

        // Start is called before the first frame update
        private void Awake()
        {
            instance = this;
        }


        public async void ShowGlobalLeaderboards(ISession session)
        {
            Debug.Log(session.Username +" Username");
            var result = await NakamaManager.Instance.Client.ListLeaderboardRecordsAsync(session, LeaderboardId);
            Debug.Log(result.Records + "   rrrr");
            foreach (var r in result.Records)
            {
                var cell= Instantiate(UiManagerHome.instance.cellLeaderboard,UiManagerHome.instance.parentPos);
                Debug.Log("{0}{1} r.Username, r.Score");
            }
        }
    }
}