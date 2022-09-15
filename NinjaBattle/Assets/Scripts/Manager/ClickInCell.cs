using Nakama.Helpers;
using NinjaBattle.Game;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class ClickInCell : MonoBehaviour,IPointerDownHandler
{

    public int numberLine;
    public int numberRow;
    public bool isLock;
    public int ValueTile;


    /// <summary>
    /// 
    /// </summary>
    /// <param name="eventData"></param>
    public  void OnPointerDown(PointerEventData eventData)
    {

        if (!MultiplayerManager.Instance.isTurn || GameManager.Instance.diceRoller.currrentDie==-1 || isLock )
            return;
  
        MultiplayerManager.Instance.SendTurn(name, GameManager.Instance.diceRoller.currrentDie, numberLine, numberRow);
        var tile = GetComponentsInChildren<Image>()[1];
        tile.GetComponent<Animator>().Play("DiceRoot", 0, 0);
        tile.enabled = true;
        ValueTile = GameManager.Instance.diceRoller.currrentDie + 1;
        tile.sprite = GameManager.Instance.diceRoller.Dice[GameManager.Instance.diceRoller.currrentDie];
        GetComponentInChildren<ParticleSystem>().Stop();
        GameManager.Instance.diceRoller.RollUp?.Invoke(false);

       // UiManager.instance.RowSum();

        GameManager.Instance.diceRoller.Rotation(true);
        MultiplayerManager.Instance.isTurn = false;
        GameManager.Instance.diceRoller.currrentDie = -1;
        isLock = true;
    }


  


}

   

