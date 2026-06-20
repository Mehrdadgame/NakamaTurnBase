using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

#if UNITY_ANDROID

namespace MyketPlugin
{
    public class IABEventManager : AbstractManager
    {
        // Fired after init is called when billing is supported on the device
        public static event Action billingSupportedEvent;

        // Fired after init is called when billing is not supported on the device
        public static event Action<string> billingNotSupportedEvent;

        // Fired when the inventory and purchase history query has returned
        public static event Action<List<MyketPurchase>, List<MyketSkuInfo>> queryInventorySucceededEvent;

        // Fired when the inventory and purchase history query fails
        public static event Action<string> queryInventoryFailedEvent;

        // Fired when the SkuDetails query has returned
        public static event Action<List<MyketSkuInfo>> querySkuDetailsSucceededEvent;

        // Fired when the SkuDetails query fails
        public static event Action<string> querySkuDetailsFailedEvent;

        // Fired when the purchase history query has returned
        public static event Action<List<MyketPurchase>> queryPurchasesSucceededEvent;

        // Fired when the purchase history query fails
        public static event Action<string> queryPurchasesFailedEvent;

        // Fired when a purchase succeeds
        public static event Action<MyketPurchase> purchaseSucceededEvent;

        // Fired when a purchase fails
        public static event Action<string> purchaseFailedEvent;

        // Fired when a call to consume a product succeeds
        public static event Action<MyketPurchase> consumePurchaseSucceededEvent;

        // Fired when a call to consume a product fails
        public static event Action<string> consumePurchaseFailedEvent;


        static IABEventManager()
        {
            initialize(typeof(IABEventManager));
        }

        public void billingSupported(string empty)
        {
            billingSupportedEvent.SafeInvoke();
        }

        public void billingNotSupported(string error)
        {
            billingNotSupportedEvent.SafeInvoke(error);
        }

        public void queryInventorySucceeded(string jsonStr)
        {
            JSONNode dataNode = JSON.Parse(jsonStr);

            JSONArray purchasesJsonArray = dataNode["purchases"].AsArray;
            var purchases = MyketPurchase.fromJsonArray(purchasesJsonArray);

            JSONArray skusJsonArray = dataNode["skus"].AsArray;
            var skus = MyketSkuInfo.fromJsonArray(skusJsonArray);

            queryInventorySucceededEvent.SafeInvoke(purchases, skus);
        }

        public void queryInventoryFailed(string error)
        {
            queryInventoryFailedEvent.SafeInvoke(error);
        }

        public void querySkuDetailsSucceeded(string jsonStr)
        {
            JSONNode dataNode = JSON.Parse(jsonStr);

            JSONArray skusJsonArray = dataNode.AsArray;
            var skus = MyketSkuInfo.fromJsonArray(skusJsonArray);

            querySkuDetailsSucceededEvent.SafeInvoke(skus);
        }

        public void querySkuDetailsFailed(string error)
        {
            querySkuDetailsFailedEvent.SafeInvoke(error);
        }

        public void queryPurchasesSucceeded(string jsonStr)
        {
            JSONNode dataNode = JSON.Parse(jsonStr);

            JSONArray purchasesJsonArray = dataNode.AsArray;
            var purchases = MyketPurchase.fromJsonArray(purchasesJsonArray);

            queryPurchasesSucceededEvent.SafeInvoke(purchases);
        }

        public void queryPurchasesFailed(string error)
        {
            queryPurchasesFailedEvent.SafeInvoke(error);
        }

        public void purchaseSucceeded(string jsonStr)
        {
            JSONNode dataNode = JSON.Parse(jsonStr);
            MyketPurchase myketPurchase = new MyketPurchase();
            myketPurchase.fromJson(dataNode.AsObject);

            purchaseSucceededEvent.SafeInvoke(myketPurchase);
        }

        public void purchaseFailed(string error)
        {
            purchaseFailedEvent.SafeInvoke(error);
        }

        public void consumePurchaseSucceeded(string jsonStr)
        {
            JSONNode dataNode = JSON.Parse(jsonStr);
            MyketPurchase myketPurchase = new MyketPurchase();
            myketPurchase.fromJson(dataNode.AsObject);

            consumePurchaseSucceededEvent.SafeInvoke(myketPurchase);
        }

        public void consumePurchaseFailed(string error)
        {
            consumePurchaseFailedEvent.SafeInvoke(error);
        }

    }
}

#endif
