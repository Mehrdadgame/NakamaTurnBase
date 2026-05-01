using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject that holds all 13 card definitions.
/// Create via Assets → Create → CardLibrary and assign sprites in the Inspector.
/// </summary>
[CreateAssetMenu(menuName = "Game/CardLibrary", fileName = "CardLibrary")]
public class CardLibrary : ScriptableObject
{
    public List<CardData> cards = new List<CardData>();

    public CardData GetById(int id)
    {
        foreach (var card in cards)
        {
            if (card.id == id) return card;
        }
        return null;
    }
}
