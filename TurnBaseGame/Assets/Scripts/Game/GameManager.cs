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

      /// <summary>
      /// The Start function is called when the script is first loaded. It subscribes to the
      /// MultiplayerManager's ChangeScene event, and sets up the onMatchJoin and onMatchLeave events
      /// </summary>
        private void Start()
        {
            MultiplayerManager.Instance.Subscribe(MultiplayerManager.Code.ChangeScene, ReceivedChangeScene);
            MultiplayerManager.Instance.onMatchJoin += JoinedMatch;
            MultiplayerManager.Instance.onMatchLeave += LeavedMatch;
        }

        private void OnEnable()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            MultiplayerManager.Instance.Unsubscribe(MultiplayerManager.Code.PlayerInput, ReceivedChangeScene);
            MultiplayerManager.Instance.onMatchJoin -= JoinedMatch;
            MultiplayerManager.Instance.onMatchLeave -= LeavedMatch;
        }

       
      /// <summary>
      /// When the player receives a message from the server, the function will be called and the player
      /// will be moved to the next scene.
      /// </summary>
      /// <param name="MultiplayerMessage">This is a class that contains the data that is sent from the
      /// server.</param>
        private async void ReceivedChangeScene(MultiplayerMessage message)
        {
            AniamtionManager.instance.AnimIconOpp.enabled = false;
            AniamtionManager.instance.PageMatchMaking.gameObject.SetActive(false);

            await Task.Delay(2000);
            AniamtionManager.instance.AnimGoToUpMe.Play("GotoUpPageMe", 0, 0);
            AniamtionManager.instance.AnimGoToUpOpp.Play("GoToUpOpp", 0, 0);

            await Task.Delay(1000);
            FindObjectOfType<UiManager>().enabled = true;
            FindObjectOfType<ActionEndGame>().enabled = true;
            AniamtionManager.instance.AnimGoToUpMe.enabled = false;
            AniamtionManager.instance.AnimGoToUpOpp.enabled = false;
            AniamtionManager.instance.AnimGoToUpMe.GetComponent<RectTransform>().parent = AniamtionManager.instance.IconMe;
            AniamtionManager.instance.AnimGoToUpOpp.GetComponent<RectTransform>().parent = AniamtionManager.instance.IconOpp;
            AniamtionManager.instance.AnimGoToUpOpp.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            AniamtionManager.instance.AnimGoToUpMe.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }

      /// <summary>
      /// "If the player is the first to join the match, then the player is the host. If the player is
      /// not the first to join the match, then the player is the client."
      /// 
      /// The first thing we do is reset the player wins. This is because we want to reset the player
      /// wins every time a new match is started
      /// </summary>
        private void JoinedMatch()
        {
            ResetPlayerWins();

            GoToGamePlayScene();
        }
       
      /// <summary>
      /// The function is called when the player leaves the match
      /// </summary>
        private void LeavedMatch()
        {
            GoToHome();
        }
       
      /// <summary>
      /// This function resets the player wins to 0
      /// </summary>
        private void ResetPlayerWins()
        {
            PlayersWins = new int[2];
        }

    /// <summary>
    /// It loads the scene with the index of the enum Scenes.Home
    /// </summary>
        private void GoToHome()
        {
            SceneManager.LoadScene((int)Scenes.Home);
        }
      
      /// <summary>
      /// It loads the scene based on the modeGame enum
      /// </summary>
        private void GoToGamePlayScene()
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
                case ModeGame.FourByFour:
                    SceneManager.LoadScene((int)Scenes.FourByFour);
                    break;
              
            }
        }

        #endregion
    }
}
