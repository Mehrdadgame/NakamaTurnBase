using System;
using System.Collections;
using DG.Tweening;
using Nakama;
using Nakama.Helpers;
using NinjaBattle.Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using RTLTMPro;
namespace Game
{
    public class UiManagerHome : MonoBehaviour
    {
        public static UiManagerHome instance;

        public GameObject cellLeaderboard;

        public Transform parentPos;

        public RTLTextMeshPro Cointext;

        [Header("Avatar")]
        [SerializeField] private Image avatarImage;   // player avatar shown on home screen

        [Header("Profile")]
        [SerializeField] private RTLTextMeshPro displayNameText;  // player name on home screen

        private int _displayedCoins = -1;
        private Tweener _coinTween;
        // Start is called before the first frame update
        private void Awake()
        {
            instance = this;
        }
        private void OnEnable()
        {
            if (WalletManager.Instance != null)
            {
                WalletManager.Instance.onCoinsChanged += RefreshCoinsDisplay;
                // Wallet may have already loaded before we subscribed — show current value immediately
                RefreshCoinsDisplay(WalletManager.Instance.Coins);
            }
            else
            {
                StartCoroutine(WaitForWallet());
            }

            // Avatar + DisplayName — subscribe then immediately show current value if already loaded
            if (ProfileService.Instance != null)
            {
                ProfileService.Instance.onAvatarChanged      += RefreshAvatarDisplay;
                ProfileService.Instance.onDisplayNameChanged += RefreshDisplayName;
                if (ProfileService.Instance.IsLoaded)
                {
                    RefreshAvatarDisplay(ProfileService.Instance.CurrentAvatarId);
                    RefreshDisplayName(ProfileService.Instance.DisplayName);
                }
            }
            else
            {
                StartCoroutine(WaitForProfileService());
            }
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            _coinTween?.Kill();
            _displayedCoins = -1;
            if (WalletManager.Instance != null)
                WalletManager.Instance.onCoinsChanged -= RefreshCoinsDisplay;

            if (ProfileService.Instance != null)
            {
                ProfileService.Instance.onAvatarChanged      -= RefreshAvatarDisplay;
                ProfileService.Instance.onDisplayNameChanged -= RefreshDisplayName;
            }
        }

        private System.Collections.IEnumerator WaitForWallet()
        {
            while (WalletManager.Instance == null)
                yield return null;
            WalletManager.Instance.onCoinsChanged += RefreshCoinsDisplay;
            RefreshCoinsDisplay(WalletManager.Instance.Coins);
        }

        private System.Collections.IEnumerator WaitForProfileService()
        {
            while (ProfileService.Instance == null)
                yield return null;
            ProfileService.Instance.onAvatarChanged      += RefreshAvatarDisplay;
            ProfileService.Instance.onDisplayNameChanged += RefreshDisplayName;
            if (ProfileService.Instance.IsLoaded)
            {
                RefreshAvatarDisplay(ProfileService.Instance.CurrentAvatarId);
                RefreshDisplayName(ProfileService.Instance.DisplayName);
            }
        }

        private void RefreshCoinsDisplay(int amount)
        {
            if (Cointext == null) return;

            // First load — just set directly, no animation
            if (_displayedCoins < 0)
            {
                _displayedCoins = amount;
                Cointext.text = amount.ToString();
                return;
            }

            if (_displayedCoins == amount) return;

            // Animate counter up or down
            int from = _displayedCoins;
            _displayedCoins = amount;
            _coinTween?.Kill();
            float counter = from;
            _coinTween = DOTween.To(
                () => counter,
                x =>
                {
                    counter = x;
                    Cointext.text = Mathf.RoundToInt(x).ToString();
                },
                amount,
                0.8f
            ).SetEase(amount > from ? Ease.OutQuad : Ease.InQuad);
        }

        private void RefreshAvatarDisplay(string avatarId)
        {
            if (avatarImage == null || ProfileService.Instance == null) return;
            var sprite = ProfileService.Instance.GetSprite(avatarId);
            if (sprite != null) avatarImage.sprite = sprite;
        }

        private void RefreshDisplayName(string name)
        {
            if (displayNameText == null) return;
            displayNameText.text = string.IsNullOrWhiteSpace(name)
                ? (ProfileService.Instance != null ? ProfileService.Instance.ResolveDisplayNameOrUsername(null) : "")
                : name;
        }

        public void GetLeaderboard()
        {
          // yield return new WaitForSeconds(2);
          Debug.Log(NakamaManager.Instance.Session.Username);
            NakamaLeaderboard.instance.ShowGlobalLeaderboards(NakamaManager.Instance.Session);
        }

      
    }
}