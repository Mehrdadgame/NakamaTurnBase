using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Nakama.Helpers;
using RTLTMPro;
using NinjaBattle.UI;

public class ActionEndGame : MonoBehaviour
{
    public static ActionEndGame instance;
    public GameObject ResultPanel;
    public RTLTextMeshPro ResultText;
    public RTLTextMeshPro ScoreMe;
    public RTLTextMeshPro ScoreOpp;
    public RTLTextMeshPro NameOpp;
    public Animator IconMe;
    public Animator IconOpp;
    public Button BackToHome;
    public GameResultPresentation ResultPresentation;

    private void Start()
    {
        instance = this;
        NameOpp.text = PlayerPrefs.GetString("Opp", "Opponent");
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    public void RefreshResultPresentation()
    {
        if (ResultPresentation != null)
            ResultPresentation.Refresh(ResultText != null ? ResultText.text : string.Empty);
    }
}
