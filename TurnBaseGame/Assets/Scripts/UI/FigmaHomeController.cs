using DG.Tweening;
using Nakama.Helpers;
using UnityEngine;
using UnityEngine.UI;

namespace NinjaBattle.UI
{
    public sealed class FigmaHomeController : MonoBehaviour
    {
        [SerializeField] private Button quickModeButton;
        [SerializeField] private Button professionalModeButton;
        [SerializeField] private Button masterModeButton;
        [SerializeField] private Button leaderboardButton;
        [SerializeField] private Button chestButton;
        [SerializeField] private GameObject shopPanel;
        [SerializeField] private GameObject profilePanel;
        [SerializeField] private GameObject missonPanel;

        [SerializeField] private GameObject leaderboardPanel;
        [SerializeField] private RectTransform startCta;
        [SerializeField] private RectTransform modePopup;
        [SerializeField] private CanvasGroup modePopupCanvasGroup;
        [SerializeField] private RectTransform activeHighlight;
        [SerializeField] private RectTransform[] navigationItems;

        private Tween highlightTween;
        private Tween popupTween;

        public void Configure(Button quick, Button professional, Button master, Button leaderboard, Button chest,
            GameObject shop, GameObject profile, GameObject leaderboardView)
        {
            quickModeButton = quick;
            professionalModeButton = professional;
            masterModeButton = master;
            leaderboardButton = leaderboard;
            chestButton = chest;
            shopPanel = shop;
            profilePanel = profile;
            leaderboardPanel = leaderboardView;
        }

        public void ConfigureNavigation(RectTransform highlight, params RectTransform[] items)
        {
            activeHighlight = highlight;
            navigationItems = items;
        }

        public void SetMissionsPanel(GameObject panel)
        {
            missonPanel = panel;
        }

        public void ConfigureModePopup(RectTransform cta, RectTransform popup, CanvasGroup canvasGroup)
        {
            startCta = cta;
            modePopup = popup;
            modePopupCanvasGroup = canvasGroup;
        }

        public void OpenModePopup()
        {
            if (modePopup == null)
                return;

            startCta?.DOKill();
            startCta?.DOPunchScale(Vector3.one * -0.055f, 0.2f, 5, 0.5f).SetUpdate(true);
            popupTween?.Kill();
            modePopup.gameObject.SetActive(true);
            modePopup.localScale = Vector3.one;
            if (modePopupCanvasGroup != null)
                modePopupCanvasGroup.alpha = 0f;

            popupTween = modePopupCanvasGroup != null
                ? modePopupCanvasGroup.DOFade(1f, 0.2f).SetEase(Ease.OutQuad).SetUpdate(true)
                : null;
        }

        public void CloseModePopup()
        {
            if (modePopup == null || !modePopup.gameObject.activeSelf)
                return;

            popupTween?.Kill();
            if (modePopupCanvasGroup == null)
            {
                modePopup.gameObject.SetActive(false);
                return;
            }

            popupTween = modePopupCanvasGroup.DOFade(0f, 0.14f)
                .SetEase(Ease.InQuad)
                .SetUpdate(true)
                .OnComplete(() => modePopup.gameObject.SetActive(false));
        }

        public void SelectQuickMode() => LaunchMode(quickModeButton);
        public void SelectProfessionalMode() => LaunchMode(professionalModeButton);
        public void SelectMasterMode() => LaunchMode(masterModeButton);

        public void OpenChest() => chestButton?.onClick.Invoke();

        public void SelectStore()
        {
            SelectNavigation(0);
            OpenShop();
        }

        public void SelectCards()
        {
            ShowHomeLayer();
            SelectNavigation(1);
        }

        public void SelectHome()
        {
            ShowHomeLayer();
            SelectNavigation(2);
        }

        public void SelectEvents()
        {
            SelectNavigation(3);
            OpenMissions();
        }

        public void OpenMissions()
        {
            CloseModePopup();
            SetPanelActive(shopPanel, false);
            SetPanelActive(leaderboardPanel, false);
            SetPanelActive(profilePanel, false);
            SetPanelActive(missonPanel, true);
        }

