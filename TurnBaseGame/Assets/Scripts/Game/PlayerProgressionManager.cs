using System;
using System.Collections;
using System.Collections.Generic;
using Nakama.Helpers;
using UnityEngine;

namespace NinjaBattle.Game
{
    [Serializable]
    public class ProgressionState
    {
        public int CurrentXp;
        public int CurrentLevel;

        public int XpIntoLevel => CurrentXp - GetXpThresholdForLevel(CurrentLevel);
        public int XpToNextLevel => CurrentLevel >= MaxLevel ? 0 : GetXpThresholdForLevel(CurrentLevel + 1) - CurrentXp;
        public string CurrentTitle => PlayerProgressionManager.GetTitleForLevel(CurrentLevel);

        public const int MaxLevel = 12;

        public static int GetXpThresholdForLevel(int level)
        {
            if (level <= 1) return 0;
            return 100 * level + 50 * (level - 1) * (level - 1);
        }
    }

    public class PlayerProgressionManager : MonoBehaviour
    {
        public static PlayerProgressionManager Instance { get; private set; }

        public event Action<int, int, string> OnLevelUp;
        public event Action<int, int, string> OnXpChanged;
        public event Action<ProgressionState> OnProgressionLoaded;

        [Header("Leveling")]
        [SerializeField] private int maxLevel = 12;

        public int CurrentXp { get; private set; }
        public int CurrentLevel { get; private set; }
        public string CurrentTitle => GetTitleForLevel(CurrentLevel);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            CurrentLevel = 1;
            CurrentXp = 0;
        }

        private void Start()
        {
            InitializeDefaultProgression();
            StartCoroutine(WaitForProfileService());
            StartCoroutine(WaitForMissionManager());
            StartCoroutine(WaitForPlayersManager());
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;

            if (ProfileService.Instance != null)
                ProfileService.Instance.onProgressionLoaded -= OnProfileProgressionLoaded;

            if (MissionManager.Instance != null)
            {
                MissionManager.Instance.OnMissionCompleted -= OnMissionCompleted;
                MissionManager.Instance.OnMissionProgressChanged -= OnMissionProgressChanged;
            }

            if (PlayersManager.Instance != null)
                PlayersManager.Instance.onMatchEnded -= OnMatchEnded;
        }

        private IEnumerator WaitForProfileService()
        {
            while (ProfileService.Instance == null)
                yield return null;

            ProfileService.Instance.onProgressionLoaded += OnProfileProgressionLoaded;
            if (ProfileService.Instance.PlayerProgression != null)
                OnProfileProgressionLoaded(ProfileService.Instance.PlayerProgression);
        }

        private IEnumerator WaitForMissionManager()
        {
            while (MissionManager.Instance == null)
                yield return null;

            MissionManager.Instance.OnMissionCompleted += OnMissionCompleted;
            MissionManager.Instance.OnMissionProgressChanged += OnMissionProgressChanged;
        }

        private IEnumerator WaitForPlayersManager()
        {
            while (PlayersManager.Instance == null)
                yield return null;

            PlayersManager.Instance.onMatchEnded += OnMatchEnded;
        }

        private void InitializeDefaultProgression()
        {
            CurrentXp = 0;
            CurrentLevel = 1;
            NotifyXpChanged();
        }

        private void OnProfileProgressionLoaded(ProfileProgressionData progression)
        {
            if (progression == null)
                return;

            CurrentXp = Mathf.Max(0, progression.currentXp);
            CurrentLevel = Mathf.Clamp(progression.currentLevel, 1, maxLevel);
            NotifyXpChanged();
            OnProgressionLoaded?.Invoke(new ProgressionState
            {
                CurrentXp = CurrentXp,
                CurrentLevel = CurrentLevel
            });
        }

        private void OnMatchEnded(MatchResult result)
        {
            if (result == null)
                return;

            int matchXp = CalculateMatchXp(result);
            AddXp(matchXp, saveNow: false);
            SaveProgression();
        }

        private void OnMissionCompleted(MissionState finishedMission)
        {
            if (finishedMission == null)
                return;

            AddXp(finishedMission.RewardXp, saveNow: false);
            SaveProgression();
        }

        private void OnMissionProgressChanged(MissionState mission)
        {
            if (mission == null)
                return;

            SaveProgression();
        }

        private int CalculateMatchXp(MatchResult result)
        {
            if (result.IsDraw)
                return 25;

            return result.IsLocalWinner ? 100 : 40;
        }

        public static string GetTitleForLevel(int level)
        {
            switch (Mathf.Clamp(level, 1, 12))
            {
                case 1: return "توریست";
                case 2: return "دست‌گرمی";
                case 3: return "امید به خدا";
                case 4: return "جویای نام";
                case 5: return "خوش‌شانس";
                case 6: return "پر ادعا";
                case 7: return "جفت‌شش‌زن";
                case 8: return "رویِ دورِ بُرد";
                case 9: return "اعصاب‌خردکن";
                case 10: return "قاتلِ میز";
                case 11: return "کابوسِ حریف";
                case 12: return "خدایگانِ تاس";
                default: return "توریست";
            }
        }

        public static int GetXpThresholdForLevel(int level)
        {
            if (level <= 1) return 0;
            return 100 * level + 50 * (level - 1) * (level - 1);
        }

        public void AddXp(int xpAmount, bool saveNow = true)
        {
            if (xpAmount <= 0)
                return;

            CurrentXp += xpAmount;
            int previousLevel = CurrentLevel;
            RecalculateLevel();
            NotifyXpChanged();

            if (CurrentLevel > previousLevel)
            {
                OnLevelUp?.Invoke(CurrentXp, CurrentLevel, CurrentTitle);
            }

            if (saveNow)
                SaveProgressionToServer();
        }

        public void LoadState(int xp, int level)
        {
            CurrentXp = Mathf.Max(0, xp);
            CurrentLevel = Mathf.Clamp(level, 1, maxLevel);
            NotifyXpChanged();
        }

        public void SetXp(int xpAmount)
        {
            CurrentXp = Mathf.Max(0, xpAmount);
            RecalculateLevel();
            NotifyXpChanged();
            SaveProgressionToServer();
        }

        public void SaveProgression()
        {
            SaveProgressionToServer();
        }

        private void RecalculateLevel()
        {
            int newLevel = 1;
            for (int level = 1; level <= maxLevel; level++)
            {
                if (CurrentXp >= GetXpThresholdForLevel(level))
                    newLevel = level;
                else
                    break;
            }
            CurrentLevel = Mathf.Clamp(newLevel, 1, maxLevel);
        }

        private void NotifyXpChanged()
        {
            OnXpChanged?.Invoke(CurrentXp, CurrentLevel, CurrentTitle);
        }

        private async void SaveProgressionToServer()
        {
            if (ProfileService.Instance == null)
                return;

            try
            {
                var payload = new ProgressionSavePayload
                {
                    currentXp = CurrentXp,
                    currentLevel = CurrentLevel,
                    missions = MissionManager.Instance?.GetMissionProgressPayloads() ?? new List<ProfileMissionProgress>()
                };

                await ProfileService.Instance.SaveProgressionAsync(payload);
            }
            catch (Exception e)
            {
                Debug.LogWarning("PlayerProgressionManager.SaveProgressionToServer failed: " + e.Message);
            }
        }

        [Serializable]
        public class ProgressionSavePayload
        {
            public int currentXp;
            public int currentLevel;
            public List<ProfileMissionProgress> missions;
        }
    }
}
