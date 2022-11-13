using Nakama.Helpers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NinjaBattle.Game;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine.U2D;
using NinjaBattle.General;

public class UiManager : MonoBehaviour
{
    #region Property

    [SerializeField] private Button dicRollButton;
    [SerializeField] private Transform transformOpp;

    [SerializeField] private List<TileDataOpp> tileDataOpps = new List<TileDataOpp>();
    [SerializeField] private List<ClickInCell> tileDataMe = new List<ClickInCell>();

    [SerializeField] private TextMeshProUGUI ScoreTextMe;
    [SerializeField] private TextMeshProUGUI ScoreTextOpp;

    [SerializeField] private Animator TextTurnYou;
    [SerializeField] private Animator TextTurnOpp;
    [SerializeField] private Animator TextValueMines;
    [SerializeField] private Sprite[] DiceRollsSprite;
    [SerializeField] private GameObject rematchPanle;
    [SerializeField] private TextMeshProUGUI messageLeftPalyerInRematch;
    [SerializeField] private Button rematchButton;

    [SerializeField] private Button acceptRematchButton;
    [SerializeField] private Button exitRematchButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private Image loading;


    public TextMeshProUGUI[] arryRowSumMe;
    public TextMeshProUGUI[] arryRowSumMeCal;
    public TextMeshProUGUI[] arryRowSumOpp;
    public TextMeshProUGUI[] arryRowSumOppCal;
    public TextMeshProUGUI NameOpp;
    public ParticleSystem WowPar;
    public static UiManager instance;


    public TextMeshProUGUI HXDWin;
    public GameObject PanelLeftPalyer;
    public TextMeshProUGUI NamePalyerLeft;

    public Animator StickerShow;
    public Image StickerOpp;
    public SpriteAtlas AllAssets;
    public Color colroParticlewhite;


    public Button SendTurn;

    public AudioClip DiceSound;

    #endregion

    private void Start()
    {
        instance = this;
        if (MultiplayerManager.Instance.isTurn)
        {
            dicRollButton.interactable = true;
            dicRollButton.GetComponent<Image>().sprite = DiceRollsSprite[0];
            TextTurnYou.Play("YouTurn", 0, 0);
            TimerTurn.instance.TimerRunning = true;
            NameOpp.text = PlayerPrefs.GetString("Opp");
            AniamtionManager.instance.AnimGoToUpMe.GetComponentInChildren<ParticleSystem>().Play();
            AniamtionManager.instance.AnimGoToUpOpp.GetComponentInChildren<ParticleSystem>().Stop();
        }
        else
        {
            dicRollButton.GetComponent<Image>().sprite = DiceRollsSprite[1];
            TextTurnOpp.Play("OppTurn", 0, 0);
            TimerTurn.instance.TimerRunning = false;
            NameOpp.text = PlayerPrefs.GetString("Opp");
            GameManager.Instance.diceRoller.Rotation(true);
            AniamtionManager.instance.AnimGoToUpMe.GetComponentInChildren<ParticleSystem>().Stop();
            AniamtionManager.instance.AnimGoToUpOpp.GetComponentInChildren<ParticleSystem>().Play();
            TimerTurn.instance.TimerText.text = "-";
        }

    }
    void OnEnable()
    {
        GameManager.Instance.diceRoller = GameObject.Find("DieImage").GetComponent<DiceRoller>();
        PlayersManager.Instance.onSetDataInTurn += Instance_SetDataInTurn;
        PlayersManager.Instance.onSetScoreMe += Instance_onSetScoreMe;
        PlayersManager.Instance.onSetScoreOpp += Instance_onSetScoreOpp;
        MultiplayerManager.Instance.onTurnMe += Instance_onTurnMe;
        PlayersManager.Instance.onSetDataInRowMe += Instance_onSetDataInRowMe;
        PlayersManager.Instance.onSetDataInRowOpp += Instance_onSetDataInRowOpp;
        PlayersManager.Instance.onRematch += Instance_onRematch;
        PlayersManager.Instance.IsTurn += Instance_IsTurn;
        GameManager.Instance.diceRoller.RollUp += ShowHighLight;
        TimerTurn.instance.TimerStop += Instance_TimerStop;
        PlayersManager.Instance.LeftPlayer += Instance_LeftPlayer;
    }
    /// <summary>
    /// action when left palyer 
    /// </summary>
    /// <param name="obj"></param>
    private void Instance_LeftPlayer(string obj)
    {
        PanelLeftPalyer.SetActive(true);
        NamePalyerLeft.text = $"{obj} Is Left of match \n Add {(MultiplayerManager.Instance.ValueHXDInGameTurn * 2)} HXD to your wallet";
    }
    /// <summary>
    /// when  timer equal zero in turn find first empety cell and set data in cell
    /// </summary>
    private void Instance_TimerStop()
    {
        if (GameManager.Instance.diceRoller.currrentDie == -1)
            GameManager.Instance.diceRoller.currrentDie = UnityEngine.Random.Range(0, 6);

        var cell = tileDataMe.First(e => e.isLock == false);
        cell.SetDataInCell();
        SendTurn.interactable = false;
    }

