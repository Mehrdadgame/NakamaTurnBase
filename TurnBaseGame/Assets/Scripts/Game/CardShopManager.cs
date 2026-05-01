using System;
using System.Collections;
using System.Collections.Generic;
using Nakama.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pre-match card shop UI. Attach to the shop panel GameObject.
/// Wire up: cardLibrary, shopContainer (parent for shop item rows),
/// shopItemPrefab, coinsText, selectedContainer, selectedItemPrefab,
/// and startMatchButton in the Inspector.
/// </summary>
public class CardShopManager : MonoBehaviour
{
    #region FIELDS

    private const string PurchaseCardRpc  = "PurchaseCardRpc";
    private const string GetCardsRpc      = "GetCardsRpc";
    private const int    MaxCardsPerMatch = 3;

    [SerializeField] private CardLibrary     cardLibrary;
    [SerializeField] private Transform       shopContainer;
    [SerializeField] private GameObject      shopItemPrefab;
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private Transform       selectedContainer;
    [SerializeField] private GameObject      selectedItemPrefab;
    [SerializeField] private Button          startMatchButton;

    private Dictionary<int, int> ownedCards = new Dictionary<int, int>();

    public static List<int> SelectedCardIds { get; private set; } = new List<int>();

    #endregion

    #region UNITY

    private void OnEnable()
    {
        if (WalletManager.Instance != null)
            WalletManager.Instance.onCoinsChanged += RefreshCoinsDisplay;
        RefreshCoinsDisplay();
        StartCoroutine(LoadShopCoroutine());
    }

    private void OnDisable()
    {
        if (WalletManager.Instance != null)
            WalletManager.Instance.onCoinsChanged -= RefreshCoinsDisplay;
    }

    #endregion

    #region SHOP

    private IEnumerator LoadShopCoroutine()
    {
        bool done = false;
        FetchOwnedCardsAsync(result => { ownedCards = result; done = true; });
        while (!done) yield return null;
        BuildShopUI();
    }

    private async void FetchOwnedCardsAsync(Action<Dictionary<int, int>> callback)
    {
        try
        {
            var result = await NakamaManager.Instance.SendRPC(GetCardsRpc, "{}");
            var data   = result.Payload.Deserialize<OwnedCardsRpcResult>();
            var dict   = new Dictionary<int, int>();
            if (data?.cards != null)
            {
                foreach (var kv in data.cards)
                {
                    if (int.TryParse(kv.Key, out int id))
                        dict[id] = kv.Value;
                }
            }
            callback(dict);
        }
        catch (Exception e)
        {
            Debug.LogWarning("GetCardsRpc failed: " + e.Message);
            callback(new Dictionary<int, int>());
        }
    }

    private void BuildShopUI()
    {
        foreach (Transform child in shopContainer) Destroy(child.gameObject);

        foreach (var card in cardLibrary.cards)
        {
            var item = Instantiate(shopItemPrefab, shopContainer);
            var ui   = item.GetComponent<CardShopItem>();
            if (ui == null) continue;

            int owned = ownedCards.ContainsKey(card.id) ? ownedCards[card.id] : 0;
            ui.Setup(card, owned, OnBuyCard, OnSelectCard);
        }
    }

    private void RefreshCoinsDisplay()
    {
        if (coinsText != null && WalletManager.Instance != null)
            coinsText.text = WalletManager.Instance.Coins + " coins";
    }

    private async void OnBuyCard(int cardId)
    {
        var card = cardLibrary.GetById(cardId);
        if (card == null) return;

        if (WalletManager.Instance != null && WalletManager.Instance.Coins < card.cost)
        {
            Debug.Log("Not enough coins for " + card.cardName);
            return;
        }

        try
        {
            var payload = JsonUtility.ToJson(new PurchaseCardPayload { cardId = cardId });
            var result  = await NakamaManager.Instance.SendRPC(PurchaseCardRpc, payload);
            var res     = result.Payload.Deserialize<PurchaseResult>();
            if (res != null && res.success)
            {
                ownedCards[cardId] = res.newCount;
                await WalletManager.Instance.RefreshAsync();
                BuildShopUI();
                RefreshSelectedDisplay();
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning("PurchaseCardRpc error: " + e.Message);
        }
    }

    private void OnSelectCard(int cardId)
    {
        var card = cardLibrary.GetById(cardId);
        if (card == null) return;
        if (SelectedCardIds.Contains(cardId)) return;
        if (SelectedCardIds.Count >= MaxCardsPerMatch) return;

        if (card.isCounter)
        {
            foreach (var id in SelectedCardIds)
            {
                var c = cardLibrary.GetById(id);
                if (c != null && c.isCounter) return;
            }
        }

        if (!ownedCards.ContainsKey(cardId) || ownedCards[cardId] <= 0) return;

        SelectedCardIds.Add(cardId);
        RefreshSelectedDisplay();
    }

    private void RefreshSelectedDisplay()
    {
        foreach (Transform child in selectedContainer) Destroy(child.gameObject);

        foreach (var cardId in SelectedCardIds)
        {
            var card = cardLibrary.GetById(cardId);
            if (card == null) continue;

            var item = Instantiate(selectedItemPrefab, selectedContainer);
            var img  = item.GetComponentInChildren<Image>();
            if (img != null && card.icon != null) img.sprite = card.icon;

            var btn = item.GetComponentInChildren<Button>();
            if (btn != null)
            {
                int capturedId = cardId;
                btn.onClick.AddListener(() => DeselectCard(capturedId));
            }
        }

        if (startMatchButton != null)
            startMatchButton.interactable = true;
    }

    private void DeselectCard(int cardId)
    {
        SelectedCardIds.Remove(cardId);
        RefreshSelectedDisplay();
    }

    public static void ClearSelection()
    {
        SelectedCardIds.Clear();
    }

    #endregion

    #region DATA MODELS

    [Serializable]
    private class OwnedCardsRpcResult
    {
        public Dictionary<string, int> cards;
    }

    [Serializable]
    private class PurchaseCardPayload
    {
        public int cardId;
    }

    [Serializable]
    private class PurchaseResult
    {
        public bool success;
        public int  cardId;
        public int  newCount;
    }

    #endregion
}
