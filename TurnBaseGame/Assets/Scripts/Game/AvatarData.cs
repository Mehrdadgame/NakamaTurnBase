using UnityEngine;

[System.Serializable]
public class AvatarData
{
    /// <summary>
    /// Must match server AVATAR_PRICES keys: "avatar_0", "avatar_1", ...
    /// </summary>
    public string id;
    public Sprite sprite;
    /// <summary>
    /// 0 = free. Must match server-side AVATAR_PRICES.
    /// </summary>
    public int price;
}
