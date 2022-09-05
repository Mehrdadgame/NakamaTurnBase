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
    [SerializeField] private TextMeshProUGUI idText;
    [SerializeField] private Transform transformOpp;
    [SerializeField] private List<TileDataOpp> tileDataOpps = new List<TileDataOpp>();
    [SerializeField] private List<ClickInCell> tileDataMe = new List<ClickInCell>();


    void Start()
    {
       GameManager.Instance.diceRoller = GameObject.Find("DieImage").GetComponent<DiceRoller>();
        PlayersManager.Instance.onSetDataInTurn += Instance_SetDataInTurn;
        MultiplayerManager.Instance.onTurnMe += Instance_onTurnMe;
        PlayersManager.Instance.onSetDataInRowMe += Instance_onSetDataInRowMe;
        PlayersManager.Instance.onSetDataInRowOpp += Instance_onSetDataInRowOpp;
        idText.text = MultiplayerManager.Instance.players.User.Id;

        PlayersManager.Instance.IsTurn += Instance_IsTurn;
        if (MultiplayerManager.Instance.isTurn)
        {
            dicRollButton.interactable = true;
        }
    }

    private void Instance_onSetDataInRowOpp(string arg1, string arg2)
    {
       var clone= tileDataOpps.Find(e=>e.line == int.Parse(arg2) && e.row == int.Parse(arg1));
        clone.GetComponentsInChildren<Image>()[1].sprite = null;
    }

    private void Instance_onSetDataInRowMe(string arg1, string arg2)
    {

        var meCell =tileDataMe.Find(r =>  r.numberRow == int.Parse(arg1) && r.numberLine == int.Parse(arg2));
        meCell.GetComponentsInChildren<Image>()[1].sprite = null;
    }

    private void Instance_SetDataInTurn(DataPlayer obj)
    {

          var imageTile=  tileDataOpps.Find(e => e.line == int.Parse( obj.ResultLine) && e.row == int.Parse( obj.ResultRow));
            imageTile.GetComponentsInChildren<Image>()[1].sprite = null;
       
        transformOpp.Find(obj.NameTile).GetComponentsInChildren<Image>()[1].sprite = GameManager.Instance.diceRoller.Dice[obj.NumberTile];
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
  

}
