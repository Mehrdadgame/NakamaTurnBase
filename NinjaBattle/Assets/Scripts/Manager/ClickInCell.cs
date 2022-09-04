using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class ClickInCell : MonoBehaviour,IPointerDownHandler
{
    public async void OnPointerDown(PointerEventData eventData)
    {
        //if (GameManager.instance.isTurn)
        //{
        //    GetComponentsInChildren<Image>()[1].sprite = GameManager.instance.diceRoller.Dice[GameManager.instance.diceRoller.currrentDie];

        //    var data = new DataForSendServer();
        //    data.NameSprite = this.name;
        //    data.DiceNumber = GameManager.instance.diceRoller.currrentDie.ToString();
        //    var jsonData = JsonUtility.ToJson(data);
        //  await  GameManager.instance.SendMatchStateAsync(1,jsonData);
        //}
    }

   

 
   
}
