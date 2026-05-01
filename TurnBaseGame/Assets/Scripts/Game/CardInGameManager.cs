using System;
using System.Collections.Generic;
using Nakama.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages card usage during a battle. Attach to a persistent in-scene GameObject.
/// Displays the player's selected cards, handles activation flow, sends UseCard messages,
/// and reacts to opponent's card usage.
/// </summary>
public class CardInGameManager : MonoBehaviour
{
    #region FIELDS

    public static CardInGameManager Instance { get; private set; }

    [SerializeField] private CardLibrary    cardLibrary;
    [SerializeField] private Transform      cardContainer;      // parent for in-game card buttons
    [SerializeField] private GameObject     cardButtonPrefab;   // CardButton prefab with Image + Button
    [SerializeField] private TextMeshProUGUI shieldIndicator;   // shows "Shield Active" if needed
    [SerializeField] private GameObject     cardEffectOverlay;  // brief VFX panel for incoming cards

    private List<CardData>      heldCards    = new List<CardData>();
    private HashSet<int>        usedCardSlots = new HashSet<int>();
    private CardData            pendingCard  = null;
    private bool                awaitingTarget = false;

    // Events that UiManager can hook into
    public event Action<int>        onCardUsed;      // card id
    public event Action<int, int, int> onBreakerUsed; // targetLine confirmed by server
    public event Action<UseCardData>   onOpponentUsedCard;

    #endregion

    #region UNITY

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        BuildFromSelection();
        MultiplayerManager.Instance.Subscribe(MultiplayerManager.Code.UseCard, OnReceiveUseCard);
    }

    private void OnDestroy()
    {
        if (MultiplayerManager.Instance != null)
            MultiplayerManager.Instance.Unsubscribe(MultiplayerManager.Code.UseCard, OnReceiveUseCard);
    }

    #endregion

    #region SETUP

    private void BuildFromSelection()
    {
        heldCards.Clear();
        foreach (Transform child in cardContainer) Destroy(child.gameObject);

        var selected = CardShopManager.SelectedCardIds;
        for (int i = 0; i < selected.Count; i++)
        {
            var card = cardLibrary.GetById(selected[i]);
            if (card == null) continue;

            heldCards.Add(card);
            CreateCardButton(card, i);
        }
    }

    private void CreateCardButton(CardData card, int slotIndex)
    {
        var go  = Instantiate(cardButtonPrefab, cardContainer);
        var btn = go.GetComponentInChildren<Button>();
        var img = go.GetComponentInChildren<Image>();

        if (img != null && card.icon != null) img.sprite = card.icon;

        int capturedSlot = slotIndex;
        btn.onClick.AddListener(() => OnCardButtonPressed(capturedSlot));
    }

    #endregion

    #region CARD ACTIVATION

    private void OnCardButtonPressed(int slot)
    {
        if (usedCardSlots.Contains(slot)) return;
        if (awaitingTarget) return;

        pendingCard = heldCards[slot];

        // Cards that need a target line selected
        if (pendingCard.id == 6 || pendingCard.id == 8) // Breaker / ChaosSwap
        {
            awaitingTarget = true;
            Debug.Log("Select a target row for: " + pendingCard.cardName);
            return;
        }

        SendCard(slot, pendingCard, -1, -1, -1);
    }

    /// <summary>Call from UiManager when player taps a row while awaitingTarget = true.</summary>
    public bool TryConfirmTarget(int targetLine, int targetCol = -1, int targetValue = -1)
    {
        if (!awaitingTarget || pendingCard == null) return false;
        awaitingTarget = false;

        int slot = heldCards.IndexOf(pendingCard);
        if (slot < 0) { pendingCard = null; return false; }

        SendCard(slot, pendingCard, targetLine, targetCol, targetValue);
        return true;
    }

    private void SendCard(int slot, CardData card, int targetLine, int targetCol, int targetValue)
    {
        usedCardSlots.Add(slot);
        pendingCard = null;

        var data = new UseCardData
        {
            cardId        = card.id,
            senderUserId  = NakamaUserManager.Instance.User.Id,
            targetLine    = targetLine,
            targetCol     = targetCol,
            targetValue   = targetValue,
        };

        MultiplayerManager.Instance.Send(MultiplayerManager.Code.UseCard, data);
        ApplyLocalEffect(card, data);
        onCardUsed?.Invoke(card.id);

        // Grey out the card button
        RefreshButtonStates();
    }

    #endregion

    #region LOCAL EFFECTS (client-side)

    private void ApplyLocalEffect(CardData card, UseCardData data)
    {
        switch (card.id)
        {
            case 2:  // GuardianShield — UI feedback only, server tracks shield
                if (shieldIndicator != null) shieldIndicator.gameObject.SetActive(true);
                break;
            case 3:  // SecondChance — re-roll dice
                onCardUsed?.Invoke(card.id);
                break;
        }
    }

    #endregion

    #region RECEIVE OPPONENT CARD

    private void OnReceiveUseCard(MultiplayerMessage message)
    {
        var data = message.GetData<UseCardData>();
        if (data == null) return;

        // Countered — our card was blocked by opponent's CounterPulse
        if (data.countered)
        {
            Debug.Log("Your card was countered!");
            return;
        }

        ShowCardEffectOverlay();
        onOpponentUsedCard?.Invoke(data);

        // Client-visible effects of opponent's cards on our grid
        switch (data.cardId)
        {
            case 2:  // Opponent got a shield — show indicator on their side
                break;
            case 12: // Opponent armed CounterPulse
                break;
        }
    }

    private void ShowCardEffectOverlay()
    {
        if (cardEffectOverlay != null)
            StartCoroutine(FlashOverlay());
    }

    private System.Collections.IEnumerator FlashOverlay()
    {
        cardEffectOverlay.SetActive(true);
        yield return new WaitForSeconds(0.8f);
        cardEffectOverlay.SetActive(false);
    }

    #endregion

    #region HELPERS

    private void RefreshButtonStates()
    {
        int slot = 0;
        foreach (Transform child in cardContainer)
        {
            var btn = child.GetComponentInChildren<Button>();
            if (btn != null) btn.interactable = !usedCardSlots.Contains(slot);
            slot++;
        }
    }

    public bool IsAwaitingTarget => awaitingTarget;

    #endregion
}

[Serializable]
public class UseCardData
{
    public int    cardId;
    public string senderUserId;
    public int    targetLine  = -1;
    public int    targetCol   = -1;
    public int    targetValue = -1;
    public bool   countered   = false;
}
