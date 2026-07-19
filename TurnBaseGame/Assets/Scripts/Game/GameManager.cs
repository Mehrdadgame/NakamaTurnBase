using Nakama.Helpers;
using NinjaBattle.General;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NinjaBattle.Game
{
    public class GameManager : MonoBehaviour
    {
        #region FIELDS

        public const int VictoriesRequiredToWin = 3;

        #endregion

        #region PROPERTIES

        public static GameManager Instance { get; private set; } = null;
        public int[] PlayersWins { get; private set; } = new int[4];
        public int? Winner { get; private set; } = 0;

        public DiceRoller diceRoller;
        public ModeGame modeGame;

        [Header("Avatar Library (assign in Inspector — مثل صفحه‌ی هوم)")]
        [SerializeField] private AvatarLibrary avatarLibrary;

        #endregion

        #region BEHAVIORS

        private void Awake()
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Application.targetFrameRate = 60;
        }
        private void Start()
        {
            MultiplayerManager.Instance.Subscribe(MultiplayerManager.Code.ChangeScene, ReceivedChangeScene);
            MultiplayerManager.Instance.onMatchJoin += JoinedMatch;
            MultiplayerManager.Instance.onMatchLeave += LeavedMatch;
        }

        private void OnEnable()
        {
            Instance = this;
            // MultiplayerManager.Instance.Subscribe(MultiplayerManager.Code.PlayerWon, ReceivedPlayerWonRound);
            //  MultiplayerManager.Instance.Subscribe(MultiplayerManager.Code.Draw, ReceivedDrawRound);


        }

        private void OnDestroy()
        {
            MultiplayerManager.Instance.Unsubscribe(MultiplayerManager.Code.PlayerWon, ReceivedPlayerWonRound);
            MultiplayerManager.Instance.Unsubscribe(MultiplayerManager.Code.Draw, ReceivedDrawRound);
            MultiplayerManager.Instance.Unsubscribe(MultiplayerManager.Code.PlayerInput, ReceivedChangeScene);
            MultiplayerManager.Instance.onMatchJoin -= JoinedMatch;
            MultiplayerManager.Instance.onMatchLeave -= LeavedMatch;
        }



        private void ReceivedPlayerWonRound(MultiplayerMessage message)
        {
            PlayerWonData playerWonData = message.GetData<PlayerWonData>();
            PlayersWins[playerWonData.PlayerNumber]++;
            Winner = playerWonData.PlayerNumber;
        }

        private void ReceivedDrawRound(MultiplayerMessage message)
        {
            Winner = null;
        }

        private async void ReceivedChangeScene(MultiplayerMessage message)
        {
            // 1. Stop matchmaking DOTween animation first
            AniamtionManager.instance.StopMatchmakingAnimation();
            AniamtionManager.instance.PageMatchMaking.gameObject.SetActive(false);

            // 2. Set correct avatar sprites BEFORE fly-up animation plays
            ApplyAvatarIcons();

            await Task.Delay(2000);
            AniamtionManager.instance.AnimGoToUpMe.Play("GotoUpPageMe", 0, 0);
            AniamtionManager.instance.AnimGoToUpOpp.Play("GoToUpOpp", 0, 0);
            await Task.Delay(750);
            FindObjectOfType<UiManager>().enabled = true;
            FindObjectOfType<ActionEndGame>().enabled = true;
            AniamtionManager.instance.AnimGoToUpMe.enabled = false;
            AniamtionManager.instance.AnimGoToUpOpp.enabled = false;
            AniamtionManager.instance.AnimGoToUpMe.GetComponent<RectTransform>().parent = AniamtionManager.instance.IconMe;
            AniamtionManager.instance.AnimGoToUpOpp.GetComponent<RectTransform>().parent = AniamtionManager.instance.IconOpp;
            AniamtionManager.instance.AnimGoToUpOpp.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            AniamtionManager.instance.AnimGoToUpMe.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }

        private void ApplyAvatarIcons()
        {
            var anim = AniamtionManager.instance;
            if (anim == null) return;

            var lib = avatarLibrary != null ? avatarLibrary : Nakama.Helpers.ProfileService.Instance?.AvatarLibrary;
            if (lib == null)
            {
                Debug.LogWarning("[ApplyAvatar] AvatarLibrary نیست — در Inspector GameManager.avatarLibrary رو assign کن");
                return;
            }

            // 🟢 اواتار خودم — از داده لوکال (ProfileService) یا از سرور (PlayersManager) بک‌آپ
            var ps = Nakama.Helpers.ProfileService.Instance;
            string myUserId = MultiplayerManager.Instance?.Self?.UserId;
            string myAvatarId = "avatar_0";

            if (ps != null && ps.IsLoaded && !string.IsNullOrEmpty(ps.CurrentAvatarId))
            {
                myAvatarId = ps.CurrentAvatarId;
                Debug.Log("[ApplyAvatar] me (from ProfileService) avatarId=" + myAvatarId);
            }
            else
            {
                // ProfileService آماده نیست — از داده سرور (PlayersManager) استفاده کن
                var myPlayer = PlayersManager.Instance?.Players?.Find(p => p != null && p.Presence?.UserId == myUserId);
                if (myPlayer != null && !string.IsNullOrEmpty(myPlayer.AvatarId))
                    myAvatarId = myPlayer.AvatarId;
                Debug.Log("[ApplyAvatar] me (from PlayersManager fallback) avatarId=" + myAvatarId +
                          " ps=" + (ps != null ? "exists,IsLoaded=" + ps.IsLoaded : "null"));
            }

            Sprite mySprite = lib.GetSprite(myAvatarId);
            if (anim.AvatarImageMe != null && mySprite != null)
                anim.AvatarImageMe.sprite = mySprite;

            // 🔴 اواتار حریف — از سرور (PlayersManager)
            var players = PlayersManager.Instance?.Players;
            if (players == null) return;

            foreach (var player in players)
            {
                if (player == null) continue;
                bool isMe = !string.IsNullOrEmpty(myUserId) && player.Presence?.UserId == myUserId;
                if (isMe) continue;  // خودمو از لوکال ست کردم

                string oppAvatarId = string.IsNullOrEmpty(player.AvatarId) ? "avatar_0" : player.AvatarId;
                Sprite oppSprite = lib.GetSprite(oppAvatarId);
                if (anim.AvatarImageOpp != null && oppSprite != null)
                    anim.AvatarImageOpp.sprite = oppSprite;
                Debug.Log("[ApplyAvatar] opp (server) avatarId=" + oppAvatarId);
            }
        }

        private async void JoinedMatch()
        {
            // اگه داره rejoin میکنیم (بعد از reconnect)، نباید Lobby بریم —
            // در همین صحنه‌ی Battle باید بمونیم
            if (MultiplayerManager.Instance != null && MultiplayerManager.Instance.IsRejoining)
            {
                AnalyticsTracker.SendDesign("match_start", 1f, new Dictionary<string, object>
                {
                    ["rejoin"] = true,
                    ["mode"] = modeGame.ToString()
                });
                return;
            }

            AnalyticsTracker.SendDesign("match_start", 1f, new Dictionary<string, object>
            {
                ["rejoin"] = false,
                ["mode"] = modeGame.ToString()
            });

            ResetPlayerWins();
            GoToLobby();
        }

        private void LeavedMatch()
        {
            AnalyticsTracker.SendDesign("match_leave");
            GoToHome();
        }

        private void ResetPlayerWins()
        {
            PlayersWins = new int[2];
        }

        private void GoToHome()
        {
            AnalyticsTracker.SendDesign("scene_transition", 1f, new Dictionary<string, object>
            {
                ["from"] = SceneManager.GetActiveScene().name,
                ["to"] = Scenes.Home.ToString()
            });
            SceneManager.LoadScene((int)Scenes.Home);
        }

        private void GoToLobby()
        {
            string targetScene = modeGame.ToString();
            switch (modeGame)
            {
                case ModeGame.ThreeByThree:
                    AnalyticsTracker.SendDesign("scene_transition", 1f, new Dictionary<string, object>
                    {
                        ["from"] = SceneManager.GetActiveScene().name,
                        ["to"] = Scenes.ThreeByThree.ToString()
                    });
                    SceneManager.LoadScene((int)Scenes.ThreeByThree);
                    break;
                case ModeGame.FourByThree:
                    AnalyticsTracker.SendDesign("scene_transition", 1f, new Dictionary<string, object>
                    {
                        ["from"] = SceneManager.GetActiveScene().name,
                        ["to"] = Scenes.FourByThree.ToString()
                    });
                    SceneManager.LoadScene((int)Scenes.FourByThree);
                    break;
                case ModeGame.VerticalAndHorizontal:
                    AnalyticsTracker.SendDesign("scene_transition", 1f, new Dictionary<string, object>
                    {
                        ["from"] = SceneManager.GetActiveScene().name,
                        ["to"] = Scenes.VerticalAndHorizontal.ToString()
                    });
                    SceneManager.LoadScene((int)Scenes.VerticalAndHorizontal);
                    break;
                default:
                    break;
            }
        }

        #endregion
    }
}
