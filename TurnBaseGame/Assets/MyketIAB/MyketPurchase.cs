
using System.Collections.Generic;
using SimpleJSON;

#if UNITY_ANDROID

namespace MyketPlugin
{
    public class MyketPurchase
    {
        public enum MyketPurchaseState
        {
            Purchased,
            Canceled,
            Refunded
        }

        public string PackageName { get; private set; }
        public string OrderId { get; private set; }
        public string ProductId { get; private set; }
        public string DeveloperPayload { get; private set; }
        public string Type { get; private set; }
        public long PurchaseTime { get; private set; }
        public MyketPurchaseState PurchaseState { get; private set; }
        public string PurchaseToken { get; private set; }
        public string Signature { get; private set; }
        public string OriginalJson { get; private set; }

        public static List<MyketPurchase> fromJsonArray(JSONArray items)
        {
            var purchases = new List<MyketPurchase>();

            foreach (JSONNode item in items.AsArray)
            {
                MyketPurchase bPurchase = new MyketPurchase();
                bPurchase.fromJson(item.AsObject);
                purchases.Add(bPurchase);
            }

            return purchases;
        }

        public MyketPurchase() { }

        public void fromJson(JSONClass json)
        {
            PackageName = json["packageName"].Value;
            OrderId = json["orderId"].Value;
            ProductId = json["productId"].Value;
            DeveloperPayload = json["developerPayload"].Value;
            Type = json["type"].Value;
            PurchaseTime = long.Parse(json["purchaseTime"].Value);
            PurchaseState = (MyketPurchaseState)int.Parse(json["purchaseState"].Value);
            PurchaseToken = json["purchaseToken"].Value;
            Signature = json["signature"].Value;
            OriginalJson = json["originalJson"].Value;
        }

        public override string ToString()
        {
            return string.Format("<MyketPurchase> packageName: {0}, orderId: {1}, productId: {2}, developerPayload: {3}, purchaseToken: {4}, purchaseState: {5}, signature: {6}, type: {7}, json: {8}",
                PackageName, OrderId, ProductId, DeveloperPayload, PurchaseToken, PurchaseState, Signature, Type, OriginalJson);
        }
    }
}
#endif