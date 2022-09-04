using Nakama.Helpers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class UiManager : MonoBehaviour
{
    // Start is called before the first frame update

   [SerializeField] private Button dicRollButton;
    [SerializeField] private TextMeshProUGUI idText;
    void Start()
    {

      
        MultiplayerManager.Instance.onTurnMe += Instance_onTurnMe;
        idText.text = MultiplayerManager.Instance.players.User.Id;
        if (MultiplayerManager.Instance.isTurn)
        {
            dicRollButton.interactable = true;
        }
    }

    private void Instance_onTurnMe()
    {
        dicRollButton.interactable = true;
    }

}
