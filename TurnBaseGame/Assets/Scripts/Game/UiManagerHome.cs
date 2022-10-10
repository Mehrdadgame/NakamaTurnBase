using System;
using System.Collections;
using Nakama;
using Nakama.Helpers;
using NinjaBattle.Game;
using UnityEngine;

namespace Game
{
    public class UiManagerHome : MonoBehaviour
    {
        public static UiManagerHome instance;

        public GameObject cellLeaderboard;

        public Transform parentPos;

       
        // Start is called before the first frame update
        private void Awake()
        {
            instance = this;
        }

        public void GetLeaderboard()
        {
          // yield return new WaitForSeconds(2);
          Debug.Log(NakamaManager.Instance.Session.Username);
            NakamaLeaderboard.instance.ShowGlobalLeaderboards(NakamaManager.Instance.Session);
        }

      
    }
}