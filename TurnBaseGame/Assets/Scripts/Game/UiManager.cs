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
using System;

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

    private void Instance_LeftPlayer(string obj)
    {
        PanelLeftPalyer.SetActive(true);
        var hxdTotal = PlayerPrefs.GetInt("HXD");
        PlayerPrefs.SetInt("HXD", hxdTotal + (MultiplayerManager.Instance.ValueHXDInGameTurn * 2));
        NamePalyerLeft.text = $"{obj} Is Left of match \n Add {(MultiplayerManager.Instance.ValueHXDInGameTurn * 2)} HXD to your wallet";
    }

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
    /// call back event rematch 
    /// </summary>
    /// <param name="obj"></param>
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
            messageLeftPalyerInRematch.text = "Player is Left";
            acceptRematchButton.gameObject.SetActive(true);
            acceptRematchButton.gameObject.SetActive(false);
            exitRematchButton.gameObject.SetActive(false);
            exitButton.gameObject.SetActive(true);
            ActionEndGame.instance.ResultPanel.SetActive(false);
            loading.gameObject.SetActive(false);
            AniamtionManager.instance.AnimGoToUpMe.gameObject.SetActive(false);
            AniamtionManager.instance.AnimGoToUpOpp.gameObject.SetActive(false);
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

    /// <summary>
    /// Button for send answer For Rematch
    /// </summary>
    /// <param name="answerOpp"></param>
    public void SendAcceptForRematch(string answerOpp)
    {
        rematchPanle.SetActive(true);
        AniamtionManager.instance.AnimGoToUpMe.gameObject.SetActive(false);
        AniamtionManager.instance.AnimGoToUpOpp.gameObject.SetActive(false);
        ActionEndGame.instance.ResultPanel.SetActive(false);
        var answer = new RematchData
        {
            Answer = answerOpp,
            UserId = MultiplayerManager.Instance.Self.UserId

        };
        if (answer.Answer == "send")
        {
            acceptRematchButton.gameObject.SetActive(false);
            exitRematchButton.gameObject.SetActive(false);
            loading.gameObject.SetActive(true);
        }
        else if (answer.Answer == "req")
        {
            loading.gameObject.SetActive(true);
            ActionEndGame.instance.ResultPanel.SetActive(false);

        }

        MultiplayerManager.Instance.Send(MultiplayerManager.Code.Rematch, answer);
    }
    /// <summary>
    /// reset game after rematch
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
        RowSum();

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
    ///  call remove dice for you  in bord game  
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="mines"></param>
    /// <param name="data"></param>
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
    /// call remove dice for opp  in bord game  
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="mines"></param>
    /// <param name="data"></param>
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


    [ContextMenu("sum")]
    public void RowSum()
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
        RowSum();




    }
    /// <summary>
    /// Set data iv row 
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

        RowSum();



    }
    /// <summary>
    /// check data in turn player
    /// </summary>
    /// <param name="obj"></param>
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
         
            RowSum();

        }
        else
        {
            if (!obj.EndGame)
                TextTurnOpp.Play("OppTurn", 0, 0);
            RowSum();
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
    /// trun of player clint
    /// </summary>
    /// <param name="obj"></param>
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
    /// check first turn in start game
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
    /// call Event leave of room 
    /// </summary>
    public void Leave()
    {
        MultiplayerManager.Instance.LeaveMatchAsync();

    }


}
