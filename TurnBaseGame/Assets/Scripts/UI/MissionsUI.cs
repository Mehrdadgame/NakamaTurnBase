using System.Collections;
using System.Collections.Generic;
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

        private void Awake()
        {
            foreach (RTLTextMeshPro text in GetComponentsInChildren<RTLTextMeshPro>(true))
                text.PreserveNumbers = true;

            if (openButton != null)
                openButton.onClick.AddListener(OpenPanel);
            if (closeButton != null)
                closeButton.onClick.AddListener(ClosePanel);

            ClosePanel();
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

        private void OnDestroy()
        {
            if (openButton != null)
                openButton.onClick.RemoveListener(OpenPanel);
            if (closeButton != null)
                closeButton.onClick.RemoveListener(ClosePanel);

            UnbindManagers();
        }

        public void OpenPanel()
        {
            if (missionPanel != null)
                missionPanel.SetActive(true);
        }

        public void ClosePanel()
        {
            if (missionPanel != null)
                missionPanel.SetActive(false);
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
                levelText.text = $"سطح {(currentLevel)}";
            if (titleText != null)
                titleText.text = string.IsNullOrWhiteSpace(currentTitle) ? "بازیکن" : currentTitle;
            if (xpText != null)
            {
                xpText.text = isMaxLevel
                    ? "بالاترین سطح"
                    : $"{(xpInsideLevel)} از {(xpSpan)} امتیاز";
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
                    ? "در حال دریافت مأموریت‌ها..."
                    : $"{(completedCount)} از {(totalCount)} انجام شده";
            }
        }


    }
}
