using Nakama.Helpers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NinjaBattle.Game;
using System.Threading.Tasks;
using System.Linq;

public class UiManager : MonoBehaviour
{
    // Start is called before the first frame update

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
    public TextMeshProUGUI[] arryRowSumOpp;
    public TextMeshProUGUI NameOpp;
    public ParticleSystem WowPar;
    public static UiManager instance;
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
        }
        else
        {
            dicRollButton.GetComponent<Image>().sprite = DiceRollsSprite[1];
            TextTurnOpp.Play("OppTurn", 0, 0);
            TimerTurn.instance.TimerRunning = false;
            NameOpp.text = PlayerPrefs.GetString("Opp");
            GameManager.Instance.diceRoller.Rotation(true);
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
    }

    private void Instance_TimerStop()
    {
        GameManager.Instance.diceRoller.currrentDie = Random.Range(0,6);
        var cell = tileDataMe.First(e => e.isLock == false);
        cell.SetDataInCell();
    }

    void OnDestroy()
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
    }
    private void Instance_onRematch(RematchData obj)
    {
        //if (obj.UserId == MultiplayerManager.Instance.Self.UserId)
        //    return;
        Debug.Log(obj.Answer);
        if (obj.Answer == "req")
        {
            rematchPanle.SetActive(true);
            AniamtionManager.instance.AnimGoToUpMe.gameObject.SetActive(false);
            AniamtionManager.instance.AnimGoToUpOpp.gameObject.SetActive(false);
        }
        if (obj.Answer == "yes")
        {
            rematchPanle.SetActive(false);
            ActionEndGame.instance.ResultPanel.SetActive(false);
            _ = Task.Delay(1000);
            ResetGame();
        }
        if (obj.Answer == "left")
        {
            messageLeftPalyerInRematch.text = "Player is Left";
            acceptRematchButton.gameObject.SetActive(true);
            acceptRematchButton.gameObject.SetActive( false);
            exitRematchButton.gameObject.SetActive(false);
            exitButton.gameObject.SetActive(true);
            ActionEndGame.instance.ResultPanel.SetActive(false);
            loading.gameObject.SetActive(false);

        }
        if (obj.Answer == "no")
        {
            messageLeftPalyerInRematch.text = "Player dont accept";
            loading.gameObject.SetActive(false);
            exitButton.gameObject.SetActive(true);
            AniamtionManager.instance.AnimGoToUpMe.gameObject.SetActive(true);
            AniamtionManager.instance.AnimGoToUpOpp.gameObject.SetActive(true);

        }
    }

    public void SendAcceptForRematch(string answerOpp)
    {
        rematchPanle.SetActive(true);
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
        else if(answer.Answer == "req")
        {
            loading.gameObject.SetActive(true);
            ActionEndGame.instance.ResultPanel.SetActive(false);

        }
      //  ResetGame();
        MultiplayerManager.Instance.Send(MultiplayerManager.Code.Rematch, answer);
    }

    private async void ResetGame()
    {
        foreach (var opp in tileDataOpps)
        {
            opp.SpriteDice.transform.parent.gameObject.SetActive(false);
            opp.ValueTile = 0;
        }
        foreach (var me in tileDataMe)
        {
            me.isLock = false;
            me.ValueTile = 0;
            me.SpriteDice.transform.parent.gameObject.SetActive(false);
        }
        ScoreTextMe.text = "0";
        ScoreTextOpp.text="0";
        RowSum();
        await Task.Delay(750);
        AniamtionManager.instance.AnimGoToUpMe.gameObject.SetActive(true);
        AniamtionManager.instance.AnimGoToUpOpp.gameObject.SetActive(true);
        AniamtionManager.instance.AnimGoToUpMe.Play("GotoUpPageMe", 0, 0);
        AniamtionManager.instance.AnimGoToUpOpp.Play("GoToUpOpp", 0, 0);
        await Task.Delay(1000);
        AniamtionManager.instance.AnimGoToUpMe.enabled = false;
        AniamtionManager.instance.AnimGoToUpOpp.enabled = false;
        AniamtionManager.instance.AnimGoToUpMe.GetComponent<RectTransform>().parent = AniamtionManager.instance.IconMe;
        AniamtionManager.instance.AnimGoToUpOpp.GetComponent<RectTransform>().parent = AniamtionManager.instance.IconOpp;
        AniamtionManager.instance.AnimGoToUpOpp.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        AniamtionManager.instance.AnimGoToUpMe.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        
    }
    private void ShowHighLight(bool obj)
    {
        var list = tileDataMe.FindAll(e => e.isLock == false);
        if (obj)
        {
            foreach (var item in list)
            {
                item.GetComponentInChildren<ParticleSystem>().Play();
            }
        }
        else
        {
            foreach (var item in list)
            {
                item.GetComponentInChildren<ParticleSystem>().Stop();
            }
        }
        RowSum();
    }

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
        arryRowSumOpp[0].text = CalculterRowScore.instance.TilesOpp(CalculterRowScore.instance.tileDataOpps).ToString();
        arryRowSumOpp[1].text = CalculterRowScore.instance.TilesOpp(CalculterRowScore.instance.tileDataOpps2).ToString();
        arryRowSumOpp[2].text = CalculterRowScore.instance.TilesOpp(CalculterRowScore.instance.tileDataOpps3).ToString();

        arryRowSumMe[0].text = CalculterRowScore.instance.TileMe(CalculterRowScore.instance.clickInCells).ToString();
        arryRowSumMe[1].text = CalculterRowScore.instance.TileMe(CalculterRowScore.instance.clickInCells1).ToString();
        arryRowSumMe[2].text = CalculterRowScore.instance.TileMe(CalculterRowScore.instance.clickInCells2).ToString();
    }

    private void Instance_onSetDataInRowOpp(int arg1, int arg2)
    {

        var clone = tileDataOpps.Find(e => e.line == arg1 && e.row == arg2);
        clone.SpriteDice.sprite = null;
        clone.ValueTile = 0;
        clone.SpriteDice.transform.parent.gameObject.SetActive(false);
        RowSum();
    }

    private void Instance_onSetDataInRowMe(int arg1, int arg2)
    {

        var meCell = tileDataMe.Find(r => r.numberLine == arg1 && r.numberRow == arg2);
        meCell.SpriteDice.sprite = null;
        meCell.ValueTile = 0;
        meCell.isLock = false;
        meCell.SpriteDice.transform.parent.gameObject.SetActive(false);
        RowSum();
    }

    private void Instance_SetDataInTurn(DataPlayer obj)
    {

        if (obj.UserId != MultiplayerManager.Instance.players.User.Id)
        {

            GameManager.Instance.diceRoller.Rotation(false);

            _ = Task.Delay(200);
            GameManager.Instance.diceRoller.GetComponent<Image>().sprite = GameManager.Instance.diceRoller.Dice[obj.NumberTile];
            GameManager.Instance.diceRoller.currrentDie = -1;
            var tile = transformOpp.Find(obj.NameTile).GetComponentInChildren<TileDataOpp>();
            tile.SpriteDice.transform.parent.gameObject.SetActive(true);
            tile.SpriteDice.GetComponent<Animator>().Play("DiceRoot", 0, 0);
            tile.ValueTile = obj.NumberTile + 1;
            tile.SpriteDice.sprite = GameManager.Instance.diceRoller.Dice[obj.NumberTile];
            if (!obj.EndGame)
                TextTurnYou.Play("YouTurn", 0, 0);
            TimerTurn.instance.TimerRunning = true;
            TimerTurn.instance.TimerPause = false;
            TimerTurn.instance.TimerText.text = "30";
            TimerTurn.instance.TimerCount = 30;
            TimerTurn.instance.TimerText.color = Color.white;

        }
        else
        {
            if (!obj.EndGame)
                TextTurnOpp.Play("OppTurn", 0, 0);
            TimerTurn.instance.TimerRunning = false;
            TimerTurn.instance.TimerText.text = "-"; 
        }
        RowSum();

    }



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

        }
        else
        {
            TextTurnOpp.Play("OppTurn", 0, 0);
            dicRollButton.GetComponent<Image>().sprite = DiceRollsSprite[1];
            dicRollButton.interactable = false;
            MultiplayerManager.Instance.isTurn = false;
            TimerTurn.instance.TimerPause = false;
            TimerTurn.instance.TimerRunning = false;
        }

    }

    private void Instance_onTurnMe()
    {

        dicRollButton.interactable = true;
         dicRollButton.GetComponent<Image>().sprite = DiceRollsSprite[0];
        TextTurnYou.Play("YouTurn", 0, 0);
        GameManager.Instance.diceRoller.Rotation(false);
   

    }
    public void Leave()
    {
        MultiplayerManager.Instance.LeaveMatchAsync();
    }


}
