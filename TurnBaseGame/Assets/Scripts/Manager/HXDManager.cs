using Nakama.Helpers;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class HXDManager : Singelton<HXDManager>
{

    public int HXDAmount;
    public TextMeshProUGUI textHXD;


    // Start is called before the first frame update
    void Start()
    {
        if (!PlayerPrefs.HasKey("HXD"))
        {
            HXDAmount = 100000;
            PlayerPrefs.SetInt("HXD", HXDAmount);
            textHXD.text = HXDAmount.ToString();
        }
        else
        {
            HXDAmount = PlayerPrefs.GetInt("HXD");
            textHXD.text = HXDAmount.ToString();
        }
        ///Develop mode
        if (HXDAmount <= 0)
        {
            HXDAmount = 100000;
            PlayerPrefs.SetInt("HXD", HXDAmount);
            textHXD.text = HXDAmount.ToString();
        }
    }

  /// <summary>
  /// It takes an integer as a parameter and subtracts it from the current amount of HXD
  /// </summary>
  /// <param name="amunt">The amount of HXD to be set.</param>
  /// <returns>
  /// Nothing
  /// </returns>
    public void SetHXD(int amunt)
    {
        if (amunt > HXDAmount)
        {

            return;
        }
        HXDAmount -= amunt;
        PlayerPrefs.SetInt("HXD", HXDAmount);
        textHXD.text = HXDAmount.ToString();
        MultiplayerManager.Instance.ValueHXDInGameTurn = amunt;
    }


}