    private void OnDisable()
    {
        PlayersManager.Instance.onSetDataInTurn -= Instance_SetDataInTurn;
        PlayersManager.Instance.onSetScoreOpp -= Instance_onSetScoreOpp;
        PlayersManager.Instance.onSetScoreMe -= Instance_onSetScoreMe;
        MultiplayerManager.Instance.onTurnMe -= Instance_onTurnMe;
        PlayersManager.Instance.onSetDataInRowMe -= Instance_onSetDataInRowMe;
        PlayersManager.Instance.onSetDataInRowOpp -= Instance_onSetDataInRowOpp;
        PlayersManager.Instance.IsTurn -= Instance_IsTurn;
        PlayersManager.Instance.onRematch -= Instance_onRematch;
        TimerTurn.instance.TimerStop -= Instance_TimerStop;
        PlayersManager.Instance.LeftPlayer -= Instance_LeftPlayer;
        GameManager.Instance.diceRoller.RollUp -= ShowHighLight;
    }


    /// <summary>
    /// A callback function that is called when a rematch is requested.
    /// </summary>
    /// <param name="RematchData"></param>
    private void Instance_onRematch(RematchData obj)
    {
        //if (obj.UserId == MultiplayerManager.Instance.Self.UserId)
        //    return;


        if (obj.Answer == "req")
        {

            rematchPanle.SetActive(true);
            ActionEndGame.instance.ResultPanel.SetActive(false);
            AniamtionManager.instance.AnimGoToUpMe.gameObject.SetActive(false);
            AniamtionManager.instance.AnimGoToUpOpp.gameObject.SetActive(false);



        }
        if (obj.Answer == "yes")
        {


            rematchPanle.SetActive(false);
            ActionEndGame.instance.ResultPanel.SetActive(false);
            _ = Task.Delay(1000);
            ResetGame();
            obj.Answer = "";

        }


        if (obj.Answer == "left")
        {
            TurnOnExitButtonWhenFinishGame("Player is Left");

        }
        if (obj.Answer == "no")
        {
            messageLeftPalyerInRematch.text = "Player dont accept";
            loading.gameObject.SetActive(false);
            exitButton.gameObject.SetActive(true);
            AniamtionManager.instance.AnimGoToUpMe.gameObject.SetActive(false);
            AniamtionManager.instance.AnimGoToUpOpp.gameObject.SetActive(false);

        }
    }

