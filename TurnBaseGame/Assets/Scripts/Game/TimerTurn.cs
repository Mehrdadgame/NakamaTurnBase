using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class TimerTurn : MonoBehaviour
{
    public static TimerTurn instance;
    public TextMeshProUGUI TimerText;
    public bool TimerRunning = false;
    public bool TimerPause = false;
    public float TimerCount = 30;
    public event Action TimerStop;
    // Start is called before the first frame update
    private void Awake()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (TimerRunning && !TimerPause)
        {
            TimerCount -= Time.deltaTime * 1;
            TimerText.text = TimerCount.ToString("F0")+"s";
            if (TimerCount <= 5)
            {
                TimerText.color = Color.red;
            }
            if (TimerCount <= 0)
            {
                TimerStop.Invoke();
                TimerCount = 30;
                TimerRunning=false;
                TimerText.color = Color.white;
            }
        }
    }
}
