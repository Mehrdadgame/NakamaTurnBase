using Nakama.Helpers;
using NinjaBattle.General;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

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
            // SceneManager.LoadScene(message.GetData<int>());

            AniamtionManager.instance.AnimIconOpp.enabled = false;
            AniamtionManager.instance.PageMatchMaking.gameObject.SetActive(false);
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

        private async void JoinedMatch()
        {
            ResetPlayerWins();

            GoToLobby();
        }

        private void LeavedMatch()
        {
            GoToHome();
        }

        private void ResetPlayerWins()
        {
            PlayersWins = new int[2];
        }

        private void GoToHome()
        {
            SceneManager.LoadScene((int)Scenes.Home);
        }

        private void GoToLobby()
        {
            switch (modeGame)
            {
                case ModeGame.ThreeByThree:
                    SceneManager.LoadScene((int)Scenes.ThreeByThree);
                    break;
                case ModeGame.FourByThree:
                    SceneManager.LoadScene((int)Scenes.FourByThree);
                    break;
                case ModeGame.VerticalAndHorizontal:
                    SceneManager.LoadScene((int)Scenes.VerticalAndHorizontal);
                    break;
                default:
                    break;
            }
        }

        #endregion
    }
}