    private void TurnOnExitButtonWhenFinishGame(string message)
    {
        messageLeftPalyerInRematch.text = message;
        acceptRematchButton.gameObject.SetActive(true);
        acceptRematchButton.gameObject.SetActive(false);
        exitRematchButton.gameObject.SetActive(false);
        exitButton.gameObject.SetActive(true);
        ActionEndGame.instance.ResultPanel.SetActive(false);
        loading.gameObject.SetActive(false);
        AniamtionManager.instance.AnimGoToUpMe.gameObject.SetActive(false);
        AniamtionManager.instance.AnimGoToUpOpp.gameObject.SetActive(false);
    }
    /// <summary>
    /// Button for send answer For Rematch
    /// </summary>
    /// <param name="answerOpp"></param>
    public void SendAcceptForRematch(string answerOpp)
    {
        //NakamaStorageManager.Instance.UpdateCollectionObject(NakamaStorageManager.Instance.NakamaCollectionObjectWallet);
        //var collction = NakamaStorageManager.Instance.NakamaCollectionObjectWallet.GetValue<WalletData>();
        //var hxdTotal = collction.hxdAmount;
        rematchPanle.SetActive(true);
        AniamtionManager.instance.AnimGoToUpMe.gameObject.SetActive(false);
        AniamtionManager.instance.AnimGoToUpOpp.gameObject.SetActive(false);
        ActionEndGame.instance.ResultPanel.SetActive(false);
       // Debug.Log(hxdTotal);
        var answer = new RematchData
        {
            Answer = answerOpp,
            UserId = MultiplayerManager.Instance.Self.UserId

        };
        if (answer.Answer == "send")
        {
          //  if (hxdTotal >= MultiplayerManager.Instance.ValueHXDInGameTurn)
            {

                acceptRematchButton.gameObject.SetActive(false);
                exitRematchButton.gameObject.SetActive(false);
                loading.gameObject.SetActive(true);
            }
            //else
            //{
            //    TurnOnExitButtonWhenFinishGame("You dont have enough HXD to play");
            //    return;
            //}
        }
        else if (answer.Answer == "req")
        {
            loading.gameObject.SetActive(true);
            ActionEndGame.instance.ResultPanel.SetActive(false);

        }
        else if (answer.Answer == "yes")
        {
           // if (hxdTotal < MultiplayerManager.Instance.ValueHXDInGameTurn)
            {
                answer.Answer = "no";
                TurnOnExitButtonWhenFinishGame("You dont have enough HXD to play");
            }
            //else
            //{
            //    answer.Answer = "yes";
            //    hxdTotal -= MultiplayerManager.Instance.ValueHXDInGameTurn;
            //    PlayerPrefs.SetInt("HXD", hxdTotal);
            //}
        }

        MultiplayerManager.Instance.Send(MultiplayerManager.Code.Rematch, answer);
        answer.Answer = "";
    }

    /// <summary>
    /// It resets the game.
    /// </summary>
    private async void ResetGame()
    {
        foreach (var opp in tileDataOpps)
        {
            opp.SpriteDice.transform.parent.gameObject.SetActive(false);
            opp.ValueTile = 0;
            opp.IsLock = false;
            opp.particleDouble.Stop();
            opp.empetySpace.Stop();
            ParticleSystem.MainModule settings = opp.GetComponentInChildren<ParticleSystem>().main;
            settings.startColor = new ParticleSystem.MinMaxGradient(colroParticlewhite);
        }
        foreach (var me in tileDataMe)
        {
            me.isLock = false;
            me.ValueTile = 0;
            me.empetySpace.Stop();
            me.particleDouble.Stop();
            ParticleSystem.MainModule settings = me.GetComponentInChildren<ParticleSystem>().main;
            settings.startColor = new ParticleSystem.MinMaxGradient(colroParticlewhite);
            me.SpriteDice.transform.parent.gameObject.SetActive(false);
        }
        ClearList();
        ScoreTextMe.text = "0";
        ScoreTextOpp.text = "0";
        TotalScoreTiles();

        await Task.Delay(1000);
        AniamtionManager.instance.AnimGoToUpMe.gameObject.SetActive(true);
        AniamtionManager.instance.AnimGoToUpOpp.gameObject.SetActive(true);

        ActionEndGame.instance.ResultPanel.SetActive(false);
        AniamtionManager.instance.AnimGoToUpMe.Play("GotoUpPageMe", 0, 0);
        AniamtionManager.instance.AnimGoToUpOpp.Play("GoToUpOpp", 0, 0);

        await Task.Delay(1000);

        AniamtionManager.instance.AnimGoToUpMe.enabled = false;
        AniamtionManager.instance.AnimGoToUpOpp.enabled = false;
        AniamtionManager.instance.AnimGoToUpMe.GetComponent<RectTransform>().parent = AniamtionManager.instance.IconMe;
        AniamtionManager.instance.AnimGoToUpOpp.GetComponent<RectTransform>().parent = AniamtionManager.instance.IconOpp;
        AniamtionManager.instance.AnimGoToUpOpp.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        AniamtionManager.instance.AnimGoToUpMe.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        TimerTurn.instance.TimerCount = 30;
    }
    /// <summary>
    /// show particle when before click to tile (empety tile )
    /// </summary>
    /// <param name="obj"></param>
    private void ShowHighLight(bool obj)
    {
        var list = tileDataMe.FindAll(e => e.isLock == false);
        if (obj)
        {
            foreach (var item in list)
            {
                item.empetySpace.Play();
            }
        }
        else
        {
            foreach (var item in list)
            {
                item.empetySpace.Stop();
            }
        }

    }

