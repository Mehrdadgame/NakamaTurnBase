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
    void Start()
    {

      
        MultiplayerManager.Instance.onTurnMe += Instance_onTurnMe;
        idText.text = MultiplayerManager.Instance.players.User.Id;

        PlayersManager.Instance.IsTurn += Instance_IsTurn;
        if (MultiplayerManager.Instance.isTurn)
        {
            dicRollButton.interactable = true;
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
        }
        else
        {
            dicRollButton.interactable = false;
        }
    }

    private void Instance_onTurnMe()
    {
        dicRollButton.interactable = true;
    }
    public void SendTurn()
    {
        MultiplayerManager.Instance.Send(MultiplayerManager.Code.ChosseTurn,MultiplayerManager.Instance.players.User.Id);
        Debug.Log(MultiplayerManager.Instance.players.User.Id);
        dicRollButton.interactable = false;
    }

}
