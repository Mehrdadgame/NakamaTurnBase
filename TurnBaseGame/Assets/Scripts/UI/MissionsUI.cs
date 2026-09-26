using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Nakama.Helpers;
using NinjaBattle.Game;
using RTLTMPro;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NinjaBattle.UI
{
    public class MissionsUI : MonoBehaviour
    {
        [Header("Mission Panel")]
        [SerializeField] private GameObject missionPanel;
        [SerializeField] private Button openButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private RectTransform missionContainer;
        [SerializeField] private MissionItemView missionItemTemplate;
        [SerializeField] private RTLTextMeshPro missionSummaryText;

        [Header("Level HUD")]
        [SerializeField] private RTLTextMeshPro levelText;
        [SerializeField] private RTLTextMeshPro titleText;
        [SerializeField] private RTLTextMeshPro xpText;
        [SerializeField] private Image xpFill;

        private readonly List<MissionItemView> _missionItems = new List<MissionItemView>();
        private MissionManager _missionManager;
        private PlayerProgressionManager _progressionManager;
        private CanvasGroup _panelCanvasGroup;
        private RectTransform _panelContentRect;

        private void Awake()
        {
            foreach (RTLTextMeshPro text in GetComponentsInChildren<RTLTextMeshPro>(true))
                text.PreserveNumbers = true;

            if (openButton != null)
                openButton.onClick.AddListener(OpenPanel);

            if (closeButton != null)
                closeButton.onClick.AddListener(ClosePanel);

            ResolveMissionPanel();
            if (missionPanel != null)
                missionPanel.SetActive(false);
        }

        private void ResolveMissionPanel()
        {
            if (missionPanel == null)
            {
                Transform found = transform.Find("MissionPanel");
                if (found != null)
                    missionPanel = found.gameObject;
            }

            if (missionPanel != null)
            {
                _panelCanvasGroup = missionPanel.GetComponent<CanvasGroup>();
                if (_panelCanvasGroup == null)
                    _panelCanvasGroup = missionPanel.AddComponent<CanvasGroup>();

                Transform journal = missionPanel.transform.Find("JournalBackground");
                if (journal != null)
                    _panelContentRect = journal as RectTransform;
                else if (missionPanel.transform.childCount > 0)
                    _panelContentRect = missionPanel.transform.GetChild(0) as RectTransform;
                else
                    _panelContentRect = missionPanel.GetComponent<RectTransform>();
            }
        }

        private IEnumerator Start()
        {
            while (MissionManager.Instance == null || PlayerProgressionManager.Instance == null)
                yield return null;

            BindManagers();
            RefreshMissions(_missionManager.Missions);
            RefreshProgression(
                _progressionManager.CurrentXp,
                _progressionManager.CurrentLevel,
                _progressionManager.CurrentTitle);
        }

        private void OnEnable()
        {
            if (_missionManager != null)
                RefreshMissions(_missionManager.Missions);
            if (_progressionManager != null)
                RefreshProgression(
                    _progressionManager.CurrentXp,
                    _progressionManager.CurrentLevel,
                    _progressionManager.CurrentTitle);
        }

        private void OnDestroy()
        {
            if (openButton != null)
                openButton.onClick.RemoveListener(OpenPanel);
            if (closeButton != null)
                closeButton.onClick.RemoveListener(ClosePanel);

            if (_panelCanvasGroup != null)
                _panelCanvasGroup.DOKill();
            if (_panelContentRect != null)
                _panelContentRect.DOKill();

            UnbindManagers();
        }

        public void OpenPanel()
        {
            ResolveMissionPanel();
            if (missionPanel == null)
                return;

            missionPanel.SetActive(true);

            if (_panelCanvasGroup != null)
            {
                _panelCanvasGroup.DOKill();
                _panelCanvasGroup.alpha = 0f;
                _panelCanvasGroup.DOFade(1f, 0.25f).SetUpdate(true);
            }

            if (_panelContentRect != null)
            {
                _panelContentRect.DOKill();
                _panelContentRect.localScale = Vector3.one * 0.82f;
                _panelContentRect.DOScale(Vector3.one, 0.32f).SetEase(Ease.OutBack).SetUpdate(true);
            }

            if (_missionManager != null)
                RefreshMissions(_missionManager.Missions);
        }

        public void ClosePanel()
        {
            if (missionPanel == null || !missionPanel.activeSelf)
                return;

            if (_panelCanvasGroup != null && _panelContentRect != null)
            {
                _panelCanvasGroup.DOKill();
                _panelContentRect.DOKill();
                _panelContentRect.DOScale(Vector3.one * 0.85f, 0.18f).SetEase(Ease.InBack).SetUpdate(true);
                _panelCanvasGroup.DOFade(0f, 0.18f).SetUpdate(true).OnComplete(() =>
                {
                    missionPanel.SetActive(false);
                });
            }
            else
            {
                missionPanel.SetActive(false);
            }
        }

        private void BindManagers()
        {
            UnbindManagers();

            _missionManager = MissionManager.Instance;
            _progressionManager = PlayerProgressionManager.Instance;

            _missionManager.OnMissionsLoaded += RefreshMissions;
            _missionManager.OnMissionProgressChanged += RefreshMission;
            _missionManager.OnMissionCompleted += RefreshMission;
            _progressionManager.OnXpChanged += RefreshProgression;
            _progressionManager.OnLevelUp += RefreshProgression;
        }

        private void UnbindManagers()
        {
            if (_missionManager != null)
            {
                _missionManager.OnMissionsLoaded -= RefreshMissions;
                _missionManager.OnMissionProgressChanged -= RefreshMission;
                _missionManager.OnMissionCompleted -= RefreshMission;
            }

            if (_progressionManager != null)
            {
                _progressionManager.OnXpChanged -= RefreshProgression;
                _progressionManager.OnLevelUp -= RefreshProgression;
            }

            _missionManager = null;
            _progressionManager = null;
        }

        private void RefreshMissions(IReadOnlyList<MissionState> missions)
        {
            if (missionContainer == null || missionItemTemplate == null)
                return;

            ClearMissionItems();

            int completedCount = 0;
            foreach (MissionState mission in missions)
            {
                MissionItemView item = Instantiate(missionItemTemplate, missionContainer);
                item.gameObject.SetActive(true);
                item.Bind(mission);
                _missionItems.Add(item);

                if (mission.IsCompleted)
                    completedCount++;
            }

            UpdateSummary(completedCount, missions.Count);
            LayoutRebuilder.ForceRebuildLayoutImmediate(missionContainer);
        }

        private void RefreshMission(MissionState mission)
        {
            if (mission == null)
                return;

            foreach (MissionItemView item in _missionItems)
            {
                if (item.MissionId == mission.MissionId)
                {
                    item.Bind(mission);
                    UpdateSummaryFromItems();
                    return;
                }
            }

            if (_missionManager != null)
                RefreshMissions(_missionManager.Missions);
        }

        private void RefreshProgression(int currentXp, int currentLevel, string currentTitle)
        {
            int currentThreshold = PlayerProgressionManager.GetXpThresholdForLevel(currentLevel);
            bool isMaxLevel = currentLevel >= ProgressionState.MaxLevel;
            int nextThreshold = isMaxLevel
                ? currentThreshold
                : PlayerProgressionManager.GetXpThresholdForLevel(currentLevel + 1);

            int xpInsideLevel = Mathf.Max(0, currentXp - currentThreshold);
            int xpSpan = Mathf.Max(1, nextThreshold - currentThreshold);
            float ratio = isMaxLevel ? 1f : Mathf.Clamp01((float)xpInsideLevel / xpSpan);

            if (levelText != null)
                levelText.text = Localization.IsPersian ? $"سطح {ToPersianDigits(currentLevel)}" : $"Level {currentLevel}";
            if (titleText != null)
                titleText.text = string.IsNullOrWhiteSpace(currentTitle) ? Localization.L("بازیکن", "Player") : currentTitle;
            if (xpText != null)
            {
                xpText.text = isMaxLevel
                    ? Localization.L("بالاترین سطح", "Max level")
                    : Localization.IsPersian
                        ? $"{ToPersianDigits(xpInsideLevel)} از {ToPersianDigits(xpSpan)} امتیاز"
                        : $"{xpInsideLevel} of {xpSpan} XP";
            }
            if (xpFill != null)
                xpFill.fillAmount = ratio;
        }

        private void ClearMissionItems()
        {
            foreach (MissionItemView item in _missionItems)
            {
                if (item != null)
                    Destroy(item.gameObject);
            }

            _missionItems.Clear();
        }

        private void UpdateSummaryFromItems()
        {
            int completedCount = 0;
            foreach (MissionItemView item in _missionItems)
            {
                if (item != null && item.IsCompleted)
                    completedCount++;
            }

            UpdateSummary(completedCount, _missionItems.Count);
        }

        private void UpdateSummary(int completedCount, int totalCount)
        {
            if (missionSummaryText != null)
            {
                missionSummaryText.text = totalCount == 0
                    ? Localization.L("در حال دریافت مأموریت‌ها...", "Loading missions...")
                    : Localization.IsPersian
                        ? $"{ToPersianDigits(completedCount)} از {ToPersianDigits(totalCount)} انجام شده"
                        : $"{completedCount} of {totalCount} completed";
            }
        }

        private static string ToPersianDigits(int value)
        {
            if (!Localization.IsPersian) return value.ToString();
        return value.ToString().Replace('0', '۰').Replace('1', '۱').Replace('2', '۲')
                .Replace('3', '۳').Replace('4', '۴').Replace('5', '۵').Replace('6', '۶')
                .Replace('7', '۷').Replace('8', '۸').Replace('9', '۹');
        }
    }
}