    /// <summary>
    /// This function is called when the score of the player is changed
    /// </summary>
    /// <param name="obj">The score of the player</param>
    /// <param name="mines">the number of mines that were hit</param>
    /// <param name="DataPlayer">This is a class that contains the player's name, score, and
    /// mines.</param>
    private void Instance_onSetScoreMe(int obj, int mines, DataPlayer data)
    {
        if (mines > 0)
        {
            TextValueMines.GetComponent<TextMeshProUGUI>().text = "-" + mines;
            TextValueMines.Play("Mines", 0, 0);

            WowPar.Play();

            mines = 0;
        }
        ScoreTextMe.text = obj.ToString();


    }

    /// <summary>
    /// This function is called when the score of the player opp is changed
    /// </summary>
    /// <param name="obj">The score of the opponent</param>
    /// <param name="mines">the number of mines that were destroyed</param>
    /// <param name="DataPlayer">This is a class that contains the player's name, score, and other
    /// information.</param>
    private void Instance_onSetScoreOpp(int obj, int mines, DataPlayer data)
    {
        if (mines > 0)
        {
            TextValueMines.GetComponent<TextMeshProUGUI>().text = "-" + mines;
            TextValueMines.Play("Mines", 0, 0);
            WowPar.Play();

            mines = 0;
        }
        ScoreTextOpp.text = obj.ToString();

    }


    /// <summary>
    /// It adds the numbers in the row together.
    /// </summary>
    public void TotalScoreTiles()
    {

        foreach (var item in tileDataMe)
        {
            item.GetComponentInChildren<ParticleSystem>().Stop();
            ParticleSystem.MainModule settings = item.GetComponentInChildren<ParticleSystem>().main;
            settings.startColor = new ParticleSystem.MinMaxGradient(CalculterRowScore.instance.whitecolor);
        }
        foreach (var item in tileDataOpps)
        {
            item.GetComponentInChildren<ParticleSystem>().Stop();
            ParticleSystem.MainModule settings = item.GetComponentInChildren<ParticleSystem>().main;
            settings.startColor = new ParticleSystem.MinMaxGradient(CalculterRowScore.instance.whitecolor);
        }

        arryRowSumOpp[0].text = CalculterRowScore.instance.TilesOpp(CalculterRowScore.instance.tileDataOpps).ToString();

        arryRowSumOpp[1].text = CalculterRowScore.instance.TilesOpp(CalculterRowScore.instance.tileDataOpps2).ToString();

        arryRowSumOpp[2].text = CalculterRowScore.instance.TilesOpp(CalculterRowScore.instance.tileDataOpps3).ToString();


        arryRowSumMe[0].text = CalculterRowScore.instance.TileMe(CalculterRowScore.instance.clickInCells).ToString();

        arryRowSumMe[1].text = CalculterRowScore.instance.TileMe(CalculterRowScore.instance.clickInCells1).ToString();

        arryRowSumMe[2].text = CalculterRowScore.instance.TileMe(CalculterRowScore.instance.clickInCells2).ToString();


        if (GameManager.Instance.modeGame == ModeGame.VerticalAndHorizontal)
        {
            arryRowSumOppCal[0].text = CalculterRowScore.instance.TilesOpp(CalculterRowScore.instance.tileDataOppsCal).ToString();

            arryRowSumOppCal[1].text = CalculterRowScore.instance.TilesOpp(CalculterRowScore.instance.tileDataOpps2Cal).ToString();

            arryRowSumOppCal[2].text = CalculterRowScore.instance.TilesOpp(CalculterRowScore.instance.tileDataOpps3Cal).ToString();


            arryRowSumMeCal[0].text = CalculterRowScore.instance.TileMe(CalculterRowScore.instance.clickInCellsCal).ToString();

            arryRowSumMeCal[1].text = CalculterRowScore.instance.TileMe(CalculterRowScore.instance.clickInCells1Cal).ToString();

            arryRowSumMeCal[2].text = CalculterRowScore.instance.TileMe(CalculterRowScore.instance.clickInCells2Cal).ToString();
        }

        if (GameManager.Instance.modeGame == ModeGame.FourByFour || GameManager.Instance.modeGame == ModeGame.FourByThree)
        {
            arryRowSumOpp[3].text = CalculterRowScore.instance.TilesOpp(CalculterRowScore.instance.tileDataOpps4).ToString();

            arryRowSumMe[3].text = CalculterRowScore.instance.TileMe(CalculterRowScore.instance.clickInCells3).ToString();

        }
        ClearList();

    }


