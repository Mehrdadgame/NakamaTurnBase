using Nakama.Helpers;
using NinjaBattle.Game;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChooseGameMode : Singelton<ChooseGameMode>
{
    public ModeGame ModeGame;
    // public Buttons_THT_Coin buttons_THT_Coin;
    public List<Button> ButtonsModeGame = new();
    public GameObject BackgroundGameModePage;
    public RectTransform ButtonGame;
    public RectTransform rectButton;
    public Sprite[] spritesModeGame;
    public HXDManager hXDManager;
    public List<Info_Buttons_THT_Coin> LeaguButton = new();
    public List<Buttons_THT_Coin> buttons_THT_CoinsThreeByThree = new();
    public List<Buttons_THT_Coin> buttons_THT_CoinsThreeByFour = new();
    public List<Buttons_THT_Coin> buttons_THT_CoinsFourBYFour = new();
    public List<Buttons_THT_Coin> buttons_THT_CoinVertical = new();

   
    public void ActionGameMode()
    {
        ButtonGame.gameObject.SetActive(true);
        ButtonGame.GetComponent<Animator>().enabled = true;
        ButtonGame.GetComponent<Image>().sprite = spritesModeGame[ModeGame.GetHashCode() - 3];
        ButtonGame.position = new Vector2(rectButton.position.x, rectButton.position.y);
        ButtonGame.GetComponent<Animator>().Play("ModeGameUp");
        //GameManager.Instance.modeGame = GetComponent<SetModeGame>().modeGame;
        foreach (var item in ButtonsModeGame)
        {
            item.gameObject.SetActive(false);
        }
        BackgroundGameModePage.SetActive(true);
        switch (ModeGame)
        {
            case ModeGame.ThreeByThree:
                for (int i = 0; i < LeaguButton.Count; i++)
                {
                    LeaguButton[i].AmountTHTForStartGame.text = $"Entry Fee : { buttons_THT_CoinsThreeByThree[i].AmountTHTForStartGame}HXD";
                    LeaguButton[i].InfoAmountHXD = buttons_THT_CoinsThreeByThree[i].AmountTHTForStartGame;
                    LeaguButton[i].ButtonTHT.GetComponent<MultiplayerJoinMatchButton>().AmountHXDPlayer = buttons_THT_CoinsThreeByThree[i].AmountTHTForStartGame;
                }
                break;
            case ModeGame.FourByThree:
                for (int i = 0; i < LeaguButton.Count; i++)
                {
                    LeaguButton[i].AmountTHTForStartGame.text = $"Entry Fee : {buttons_THT_CoinsThreeByFour[i].AmountTHTForStartGame}HXD";
                    LeaguButton[i].InfoAmountHXD = buttons_THT_CoinsThreeByFour[i].AmountTHTForStartGame;
                    LeaguButton[i].ButtonTHT.GetComponent<MultiplayerJoinMatchButton>().AmountHXDPlayer = buttons_THT_CoinsThreeByFour[i].AmountTHTForStartGame;
                }
                break;
            case ModeGame.VerticalAndHorizontal:
                for (int i = 0; i < LeaguButton.Count; i++)
                {
                    LeaguButton[i].AmountTHTForStartGame.text = $"Entry Fee : {buttons_THT_CoinVertical[i].AmountTHTForStartGame}HXD";
                    LeaguButton[i].InfoAmountHXD = buttons_THT_CoinVertical[i].AmountTHTForStartGame;
                    LeaguButton[i].ButtonTHT.GetComponent<MultiplayerJoinMatchButton>().AmountHXDPlayer = buttons_THT_CoinVertical[i].AmountTHTForStartGame;

                }
                break;
            case ModeGame.FourByFour:
                for (int i = 0; i < LeaguButton.Count; i++)
                {
                    LeaguButton[i].AmountTHTForStartGame.text = $"Entry Fee : {buttons_THT_CoinsFourBYFour[i].AmountTHTForStartGame}HXD";
                    LeaguButton[i].InfoAmountHXD = buttons_THT_CoinsFourBYFour[i].AmountTHTForStartGame;
                    LeaguButton[i].ButtonTHT.GetComponent<MultiplayerJoinMatchButton>().AmountHXDPlayer = buttons_THT_CoinsFourBYFour[i].AmountTHTForStartGame;

                }
                break;
        }

    }
    public void BackActionGameMode()
    {
       
        // ButtonGame.position = new Vector2( rectButton.position.x, rectButton.position.y);
        ButtonGame.GetComponent<Animator>().Play("ModeGameUp Back");
        //GameManager.Instance.modeGame = GetComponent<SetModeGame>().modeGame;

        DelayRun();

       // ButtonGame.gameObject.SetActive(false);
    }

    private async void DelayRun()
    {
        await Task.Delay(450);
        ButtonGame.GetComponent<Animator>().enabled = false;
        ButtonGame.position = new Vector2(rectButton.position.x, rectButton.position.y);
        BackgroundGameModePage.SetActive(false);
        foreach (var item in ButtonsModeGame)
        {
            item.gameObject.SetActive(true);
        }
        ButtonGame.gameObject.SetActive(false);
    }


}
[System.Serializable]
public struct Buttons_THT_Coin
{
 
    public int AmountTHTForStartGame;
}
[System.Serializable]
public class Info_Buttons_THT_Coin
{
    public Button ButtonTHT;
    public TextMeshProUGUI AmountTHTForStartGame;
    public int InfoAmountHXD;
}

