using System;
using System.Collections;
using Nakama;
using Nakama.Helpers;
using NinjaBattle.Game;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
namespace Game
{
    public class UiManagerHome : MonoBehaviour
    {
        public static UiManagerHome instance;

        public GameObject cellLeaderboard;

        public Transform parentPos;
        public TextMeshProUGUI TextMeshProUGUI;
        public Image PlayerIcon;
        // Start is called before the first frame update
        private void Awake()
        {
            instance = this;
        }
        private void Start()
        {
            StartCoroutine(GetImagePlayer());
        }

        public void GetLeaderboard()
        {
            // yield return new WaitForSeconds(2);
            Debug.Log(NakamaManager.Instance.Session.Username);
            NakamaLeaderboard.instance.ShowGlobalLeaderboards(NakamaManager.Instance.Session);
        }

        private IEnumerator GetImagePlayer()
        {

            UnityWebRequest www = UnityWebRequestTexture.GetTexture(PlayerPrefs.GetString("Image"));
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.DataProcessingError || www.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.Log(www.error);
            }
            else
            {
                Texture myTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                NakamaManager.Instance.SpritePlayerIcon = Sprite.Create((Texture2D)myTexture, new Rect(0, 0, myTexture.width, myTexture.height), new Vector2(.5f, .5f));
                PlayerIcon.sprite = NakamaManager.Instance.SpritePlayerIcon;
            }


        }


    }


}