using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using RTLTMPro;
using DG.Tweening;

public class TimerTurn : MonoBehaviour
{
    public static TimerTurn instance;
    public RTLTextMeshPro TimerText;
    public bool TimerRunning = false;
    public bool TimerPause = false;
    public float TimerCount = 30;
    public event Action TimerStop;

    private int _lastSecond = -1;

    private void Awake()
    {
        instance = this;
    }

    private void OnDisable()
    {
        ResetVisuals();
    }

    public void ResetVisuals()
    {
        if (TimerText != null)
        {
            TimerText.rectTransform.DOKill();
            TimerText.rectTransform.localScale = Vector3.one;
            TimerText.color = Color.white;
        }
        _lastSecond = -1;
    }

    void Update()
    {
        if (TimerRunning && !TimerPause)
        {
            TimerCount -= Time.deltaTime;
            int currentSecond = Mathf.Max(0, Mathf.CeilToInt(TimerCount));

            if (TimerText != null)
            {
                TimerText.text = currentSecond.ToString();

                if (TimerCount <= 5f)
                {
                    TimerText.color = Color.red;

                    if (currentSecond != _lastSecond && currentSecond > 0)
                    {
                        _lastSecond = currentSecond;
                        TimerText.rectTransform.DOKill();
                        TimerText.rectTransform.localScale = Vector3.one;
                        TimerText.rectTransform.DOPunchScale(Vector3.one * 0.35f, 0.35f, 5, 0.5f).SetUpdate(true);
                    }
                }
                else
                {
                    TimerText.color = Color.white;
                }
            }

            if (TimerCount <= 0)
            {
                TimerStop?.Invoke();
                TimerCount = 30;
                TimerRunning = false;
                ResetVisuals();
            }
        }
    }
}
