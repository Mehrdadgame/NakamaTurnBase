using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataPlayer 
{

    public string UserId;
    public int Score;
    public int NumberTile;
    public string NameTile;
    public int NumberLine;
    public int NumberRow;
    public int ResultLine;
    public int[] ResultRow;
    public bool EndGame;
    public string PlayerWin;
    public int ScoreOtherPlayer;
    public bool MinesScore;
    public int ValueMines;
    public int[] sumRow1= new int [3];
    public int[] sumRow2 = new int[3];
    public bool master;

}

public class RematchData
{
    public string UserId;
    public string Answer;
}

