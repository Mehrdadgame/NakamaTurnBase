using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Nakama.Helpers;

/* This class is used to display the result of the game. */
public class ActionEndGame : MonoBehaviour
{
    public static ActionEndGame instance;
    public GameObject ResultPanel;
    public TextMeshProUGUI ResultText;
    public TextMeshProUGUI ScoreMe;
    public TextMeshProUGUI ScoreOpp;
    public TextMeshProUGUI NameOpp;
    public Animator IconMe;
    public Animator IconOpp;
    public Button BackToHome;

    private void Start()
    {
        instance = this;
        NameOpp.text = PlayerPrefs.GetString("Opp");
    }
}
