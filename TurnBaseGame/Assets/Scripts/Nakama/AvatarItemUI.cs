using RTLTMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nakama.Helpers
{
    /// <summary>
    /// One cell in the avatar selection popup grid.
    /// States:
    ///   - Free / Owned  → selectable, label shows "Free" or "Owned"
    ///   - Not Owned     → selectable (will buy on confirm), label shows "{price} Coin"
    ///   - Selected ring → shown on the active avatar
    /// </summary>
    public class AvatarItemUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image           avatarImage;
        [SerializeField] private RTLTextMeshPro priceLabel;
        [SerializeField] private GameObject      selectedRing;   // orange/gold border when selected
        [SerializeField] private GameObject      ownedBadge;     // small "✓" badge (optional)
        [SerializeField] private Button          selectButton;

        private AvatarData         _data;
        private AvatarPopupManager _popup;

        /// <summary>
        /// Initialize the cell.
        /// isSelected  = this is the currently equipped avatar
        /// isOwned     = player has purchased (or it is free)
        /// </summary>
        public void Init(AvatarData data, AvatarPopupManager popup, bool isSelected, bool isOwned)
        {
            _data  = data;
            _popup = popup;

            if (avatarImage != null) avatarImage.sprite = data.sprite;

            // Use server price if available, fall back to library price
            int price = ProfileService.Instance != null
                ? ProfileService.Instance.GetPrice(data.id)
                : data.price;

            // Label
            if (priceLabel != null)
            {
                if (price == 0)
                    priceLabel.text = "مجانی";
                else if (isOwned)
                    priceLabel.text = "خریدی";
                else
                    priceLabel.text = price + " تاسی";
            }

            if (selectedRing != null) selectedRing.SetActive(isSelected);
            if (ownedBadge   != null) ownedBadge.SetActive(isOwned && !isSelected);

            if (selectButton != null)
            {
                selectButton.onClick.RemoveAllListeners();
                selectButton.onClick.AddListener(OnClicked);
            }
        }

        public void SetSelected(bool selected)
        {
            if (selectedRing != null) selectedRing.SetActive(selected);
        }

        private void OnClicked() => _popup?.OnAvatarItemClicked(_data);
    }
}
