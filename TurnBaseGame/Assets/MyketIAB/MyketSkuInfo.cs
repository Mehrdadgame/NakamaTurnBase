
using System.Collections.Generic;
using SimpleJSON;

#if UNITY_ANDROID

namespace MyketPlugin
{
    public class MyketSkuInfo
    {
        public string Title { get; private set; }
        public string Price { get; private set; }
        public string Type { get; private set; }
        public string Description { get; private set; }
        public string ProductId { get; private set; }

        public static List<MyketSkuInfo> fromJsonArray(JSONArray items)
        {
            var skuInfos = new List<MyketSkuInfo>();

            foreach (JSONNode item in items.AsArray)
            {
                MyketSkuInfo bSkuInfo = new MyketSkuInfo();
                bSkuInfo.fromJson(item.AsObject);
                skuInfos.Add(bSkuInfo);
            }

            return skuInfos;
        }

        public MyketSkuInfo() { }

        public void fromJson(JSONClass json)
        {
            Title = json["title"].Value;
            Price = json["price"].Value;
            Type = json["type"].Value;
            Description = json["description"].Value;
            ProductId = json["productId"].Value;
        }

        public override string ToString()
        {
            return string.Format("<MyketSkuInfo> title: {0}, price: {1}, type: {2}, description: {3}, productId: {4}",
                Title, Price, Type, Description, ProductId);
        }

    }
}
#endif