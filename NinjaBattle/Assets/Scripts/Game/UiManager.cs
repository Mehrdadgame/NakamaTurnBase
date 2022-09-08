using Nakama.Helpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NinjaBattle.Game;

public class UiManager : MonoBehaviour
{
    // Start is called before the first frame update

   [SerializeField] private Button dicRollButton;
    [SerializeField] private Transform transformOpp;
    [SerializeField] private List<TileDataOpp> tileDataOpps = new List<TileDataOpp>();
    [SerializeField] private List<ClickInCell> tileDataMe = new List<ClickInCell>();
    [SerializeField] private TextMeshProUGUI ScoreTextMe;
    [SerializeField] private TextMeshProUGUI ScoreTextOpp;


    void Start()
    {
       GameManager.Instance.diceRoller = GameObject.Find("DieImage").GetComponent<DiceRoller>();
       PlayersManager.Instance.onSetDataInTurn += Instance_SetDataInTurn;
        MultiplayerManager.Instance.onTurnMe += Instance_onTurnMe;
        PlayersManager.Instance.onSetDataInRowMe += Instance_onSetDataInRowMe;
        PlayersManager.Instance.onSetDataInRowOpp += Instance_onSetDataInRowOpp;
        PlayersManager.Instance.onSetScoreOpp += Instance_onSetScoreOpp;
        PlayersManager.Instance.onSetScoreMe += Instance_onSetScoreMe;
   

        PlayersManager.Instance.IsTurn += Instance_IsTurn;
        if (MultiplayerManager.Instance.isTurn)
        {
            dicRollButton.interactable = true;
        }
    }

    private void Instance_onSetScoreMe(int obj)
    {
        ScoreTextMe.text = obj.ToString();
    }

    private void Instance_onSetScoreOpp(int obj)
    {
        ScoreTextOpp.text = obj.ToString();
    }

    private void Instance_onSetDataInRowOpp(int arg1, int arg2 )
    {
        Debug.Log(arg1 + " " + arg2);
       var clone= tileDataOpps.Find(e=>e.line == arg1 && e.row == arg2);
        clone.GetComponentsInChildren<Image>()[1].sprite = null;

    }

    private void Instance_onSetDataInRowMe(int arg1, int arg2 )
    {
        Debug.Log(arg1 + " " + arg2);
        var meCell =tileDataMe.Find(r =>  r.numberLine == arg1 && r.numberRow == arg2);
        meCell.GetComponentsInChildren<Image>()[1].sprite = null;
        meCell.isLock = false;
          
    }

    private void Instance_SetDataInTurn(DataPlayer obj)
    {
      
        if (obj.UserId != MultiplayerManager.Instance.players.User.Id)
        {
            transformOpp.Find(obj.NameTile).GetComponentsInChildren<Image>()[1].sprite = GameManager.Instance.diceRoller.Dice[obj.NumberTile];
          
        }
      
    }

    private void OnDestroy()
    {
        PlayersManager.Instance.IsTurn -= Instance_IsTurn;
    }

    private void Instance_IsTurn(bool obj)
    {
        if (obj)
        {
            dicRollButton.interactable = true;
            MultiplayerManager.Instance.isTurn=true;

           
        }
        else
        {
          
            dicRollButton.interactable = false;
            MultiplayerManager.Instance.isTurn = false;
        }
    }

    private void Instance_onTurnMe()
    {
        dicRollButton.interactable = true;


    }
    public void Leave()
    {
        MultiplayerManager.Instance.LeaveMatchAsync();
    }
  

}
