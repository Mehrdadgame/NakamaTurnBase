using UnityEngine;

[System.Serializable]
public class CardData
{
    public int    id;
    public string cardName;
    [TextArea(2, 4)]
    public string description;
    public int    cost;
    public Sprite icon;
    public bool   isCounter;  // true for CounterPulse (max 1 per match)
}
