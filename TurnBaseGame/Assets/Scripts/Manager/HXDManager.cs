using Nakama.Helpers;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
public class HXDManager :MonoBehaviour
{

    public int HXDAmount;
    public TextMeshProUGUI textHXD;
    public static HXDManager instance;

    private void Awake()
    {
        instance = this;    
    }
    // Start is called before the first frame update
    private IEnumerator Start()
    {
        NakamaStorageManager.Instance.UpdateCollectionObject(NakamaStorageManager.Instance.NakamaCollectionObjectWallet);
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
}
