using Nakama.Helpers;
using NinjaBattle.Game;
using NinjaBattle.General;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class ClickInCell : MonoBehaviour, IPointerDownHandler
{

    public int numberLine;
    public int numberRow;
    public bool isLock;
    public int ValueTile;
    public Image SpriteDice;

    /// <summary>
    /// Enter Pointerdown in cell 
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerDown(PointerEventData eventData)
    {

        if (!MultiplayerManager.Instance.isTurn || GameManager.Instance.diceRoller.currrentDie == -1 || isLock)
            return;
        SetDataInCell();
    }

    public void SetDataInCell()
    {
        MultiplayerManager.Instance.SendTurn(name, GameManager.Instance.diceRoller.currrentDie, numberLine, numberRow);
        var tile = SpriteDice.transform.parent;
        tile.gameObject.SetActive(true);
        SpriteDice.GetComponent<Animator>().Play("DiceRoot", 0, 0);
        AudioManager.Instance.PlayPointAudio(UiManager.instance.DiceSound);
        ValueTile = GameManager.Instance.diceRoller.currrentDie + 1;
        SpriteDice.sprite = GameManager.Instance.diceRoller.Dice[GameManager.Instance.diceRoller.currrentDie];
        GetComponentInChildren<ParticleSystem>().Stop();
        GameManager.Instance.diceRoller.RollUp?.Invoke(false);
        GameManager.Instance.diceRoller.Rotation(true);
        MultiplayerManager.Instance.isTurn = false;
        GameManager.Instance.diceRoller.currrentDie = -1;
        isLock = true;
       
    }




}



