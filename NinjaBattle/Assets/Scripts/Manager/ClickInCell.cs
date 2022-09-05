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
    public  void OnPointerDown(PointerEventData eventData)
    {

        if (!MultiplayerManager.Instance.isTurn || GameManager.Instance.diceRoller.currrentDie==-1)
            return;

        MultiplayerManager.Instance.SendTurn(name, GameManager.Instance.diceRoller.currrentDie, numberLine, numberRow);

        GetComponentsInChildren<Image>()[1].sprite = GameManager.Instance.diceRoller.Dice[GameManager.Instance.diceRoller.currrentDie];
        MultiplayerManager.Instance.isTurn=false;
        GameManager.Instance.diceRoller.currrentDie = -1;
    }


  


}

   

