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

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if (NameOpp != null && string.IsNullOrEmpty(NameOpp.text))
            NameOpp.text = PlayerPrefs.GetString("Opp", "sohrab ۱");
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    public void RefreshResultPresentation()
    {
        // RTLTextMeshPro's .text getter returns the RESHAPED glyph string (reversed,
        // presentation forms), so Contains("برد") never matched and the panel always
        // showed the lose state. OriginalText is the raw string that was assigned.
        if (ResultPresentation != null)
            ResultPresentation.Refresh(ResultText != null ? ResultText.OriginalText : string.Empty);
    }
}
