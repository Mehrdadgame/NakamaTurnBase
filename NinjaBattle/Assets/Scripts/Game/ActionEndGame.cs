using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class ActionEndGame : MonoBehaviour
{
    public static ActionEndGame instance;
    public GameObject ResultPanel;
    public TextMeshProUGUI ResultText;
    public TextMeshProUGUI ScoreMe;
    public TextMeshProUGUI ScoreOpp;
    public Animator IconMe;
    public Animator IconOpp;
    public Button BackToHome;

    private void Start()
    {
        instance = this;
    }
}