    /// <summary>
    /// find dice in tiles for remove opp
    /// </summary>
    /// <param name="arg1"></param>
    /// <param name="arg2"></param>
    private void Instance_onSetDataInRowOpp(int arg1, int arg2)
    {


        var clone = tileDataOpps.Find(e => e.line == arg1 && e.row == arg2 && e.IsLock);
        if (clone != null)
        {

            clone.SpriteDice.sprite = null;
            clone.ValueTile = 0;
            clone.IsLock = false;
            clone.PluseScore = false;
            clone.SpriteDice.transform.parent.gameObject.SetActive(false);
            clone.particleDouble.Stop();
            ParticleSystem.MainModule settings = clone.particleDouble.main;
            settings.startColor = new ParticleSystem.MinMaxGradient(colroParticlewhite);
        }
        TotalScoreTiles();

    }
    /// <summary>
    ///  find dice in tiles for remove player
    /// </summary>
    /// <param name="arg1"></param>
    /// <param name="arg2"></param>
    private void Instance_onSetDataInRowMe(int arg1, int arg2)
    {


        var meCell = tileDataMe.Find(r => r.numberLine == arg1 && r.numberRow == arg2 && r.isLock);

        if (meCell != null)
        {
            meCell.PlusScore = false;
            meCell.SpriteDice.sprite = null;
            meCell.ValueTile = 0;
            meCell.isLock = false;
            meCell.particleDouble.Stop();
            ParticleSystem.MainModule settings = meCell.particleDouble.main;
            settings.startColor = new ParticleSystem.MinMaxGradient(colroParticlewhite);
            meCell.SpriteDice.transform.parent.gameObject.SetActive(false);
        }

        TotalScoreTiles();



    }

