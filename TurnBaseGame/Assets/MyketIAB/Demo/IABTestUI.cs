using UnityEngine;

using MyketPlugin;

public class IABTestUI : MonoBehaviour
{
#if UNITY_ANDROID

    // Enter all the available skus from the Myket Developer Portal in this array so that item information can be fetched for them
    string[] skus = { "com.fanafzar.myketplugin.test1"
                , "com.fanafzar.myketplugin.test2"
                , "com.fanafzar.myketplugin.test3"
                , "com.fanafzar.myketplugin.monthly_subscribtion_test"
                , "com.fanafzar.myketplugin.annually_subscribtion_test"};

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10f, 10f, Screen.width - 15f, Screen.height - 15f));
        GUI.skin.button.fixedHeight = 50;
        GUI.skin.button.fontSize = 20;

        if (Button("Initialize IAB"))
        {
            var key = "MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCAUg0QXbGGu2vXQ2zinvvc6WKPxkax0ZMEsRdfh/DP7st8awKdyvWXSP761LPfH0jsF9rrYZWsA5Ov9iO1a3RyxfS5fVMKRGKTOuNW2N+t5rbpnf7ngBxHWUUlVSw1ODixYsoQ1AZ8fMjTOpcU72eR2yF0gAmW8hy5DYMXaMbiFwIDAQAB";
            MyketIAB.init(key);
        }

        if (Button("Query Inventory"))
        {
            MyketIAB.queryInventory(skus);
        }

        if (Button("Query SkuDetails"))
        {
            MyketIAB.querySkuDetails(skus);
        }

        if (Button("Query Purchases"))
        {
            MyketIAB.queryPurchases();
        }

        if (Button("Are subscriptions supported?"))
        {
            Debug.Log("subscriptions supported: " + MyketIAB.areSubscriptionsSupported());
        }

        if (Button("Purchase Product Test1"))
        {
            MyketIAB.purchaseProduct("com.fanafzar.myketplugin.test1");
        }

        if (Button("Purchase Product Test2"))
        {
            MyketIAB.purchaseProduct("com.fanafzar.myketplugin.test2");
        }

        if (Button("Consume Purchase Test1"))
        {
            MyketIAB.consumeProduct("com.fanafzar.myketplugin.test1");
        }

        if (Button("Consume Purchase Test2"))
        {
            MyketIAB.consumeProduct("com.fanafzar.myketplugin.test2");
        }

        if (Button("Consume Multiple Purchases"))
        {
            var skus = new string[] { "com.fanafzar.myketplugin.test1", "com.fanafzar.myketplugin.test2" };
            MyketIAB.consumeProducts(skus);
        }

        if (Button("Test Unavailable Item"))
        {
            MyketIAB.purchaseProduct("com.fanafzar.myketplugin.unavailable");
        }

        if (Button("Purchase Monthly Subscription"))
        {
            MyketIAB.purchaseProduct("com.fanafzar.myketplugin.monthly_subscribtion_test", "subscription payload");
        }

        if (Button("Purchase Annually Subscription"))
        {
            MyketIAB.purchaseProduct("com.fanafzar.myketplugin.annually_subscribtion_test", "subscription payload");
        }

        if (Button("Enable High Details Logs"))
        {
            MyketIAB.enableLogging(true);
        }

        GUILayout.EndArea();
    }

    bool Button(string label)
    {
        GUILayout.Space(5);
        return GUILayout.Button(label);
    }

#endif

}

