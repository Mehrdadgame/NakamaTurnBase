using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject that holds all avatar definitions.
/// Create via Assets → Create → Game → AvatarLibrary.
/// Prices here are display-only — server is authoritative.
/// </summary>
[CreateAssetMenu(menuName = "Game/AvatarLibrary", fileName = "AvatarLibrary")]
public class AvatarLibrary : ScriptableObject
{
    public List<AvatarData> avatars = new List<AvatarData>();

    public AvatarData GetById(string id)
    {
        foreach (var a in avatars)
            if (a.id == id) return a;
        return null;
    }

    public Sprite GetSprite(string id)
    {
        var data = GetById(id);
        return data != null ? data.sprite : null;
    }
}
