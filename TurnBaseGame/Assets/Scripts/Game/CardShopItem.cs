using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardShopItem : MonoBehaviour
{
    [SerializeField] private Image           iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Button          selectButton;
    [SerializeField] private TextMeshProUGUI buttonLabel;   // text on the select button

    private CardData    card;
    private Action<int> onSelect;

    public void Setup(CardData cardData, bool alreadySelected, Action<int> selectCallback)
    {
        card     = cardData;
        onSelect = selectCallback;

        if (iconImage != null && cardData.icon != null) iconImage.sprite = cardData.icon;
        if (nameText  != null) nameText.text = cardData.cardName;
        if (descText  != null) descText.text = cardData.description;
        if (costText  != null) costText.text = cardData.cost + " coins";

        selectButton.onClick.RemoveAllListeners();

        if (alreadySelected)
        {
            if (buttonLabel != null) buttonLabel.text = "Selected";
            selectButton.interactable = false;
        }
        else
        {
            if (buttonLabel != null) buttonLabel.text = "Select  " + cardData.cost + "c";
            selectButton.interactable = true;
            selectButton.onClick.AddListener(() => onSelect?.Invoke(card.id));
        }
    }
}
