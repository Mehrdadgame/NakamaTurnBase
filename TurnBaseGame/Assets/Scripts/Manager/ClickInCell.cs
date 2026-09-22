using Nakama.Helpers;
using NinjaBattle.Game;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

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
        DiceRoller diceRoller = GameManager.Instance.diceRoller;
        if (!MultiplayerManager.Instance.isTurn || !diceRoller.dieRolled ||
            diceRoller.currrentDie == -1 || isLock || diceRoller.isRolling)
            return;

        TutorialManager tutorial = TutorialManager.Instance;
        if (tutorial != null && !tutorial.CanPlaceCell(this))
        {
            tutorial.NotifyInvalidCell(this);
            return;
        }

        SetDataInCell();
    }

    public void SetDataInCell()
    {
        MultiplayerManager.Instance.SendTurn(name, GameManager.Instance.diceRoller.currrentDie, numberLine, numberRow);
        var tile = SpriteDice.transform.parent;
        tile.gameObject.SetActive(true);
        tile.DOKill();
        tile.localScale = Vector3.zero;
        tile.DOScale(Vector3.one, 0.28f).SetEase(Ease.OutBack).SetUpdate(true);

        SpriteDice.GetComponent<Animator>().Play("DiceRoot", 0, 0);
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



