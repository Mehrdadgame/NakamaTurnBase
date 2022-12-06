using NinjaBattle.Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* This class is a Singleton that holds the current game mode */
public class SetModeGame : MonoBehaviour
{
    public ModeGame modeGame;
    [SerializeField] private ChooseGameMode chooseGameMode;
    public void SetMode()
    {
        GameManager.Instance.modeGame = modeGame;
    }
}
