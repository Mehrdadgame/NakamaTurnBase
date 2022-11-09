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
        var collction =NakamaStorageManager.Instance.NakamaCollectionObjectWallet.GetValue<WalletData>();
        NakamaStorageManager.Instance.wallet = collction;
        Debug.Log(collction.hxdAmount);
        HXDAmount = collction.hxdAmount;
        textHXD.text = HXDAmount.ToString();

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
