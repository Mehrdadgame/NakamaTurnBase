using Nakama.Helpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NinjaBattle.Game;
using Unity.Mathematics;
using System;
using System.Threading.Tasks;

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

            NameOpp.text = PlayerPrefs.GetString("Opp");
        }
        else
        {
            dicRollButton.GetComponent<Image>().sprite = DiceRollsSprite[1];
            TextTurnOpp.Play("OppTurn", 0, 0);

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

        PlayersManager.Instance.IsTurn += Instance_IsTurn;
        GameManager.Instance.diceRoller.RollUp += ShowHighLight;
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
        Debug.Log(arg1 + " " + arg2);
        var clone = tileDataOpps.Find(e => e.line == arg1 && e.row == arg2);
        clone.GetComponentsInChildren<Image>()[1].sprite = null;
        clone.GetComponentsInChildren<Image>()[1].enabled = false;
        clone.GetComponentInParent<TileDataOpp>().ValueTile = 0;
        RowSum();
    }

    private void Instance_onSetDataInRowMe(int arg1, int arg2)
    {
        Debug.Log(arg1 + " " + arg2);
        var meCell = tileDataMe.Find(r => r.numberLine == arg1 && r.numberRow == arg2);
        meCell.GetComponentsInChildren<Image>()[1].sprite = null;
        meCell.GetComponentInParent<ClickInCell>().ValueTile = 0;
        meCell.GetComponentsInChildren<Image>()[1].enabled = false;
        meCell.isLock = false;
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
            var tile = transformOpp.Find(obj.NameTile).GetComponentsInChildren<Image>()[1];
            tile.enabled = true;
            tile.GetComponent<Animator>().Play("DiceRoot", 0, 0);
            tile.GetComponentInParent<TileDataOpp>().ValueTile = obj.NumberTile + 1;
            tile.sprite = GameManager.Instance.diceRoller.Dice[obj.NumberTile];
            TextTurnYou.Play("YouTurn", 0, 0);


        }
        else
        {

            TextTurnOpp.Play("OppTurn", 0, 0);
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


        }
        else
        {
            TextTurnOpp.Play("OppTurn", 0, 0);
            dicRollButton.GetComponent<Image>().sprite = DiceRollsSprite[1];

            dicRollButton.interactable = false;
            MultiplayerManager.Instance.isTurn = false;

        }

    }

    private void Instance_onTurnMe()
    {

        dicRollButton.interactable = true;
        // dicRollButton.GetComponent<Image>().sprite = DiceRollsSprite[0];
        TextTurnYou.Play("YouTurn", 0, 0);


    }
    public void Leave()
    {
        MultiplayerManager.Instance.LeaveMatchAsync();
    }


}
