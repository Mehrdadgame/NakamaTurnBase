using Nakama.Helpers;
using NinjaBattle.Game;
using System.Collections;
using System.Collections.Generic;
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

        if (!MultiplayerManager.Instance.isTurn || GameManager.Instance.diceRoller.currrentDie==-1 || isLock)
            return;
        ValueTile = GameManager.Instance.diceRoller.currrentDie + 1;
        MultiplayerManager.Instance.SendTurn(name, GameManager.Instance.diceRoller.currrentDie, numberLine, numberRow);
        var tile = GetComponentsInChildren<Image>()[1];
        tile.GetComponent<Animator>().Play("DiceRoot", 0, 0);
        tile.enabled = true;

        tile.sprite = GameManager.Instance.diceRoller.Dice[GameManager.Instance.diceRoller.currrentDie];
        MultiplayerManager.Instance.isTurn=false;
        GameManager.Instance.diceRoller.currrentDie = -1;
        isLock=true;
        GetComponentInChildren<ParticleSystem>().Stop();
        GameManager.Instance.diceRoller.RollUp?.Invoke(false);
    }


  


}

   

