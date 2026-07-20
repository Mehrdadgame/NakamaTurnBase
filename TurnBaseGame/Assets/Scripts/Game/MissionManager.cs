using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Nakama.Helpers;
using UnityEngine;

namespace NinjaBattle.Game
{
    public enum MissionGoalType
    {
        PlayMatches,
        WinMatches,
        PlaceTiles,
        ClearTiles
    }

    [Serializable]
    public class MissionDefinition
    {
        public string MissionId;
        public string Title;
        public string Description;
        public MissionGoalType GoalType;
        public int Target;
        public int RewardXp;
        public bool IsRepeatable;
    }

    [Serializable]
    public class MissionState
    {
        public string MissionId;
        public string Title;
        public string Description;
        public MissionGoalType GoalType;
        public int CurrentProgress;
        public int Target;
        public bool IsCompleted;
        public int RewardXp;
        public bool IsRepeatable;

        public float ProgressRatio => Target <= 0 ? 0f : Mathf.Clamp01((float)CurrentProgress / Target);

        public bool TryIncrement(int amount)
        {
            if (amount <= 0)
                return false;

            if (IsCompleted && !IsRepeatable)
                return false;

            CurrentProgress += amount;
            if (CurrentProgress >= Target)
            {
                if (IsRepeatable)
                {
                    CurrentProgress -= Target;
                    if (CurrentProgress < 0)
                        CurrentProgress = 0;
                }
                else
                {
                    CurrentProgress = Target;
                }

                IsCompleted = true;
                return true;
            }

            IsCompleted = false;
            return false;
        }
    }

    public class MissionManager : MonoBehaviour
    {
        public static MissionManager Instance { get; private set; }

        public event Action<MissionState> OnMissionProgressChanged;
        public event Action<MissionState> OnMissionCompleted;
        public event Action<IReadOnlyList<MissionState>> OnMissionsLoaded;

        private List<MissionDefinition> _definitions = new List<MissionDefinition>();

        private readonly List<MissionState> _missions = new List<MissionState>();

        public IReadOnlyList<MissionState> Missions => _missions.AsReadOnly();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            InitializeMissions();
            StartCoroutine(WaitForProfileService());
            StartCoroutine(WaitForPlayersManager());
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;

            if (ProfileService.Instance != null)
            {
                ProfileService.Instance.onProgressionLoaded -= OnProfileProgressionLoaded;
                ProfileService.Instance.onMissionDefinitionsLoaded -= OnMissionDefinitionsLoaded;
            }

            if (PlayersManager.Instance != null)
            {
                PlayersManager.Instance.onMatchEnded -= OnMatchEnded;
                PlayersManager.Instance.onSetDataInTurn -= OnSetDataInTurn;
                PlayersManager.Instance.onSetDataInRowMe -= OnSetDataInRowCleared;
                PlayersManager.Instance.onSetDataInRowOpp -= OnSetDataInRowCleared;
            }
        }

        private IEnumerator WaitForProfileService()
        {
            while (ProfileService.Instance == null)
                yield return null;

            ProfileService.Instance.onProgressionLoaded += OnProfileProgressionLoaded;
            ProfileService.Instance.onMissionDefinitionsLoaded += OnMissionDefinitionsLoaded;
            if (ProfileService.Instance.IsLoaded)
            {
                OnMissionDefinitionsLoaded(ProfileService.Instance.MissionDefinitions);
                OnProfileProgressionLoaded(ProfileService.Instance.PlayerProgression);
            }
        }

        private IEnumerator WaitForPlayersManager()
        {
            while (PlayersManager.Instance == null)
                yield return null;

            PlayersManager.Instance.onMatchEnded += OnMatchEnded;
            PlayersManager.Instance.onSetDataInTurn += OnSetDataInTurn;
            PlayersManager.Instance.onSetDataInRowMe += OnSetDataInRowCleared;
            PlayersManager.Instance.onSetDataInRowOpp += OnSetDataInRowCleared;
        }

