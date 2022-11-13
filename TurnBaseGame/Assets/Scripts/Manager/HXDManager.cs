using Nakama.Helpers;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
public class HXDManager : Singelton<HXDManager>
{

    public int HXDAmount;
    public TextMeshProUGUI textHXD;


    // Start is called before the first frame update
    private IEnumerator Start()
    {

        var collction = NakamaStorageManager.Instance.NakamaCollectionObjectWallet.GetValue<WalletData>();
        if (collction ==null)
        {
            yield return new WaitForSeconds(1);
            collction = NakamaStorageManager.Instance.NakamaCollectionObjectWallet.GetValue<WalletData>();
        }
        NakamaStorageManager.Instance.wallet = collction;
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
  


}
