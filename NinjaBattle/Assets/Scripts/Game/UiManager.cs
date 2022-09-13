using Nakama.Helpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NinjaBattle.Game;
using Unity.Mathematics;
using System;

public class UiManager : MonoBehaviour
{
    // Start is called before the first frame update

   [SerializeField] private Button dicRollButton;
    [SerializeField] private Transform transformOpp;
    [SerializeField] private List<TileDataOpp> tileDataOpps = new List<TileDataOpp>();
    [SerializeField] private List<ClickInCell> tileDataMe = new List<ClickInCell>();
    [SerializeField] private TextMeshProUGUI ScoreTextMe;
    [SerializeField] private TextMeshProUGUI ScoreTextOpp;
    [SerializeField] private Animator TextTurn;
    [SerializeField] private Animator TextValueMines;
    [SerializeField] private Sprite[] DiceRollsSprite;
    public TextMeshProUGUI[] arryRowSumMe;
    public TextMeshProUGUI[] arryRowSumOpp;
    public TextMeshProUGUI NameOpp;

    private void Start()
    {
        if (MultiplayerManager.Instance.isTurn)
        {
            dicRollButton.interactable = true;
            dicRollButton.GetComponent<Image>().sprite = DiceRollsSprite[0];
            TextTurn.Play("TuenText", 0, 0);
            TextTurn.GetComponent<TextMeshProUGUI>().text = "Your Turn";
            NameOpp.text =PlayerPrefs.GetString("Opp");
        }
        else
        {
            dicRollButton.GetComponent<Image>().sprite = DiceRollsSprite[1];
            TextTurn.Play("TuenText", 0, 0);
            TextTurn.GetComponent<TextMeshProUGUI>().text = "Opponent's Turn";
            NameOpp.text = PlayerPrefs.GetString("Opp");
        }
        
    }
    void OnEnable()
    {
       GameManager.Instance.diceRoller = GameObject.Find("DieImage").GetComponent<DiceRoller>();
       PlayersManager.Instance.onSetDataInTurn += Instance_SetDataInTurn;
        MultiplayerManager.Instance.onTurnMe += Instance_onTurnMe;
        PlayersManager.Instance.onSetDataInRowMe += Instance_onSetDataInRowMe;
        PlayersManager.Instance.onSetDataInRowOpp += Instance_onSetDataInRowOpp;
        PlayersManager.Instance.onSetScoreOpp += Instance_onSetScoreOpp;
        PlayersManager.Instance.onSetScoreMe += Instance_onSetScoreMe;
   

        PlayersManager.Instance.IsTurn += Instance_IsTurn;
    }
    void OnDestroy()
    {
        PlayersManager.Instance.onSetDataInTurn -= Instance_SetDataInTurn;
        MultiplayerManager.Instance.onTurnMe -= Instance_onTurnMe;
        PlayersManager.Instance.onSetDataInRowMe -= Instance_onSetDataInRowMe;
        PlayersManager.Instance.onSetDataInRowOpp -= Instance_onSetDataInRowOpp;
        PlayersManager.Instance.onSetScoreOpp -= Instance_onSetScoreOpp;
        PlayersManager.Instance.onSetScoreMe -= Instance_onSetScoreMe;
        PlayersManager.Instance.IsTurn -= Instance_IsTurn;
    }

    private void Instance_onSetScoreMe(int obj,int mines, DataPlayer data)
    {
        ScoreTextMe.text = obj.ToString();
        if (mines>0)
        {
            TextValueMines.GetComponent<TextMeshProUGUI>().text = "-" + mines;
            TextValueMines.Play("Mines", 0, 0);
          

            var total = int.Parse(arryRowSumOpp[data.NumberLine].text) - mines;
            arryRowSumOpp[data.NumberLine].text = total.ToString();
            mines = 0;
        }
    }

    private void Instance_onSetScoreOpp(int obj ,int mines ,DataPlayer data)
    {
        ScoreTextOpp.text = obj.ToString();
        if (mines > 0)
        {
            TextValueMines.GetComponent<TextMeshProUGUI>().text = "-" + mines;
            TextValueMines.Play("Mines", 0, 0);
       
           var total = int.Parse(arryRowSumMe[data.NumberLine].text) - mines;
            arryRowSumMe[data.NumberLine].text = total.ToString();
            mines =0;
        }
    }