        private void InitializeMissions()
        {
            _missions.Clear();
            foreach (var definition in _definitions)
            {
                _missions.Add(new MissionState
                {
                    MissionId = definition.MissionId,
                    Title = definition.Title,
                    Description = definition.Description,
                    GoalType = definition.GoalType,
                    Target = definition.Target,
                    RewardXp = definition.RewardXp,
                    IsRepeatable = definition.IsRepeatable,
                    CurrentProgress = 0,
                    IsCompleted = false
                });
            }

            OnMissionsLoaded?.Invoke(Missions);
        }

        private void OnMissionDefinitionsLoaded(List<ProfileMissionDefinition> defs)
        {
            if (defs == null || defs.Count == 0)
                return;

            _definitions.Clear();
            foreach (var d in defs)
            {
                if (!Enum.TryParse(d.goalType, true, out MissionGoalType goal))
                {
                    Debug.LogWarning($"MissionManager: unknown goal type '{d.goalType}' for mission '{d.missionId}'.");
                    continue;
                }

                _definitions.Add(new MissionDefinition
                {
                    MissionId = d.missionId,
                    Title = d.title,
                    Description = d.description,
                    GoalType = goal,
                    Target = d.target,
                    RewardXp = d.rewardXp,
                    IsRepeatable = d.isRepeatable
                });
            }

            InitializeMissions();

            // Re-apply saved progression if available
            if (ProfileService.Instance != null && ProfileService.Instance.PlayerProgression != null)
                OnProfileProgressionLoaded(ProfileService.Instance.PlayerProgression);
        }

        private void OnProfileProgressionLoaded(ProfileProgressionData progression)
        {
            if (progression == null)
                return;

            foreach (var mission in _missions)
            {
                var saved = progression.missions?.Find(entry => entry.missionId == mission.MissionId);
                if (saved == null)
                    continue;

                mission.CurrentProgress = Mathf.Clamp(saved.currentProgress, 0, mission.Target);
                mission.IsCompleted = saved.isCompleted || mission.CurrentProgress >= mission.Target;
            }

            OnMissionsLoaded?.Invoke(Missions);
        }

        private void OnMatchEnded(MatchResult result)
        {
            if (result == null)
                return;

            AddProgress(MissionGoalType.PlayMatches, 1);
            if (result.IsLocalWinner)
                AddProgress(MissionGoalType.WinMatches, 1);
        }

        private void OnSetDataInTurn(DataPlayer data)
        {
            if (data == null || string.IsNullOrEmpty(data.UserId))
                return;

            if (NakamaManager.Instance == null || MultiplayerManager.Instance == null)
                return;

            if (MultiplayerManager.Instance.Self != null &&
                data.UserId == MultiplayerManager.Instance.Self.UserId &&
                !data.EndGame)
                AddProgress(MissionGoalType.PlaceTiles, 1);
        }

        private void OnSetDataInRowCleared(int arg1, int arg2)
        {
            AddProgress(MissionGoalType.ClearTiles, 1);
        }

        public void AddProgress(MissionGoalType goalType, int amount = 1)
        {
            if (amount <= 0)
                return;

            foreach (var mission in _missions)
            {
                if (mission.GoalType != goalType)
                    continue;

                bool completed = mission.TryIncrement(amount);
                if (completed)
                {
                    OnMissionProgressChanged?.Invoke(mission);
                    OnMissionCompleted?.Invoke(mission);
                }
                else if (!mission.IsCompleted)
                {
                    OnMissionProgressChanged?.Invoke(mission);
                }
            }
        }

        public List<ProfileMissionProgress> GetMissionProgressPayloads()
        {
            return _missions.Select(m => new ProfileMissionProgress
            {
                missionId = m.MissionId,
                currentProgress = m.CurrentProgress,
                isCompleted = m.IsCompleted
            }).ToList();
        }
    }
}
