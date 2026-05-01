using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents one row in the card shop.
/// Assign references in the prefab Inspector.
/// </summary>
public class CardShopItem : MonoBehaviour
{
    [SerializeField] private Image            iconImage;
    [SerializeField] private TextMeshProUGUI  nameText;
    [SerializeField] private TextMeshProUGUI  descText;
    [SerializeField] private TextMeshProUGUI  costText;
    [SerializeField] private TextMeshProUGUI  ownedText;
    [SerializeField] private Button           buyButton;
    [SerializeField] private Button           selectButton;

    private CardData     card;
    private Action<int>  onBuy;
    private Action<int>  onSelect;

    public void Setup(CardData cardData, int ownedCount, Action<int> buyCallback, Action<int> selectCallback)
    {
        card     = cardData;
        onBuy    = buyCallback;
        onSelect = selectCallback;

        if (iconImage != null && cardData.icon != null) iconImage.sprite = cardData.icon;
        if (nameText  != null) nameText.text  = cardData.cardName;
        if (descText  != null) descText.text  = cardData.description;
        if (costText  != null) costText.text  = cardData.cost + " coins";
        if (ownedText != null) ownedText.text = "Owned: " + ownedCount;

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() => onBuy?.Invoke(card.id));

        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(() => onSelect?.Invoke(card.id));
        selectButton.interactable = ownedCount > 0;
    }
}
