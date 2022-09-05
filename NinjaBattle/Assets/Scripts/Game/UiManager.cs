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
    [SerializeField] private Sprite[] dicesSprite;
    [SerializeField] private Transform transformOpp;

    void Start()
    {
       GameManager.Instance.diceRoller = GameObject.Find("DieImage").GetComponent<DiceRoller>();
        PlayersManager.Instance.onSetDataInTurn += Instance_SetDataInTurn;
        MultiplayerManager.Instance.onTurnMe += Instance_onTurnMe;
        idText.text = MultiplayerManager.Instance.players.User.Id;

        PlayersManager.Instance.IsTurn += Instance_IsTurn;
        if (MultiplayerManager.Instance.isTurn)
        {
            dicRollButton.interactable = true;
        }
    }

    private void Instance_SetDataInTurn(DataPlayer obj)
    {
     
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
