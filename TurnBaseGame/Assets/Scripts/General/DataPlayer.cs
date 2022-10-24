using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum ModeGame
{
    ThreeByThree =3,
    FourByThree =4,
    VerticalAndHorizontal =5,
    FourByFour=6,
}
public class DataPlayer 
{

    public string UserId;
    public int Score;
    public int NumberTile;
    public string NameTile;
    public int NumberLine;
    public int NumberRow;
    public bool EndGame;
    public string PlayerWin;
    public int ScoreOtherPlayer;
    public bool MinesScore;
    public int ValueMines;
    public int[] sumRow1= new int [3];
    public int[] sumRow2 = new int[3];
    public bool master;
    public ModeGame modeGame;
    public int[][] Array2DTilesPlayer;
    public int[][] Array2DTilesOtherPlayer;

}

public class RematchData
{
    public string UserId;
    public string Answer;
}