        public void CloseMissions()
        {
            SetPanelActive(missonPanel, false);
            SelectNavigation(2);
        }

        public void SelectLeaderboard()
        {
            SelectNavigation(4);
            if (leaderboardPanel == null)
                return;

            CloseModePopup();
            SetPanelActive(shopPanel, false);
            SetPanelActive(profilePanel, false);
            SetPanelActive(missonPanel, false);
            bool alreadyOpen = leaderboardPanel.activeSelf;
            leaderboardPanel.SetActive(true);
            if (alreadyOpen)
                leaderboardPanel.GetComponent<LeaderboardManager>()?.RefreshCurrent();
        }

        public void CloseLeaderboard()
        {
            if (leaderboardPanel != null)
                leaderboardPanel.SetActive(false);
            SelectNavigation(2);
        }

        public void OpenShop()
        {
            CloseModePopup();
            SetPanelActive(leaderboardPanel, false);
            SetPanelActive(profilePanel, false);
            SetPanelActive(missonPanel, false);
            SetPanelActive(shopPanel, true);
        }

        public void CloseShop()
        {
            if (shopPanel != null)
                shopPanel.SetActive(false);
            SelectNavigation(2);
        }

        public void OpenLeaderboardFromShop()
        {
            if (shopPanel != null)
                shopPanel.SetActive(false);
            SelectLeaderboard();
        }

        public void OpenProfile()
        {
            CloseModePopup();
            SetPanelActive(shopPanel, false);
            SetPanelActive(leaderboardPanel, false);
            SetPanelActive(missonPanel, false);
            SetPanelActive(profilePanel, true);
        }

        public void CloseProfile()
        {
            SetPanelActive(profilePanel, false);
            SelectNavigation(2);
        }

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Escape))
                return;

            if (modePopup != null && modePopup.gameObject.activeSelf)
                CloseModePopup();
            else if (missonPanel != null && missonPanel.activeSelf)
                CloseMissions();
            else if (profilePanel != null && profilePanel.activeSelf)
                CloseProfile();
            else if (leaderboardPanel != null && leaderboardPanel.activeSelf)
                CloseLeaderboard();
            else if (shopPanel != null && shopPanel.activeSelf)
                CloseShop();
        }

        private void LaunchMode(Button modeButton)
        {
            if (modeButton == null)
                return;

            if (modePopupCanvasGroup != null)
            {
                modePopupCanvasGroup.DOKill();
                modePopupCanvasGroup.DOFade(0.8f, 0.08f).SetLoops(2, LoopType.Yoyo).SetUpdate(true)
                    .OnComplete(() => modeButton.onClick.Invoke());
            }
            else
            {
                modeButton.onClick.Invoke();
            }
        }

        private void SelectNavigation(int index)
        {
            if (activeHighlight == null || navigationItems == null || index < 0 || index >= navigationItems.Length)
                return;

            RectTransform item = navigationItems[index];
            float targetX = item.anchoredPosition.x + (item.rect.width - activeHighlight.rect.width) * 0.5f;
            highlightTween?.Kill();
            highlightTween = activeHighlight.DOAnchorPosX(targetX, 0.32f)
                .SetEase(Ease.OutBack, 1.2f)
                .SetUpdate(true);

            item.DOKill();
            item.DOPunchScale(Vector3.one * 0.075f, 0.25f, 6, 0.45f).SetUpdate(true);
        }

        private void ShowHomeLayer()
        {
            CloseModePopup();
            SetPanelActive(shopPanel, false);
            SetPanelActive(leaderboardPanel, false);
            SetPanelActive(profilePanel, false);
            SetPanelActive(missonPanel, false);
        }

        private static void SetPanelActive(GameObject panel, bool active)
        {
            if (panel != null && panel.activeSelf != active)
                panel.SetActive(active);
        }

        private void OnDestroy()
        {
            highlightTween?.Kill();
            popupTween?.Kill();
            startCta?.DOKill();
            modePopup?.DOKill();
            if (navigationItems == null)
                return;
            foreach (RectTransform item in navigationItems)
                item?.DOKill();
        }
    }
}