    private void Instance_onSetDataInRowOpp(int arg1, int arg2 )
    {
        Debug.Log(arg1 + " " + arg2);
       var clone= tileDataOpps.Find(e=>e.line == arg1 && e.row == arg2);
        clone.GetComponentsInChildren<Image>()[1].sprite = null;
        clone.GetComponentsInChildren<Image>()[1].enabled = false;
        clone.GetComponentInParent<TileDataOpp>().ValueTile = 0;
    }

    private void Instance_onSetDataInRowMe(int arg1, int arg2 )
    {
        Debug.Log(arg1 + " " + arg2);
        var meCell =tileDataMe.Find(r =>  r.numberLine == arg1 && r.numberRow == arg2);
        meCell.GetComponentsInChildren<Image>()[1].sprite = null;
        meCell.GetComponentInParent<ClickInCell>().ValueTile = 0;
        meCell.GetComponentsInChildren<Image>()[1].enabled = false;
        meCell.isLock = false;
    }

    private void Instance_SetDataInTurn(DataPlayer obj)
    {
   
        if (obj.UserId != MultiplayerManager.Instance.players.User.Id)
        {

            var tile = transformOpp.Find(obj.NameTile).GetComponentsInChildren<Image>()[1];
            tile.enabled = true;
            tile.GetComponent<Animator>().Play("DiceRoot", 0, 0);
            tile.GetComponentInParent<TileDataOpp>().ValueTile = obj.NumberTile+1;
            tile.sprite = GameManager.Instance.diceRoller.Dice[obj.NumberTile];
            TextTurn.Play("TuenText", 0, 0);
            TextTurn.GetComponent<TextMeshProUGUI>().text = "Your Turn";
            if (obj.master)
            {
                var total =  int.Parse(arryRowSumOpp[obj.NumberLine].text) + obj.sumRow1[obj.NumberLine] ;
                arryRowSumOpp[obj.NumberLine].text = total.ToString();
               // arryRowSumMe[obj.NumberLine].text = arryRowSumMe[obj.NumberLine] + obj.sumRow1[obj.NumberLine].ToString();
            }
            else
            {
                var total = int.Parse(arryRowSumOpp[obj.NumberLine].text) + obj.sumRow2[obj.NumberLine];
                arryRowSumOpp[obj.NumberLine].text =total.ToString();
              //  arryRowSumMe[obj.NumberLine].text = arryRowSumMe[obj.NumberLine] + obj.sumRow2[obj.NumberLine].ToString();
            }
        }
        else
        {
            if (obj.master)
            {
                var total = int.Parse(arryRowSumMe[obj.NumberLine].text) + obj.sumRow1[obj.NumberLine];
                arryRowSumMe[obj.NumberLine].text = total.ToString();
              //  arryRowSumMe[obj.NumberLine].text = arryRowSumMe[obj.NumberLine] + obj.sumRow1[obj.NumberLine].ToString();
            }
            else
            {
                var total = int.Parse(arryRowSumMe[obj.NumberLine].text) + obj.sumRow2[obj.NumberLine];
                // arryRowSumOpp[obj.NumberLine].text = arryRowSumOpp[obj.NumberLine] + obj.sumRow1[obj.NumberLine].ToString();
                arryRowSumMe[obj.NumberLine].text = total.ToString();
            }
            TextTurn.Play("TuenText", 0, 0);
            TextTurn.GetComponent<TextMeshProUGUI>().text = "Opponent's Turn";
          
        }
      
    }

   

    private void Instance_IsTurn(bool obj)
    {
        if (obj)
        {
            dicRollButton.interactable = true;
            dicRollButton.GetComponent<Image>().sprite = DiceRollsSprite[0];
            MultiplayerManager.Instance.isTurn=true;
            TextTurn.Play("TuenText", 0, 0);
            TextTurn.GetComponent<TextMeshProUGUI>().text = "Your Turn";

        }
        else
        {
            TextTurn.Play("TuenText", 0, 0);
            dicRollButton.GetComponent<Image>().sprite = DiceRollsSprite[1];
            TextTurn.GetComponent<TextMeshProUGUI>().text = "Opponent's Turn";
            dicRollButton.interactable = false;
            MultiplayerManager.Instance.isTurn = false;
        }
    }

    private void Instance_onTurnMe()
    {
        dicRollButton.interactable = true;
       // dicRollButton.GetComponent<Image>().sprite = DiceRollsSprite[0];
        TextTurn.Play("TuenText", 0, 0);
        TextTurn.GetComponent<TextMeshProUGUI>().text = "Your Turn";

    }
    public void Leave()
    {
        MultiplayerManager.Instance.LeaveMatchAsync();
    }
  

}