    /// <summary>
    /// A function that is called when the SetDataInTurn event is raised.
    /// </summary>
    /// <param name="DataPlayer">The player who's turn it is.</param>
    private void Instance_SetDataInTurn(DataPlayer obj)
    {
        //MultiplayerManager.Instance.end = DateTime.Now;
        //MultiplayerManager.Instance.ping = MultiplayerManager.Instance.end - MultiplayerManager.Instance.start;
        //Debug.Log(MultiplayerManager.Instance.ping.Milliseconds + " ping");
        if (obj.UserId != MultiplayerManager.Instance.players.User.Id)
        {
            GameManager.Instance.diceRoller.Rotation(false);
            AniamtionManager.instance.AnimGoToUpMe.GetComponentInChildren<ParticleSystem>().Play();
            AniamtionManager.instance.AnimGoToUpOpp.GetComponentInChildren<ParticleSystem>().Stop();
            _ = Task.Delay(200);
            GameManager.Instance.diceRoller.GetComponent<Image>().sprite = GameManager.Instance.diceRoller.Dice[obj.NumberTile];
            GameManager.Instance.diceRoller.currrentDie = -1;
            var tile = transformOpp.Find(obj.NameTile).GetComponentInChildren<TileDataOpp>();
            tile.IsLock = true;
            tile.SpriteDice.transform.parent.gameObject.SetActive(true);
            tile.SpriteDice.GetComponent<Animator>().Play("DiceRoot", 0, 0);
            AudioManager.Instance.PlayPointAudio(DiceSound);
            tile.ValueTile = obj.NumberTile + 1;
            tile.SpriteDice.sprite = GameManager.Instance.diceRoller.Dice[obj.NumberTile];
            if (!obj.EndGame)
                TextTurnYou.Play("YouTurn", 0, 0);
            TimerTurn.instance.TimerRunning = true;
            TimerTurn.instance.TimerPause = false;
            TimerTurn.instance.TimerText.text = "30";
            TimerTurn.instance.TimerCount = 30;

            TotalScoreTiles();

        }
        else
        {
            if (!obj.EndGame)
                TextTurnOpp.Play("OppTurn", 0, 0);
            TotalScoreTiles();
            AniamtionManager.instance.AnimGoToUpMe.GetComponentInChildren<ParticleSystem>().Stop();
            AniamtionManager.instance.AnimGoToUpOpp.GetComponentInChildren<ParticleSystem>().Play();
            TimerTurn.instance.TimerRunning = false;
            TimerTurn.instance.TimerText.text = "-";

        }

    }
    /// <summary>
    /// Clear list DuobleScores
    /// </summary>
    private void ClearList()
    {
        CalculterRowScore.instance.DuobleScore2.Clear();
        CalculterRowScore.instance.DuobleScore1.Clear();
    }

    /// <summary>
    /// A function that is called when the IsTurn event is raised.
    /// </summary>
    /// <param name="obj">This is a boolean value that tells you if it's your turn or not.</param>
    private void Instance_IsTurn(bool obj)
    {
        if (obj)
        {
            dicRollButton.interactable = true;
            dicRollButton.GetComponent<Image>().sprite = DiceRollsSprite[0];
            MultiplayerManager.Instance.isTurn = true;
            TextTurnYou.Play("YouTurn", 0, 0);
            GameManager.Instance.diceRoller.Rotation(false);
            _ = Task.Delay(1000);
            TimerTurn.instance.TimerPause = false;
            TimerTurn.instance.TimerRunning = true;
            AniamtionManager.instance.AnimGoToUpMe.GetComponentInChildren<ParticleSystem>().Play();
            AniamtionManager.instance.AnimGoToUpOpp.GetComponentInChildren<ParticleSystem>().Stop();

        }
        else
        {
            TextTurnOpp.Play("OppTurn", 0, 0);
            dicRollButton.GetComponent<Image>().sprite = DiceRollsSprite[1];
            dicRollButton.interactable = false;
            MultiplayerManager.Instance.isTurn = false;
            TimerTurn.instance.TimerPause = false;
            TimerTurn.instance.TimerRunning = false;
            AniamtionManager.instance.AnimGoToUpMe.GetComponentInChildren<ParticleSystem>().Stop();
            AniamtionManager.instance.AnimGoToUpOpp.GetComponentInChildren<ParticleSystem>().Play();
        }

    }

    /// <summary>
    /// The function is called when the player's turn is over
    /// </summary>
    private void Instance_onTurnMe()
    {

        dicRollButton.interactable = true;
        dicRollButton.GetComponent<Image>().sprite = DiceRollsSprite[0];
        TextTurnYou.Play("YouTurn", 0, 0);
        GameManager.Instance.diceRoller.Rotation(false);
        AniamtionManager.instance.AnimGoToUpMe.GetComponentInChildren<ParticleSystem>().Play();
        AniamtionManager.instance.AnimGoToUpOpp.GetComponentInChildren<ParticleSystem>().Stop();


    }

    /// <summary>
    /// It's a function that leaves the match
    /// </summary>
    public void Leave()
    {
        MultiplayerManager.Instance.LeaveMatchAsync();

    }


}
