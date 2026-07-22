using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NinjaBattle.Game;
using UnityEngine;

namespace Nakama.Helpers
{
    public class ProfileService : MonoBehaviour
    {
        public static ProfileService Instance { get; private set; }

        private const string GetProfileRpcId = "GetProfileRpc";
        private const string SaveProgressionRpcId = "SaveProgressionRpc";

        // ── Cached state ──────────────────────────────────────────────────────────
        public string       CurrentAvatarId { get; private set; } = "avatar_0";
        public string       DisplayName     { get; private set; } = "";
        public bool         IsLoaded        { get; private set; } = false;
        public List<string> OwnedAvatarIds  { get; private set; } = new List<string> { "avatar_0", "avatar_1" };
        public ProfileProgressionData PlayerProgression { get; private set; } = new ProfileProgressionData();
        public List<ProfileMissionDefinition> MissionDefinitions { get; private set; } = new List<ProfileMissionDefinition>();

        // ── Events ────────────────────────────────────────────────────────────────
        public event Action<string>       onAvatarChanged;
        public event Action<List<string>> onOwnedAvatarsChanged;
        public event Action<string>       onDisplayNameChanged;
        public event Action<ProfileProgressionData> onProgressionLoaded;
        public event Action<List<ProfileMissionDefinition>> onMissionDefinitionsLoaded;

        // Server-authoritative prices — used for display instead of AvatarLibrary.price
        private Dictionary<string, int> _serverPrices = new Dictionary<string, int>();

        // ── Inspector ─────────────────────────────────────────────────────────────
        [Header("Avatar Library (assign in Inspector)")]
        [SerializeField] private AvatarLibrary avatarLibrary;
        public AvatarLibrary AvatarLibrary => avatarLibrary;

        // ─────────────────────────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        private void Start() => StartCoroutine(WaitAndLoad());

        // ── Loading ───────────────────────────────────────────────────────────────
        private IEnumerator WaitAndLoad()
        {
            while (NakamaUserManager.Instance == null || !NakamaUserManager.Instance.LoadingFinished)
                yield return null;
            var task = LoadProfileAsync();
            yield return new WaitUntil(() => task.IsCompleted);
        }

        private async System.Threading.Tasks.Task LoadProfileAsync()
        {
            try
            {
                var rpc = await NakamaManager.Instance.SendRPC(GetProfileRpcId, "{}");
                if (rpc == null || string.IsNullOrEmpty(rpc.Payload))
                {
                    Debug.LogWarning("[ProfileService] GetProfileRpc returned an empty payload.");
                    return;
                }

                var data = rpc.Payload.Deserialize<ProfileServiceData>();
                if (data == null) return;

                string resolvedName = ResolveDisplayNameOrUsername(data.displayName);
                bool nameChanged = resolvedName != DisplayName;

                CurrentAvatarId = string.IsNullOrEmpty(data.avatarId) ? "avatar_0" : data.avatarId;
                DisplayName     = resolvedName;

                if (data.ownedAvatars != null && data.ownedAvatars.Count > 0)
                    OwnedAvatarIds = data.ownedAvatars;

                // Cache server-authoritative prices
                if (data.avatarPrices != null)
                {
                    _serverPrices.Clear();
                    foreach (var entry in data.avatarPrices)
                        _serverPrices[entry.id] = entry.price;
                }

                IsLoaded = true;
                PlayerProgression = data.progression ?? new ProfileProgressionData();
                MissionDefinitions = data.missionDefinitions ?? new List<ProfileMissionDefinition>();
                Debug.Log("[ProfileService] Loaded " + MissionDefinitions.Count + " mission definitions from Nakama.");
                if (MissionDefinitions.Count == 0)
                    Debug.LogWarning("[ProfileService] Nakama returned no mission definitions. Check MissionDefinitions storage and deployed runtime module.");

                // Always fire — subscribers may have missed earlier events
                onAvatarChanged?.Invoke(CurrentAvatarId);
                onOwnedAvatarsChanged?.Invoke(OwnedAvatarIds);
                if (nameChanged) onDisplayNameChanged?.Invoke(DisplayName);
                onProgressionLoaded?.Invoke(PlayerProgression);
                onMissionDefinitionsLoaded?.Invoke(MissionDefinitions);

                // GetProfileRpc may have granted the first-login bonus (3500 coins).
                // Refresh wallet AFTER the RPC completes so the UI shows the correct balance.
                if (WalletManager.Instance != null)
                    await WalletManager.Instance.RefreshAsync();
            }
            catch (Exception e)
            {
                Debug.LogWarning("[ProfileService] Load error: " + e.Message);
            }
        }

        // ── Public helpers ────────────────────────────────────────────────────────

        /// <summary>Price from server for display. Falls back to AvatarLibrary price.</summary>
        public int GetPrice(string avatarId)
        {
            if (_serverPrices.TryGetValue(avatarId, out int p)) return p;
            // fallback to library
            if (avatarLibrary != null)
            {
                var d = avatarLibrary.GetById(avatarId);
                if (d != null) return d.price;
            }
            return 0;
        }

        public bool IsOwned(string avatarId) => OwnedAvatarIds.Contains(avatarId);

        /// <summary>
        /// Uses displayName when available; otherwise falls back to the Nakama username.
        /// </summary>
        public string ResolveDisplayNameOrUsername(string rawDisplayName)
        {
            if (!string.IsNullOrWhiteSpace(rawDisplayName))
                return rawDisplayName.Trim();

            var userManager = NakamaUserManager.Instance;
            if (userManager != null && userManager.LoadingFinished)
            {
                var user = userManager.User;
                if (user != null)
                {
                    if (!string.IsNullOrWhiteSpace(user.DisplayName))
                        return user.DisplayName.Trim();
                    if (!string.IsNullOrWhiteSpace(user.Username))
                        return user.Username.Trim();
                }
            }

            if (NakamaManager.Instance != null && !string.IsNullOrWhiteSpace(NakamaManager.Instance.Username))
                return NakamaManager.Instance.Username.Trim();

            return string.Empty;
        }

        /// <summary>
        /// Called by AvatarPopupManager after a successful SelectAvatarRpc.
        /// Updates cache and fires events — no extra RPC needed.
        /// </summary>
        public void NotifyAvatarChanged(string newAvatarId, List<string> newOwnedAvatars = null)
        {
            CurrentAvatarId = newAvatarId;
            if (newOwnedAvatars != null && newOwnedAvatars.Count > 0)
            {
                OwnedAvatarIds = newOwnedAvatars;
                onOwnedAvatarsChanged?.Invoke(OwnedAvatarIds);
            }
            onAvatarChanged?.Invoke(newAvatarId);
        }

        public async Task SaveProgressionAsync(PlayerProgressionManager.ProgressionSavePayload payload)
        {
            if (payload == null)
                return;

            try
            {
                string json = JsonUtility.ToJson(payload);
                var rpc = await NakamaManager.Instance.SendRPC(SaveProgressionRpcId, json);
                if (rpc == null || string.IsNullOrEmpty(rpc.Payload))
                    return;

                // Update local progression cache after server save.
                PlayerProgression = new ProfileProgressionData
                {
                    currentXp = payload.currentXp,
                    currentLevel = payload.currentLevel,
                    missions = payload.missions
                };
                onProgressionLoaded?.Invoke(PlayerProgression);
            }
            catch (Exception e)
            {
                Debug.LogWarning("ProfileService.SaveProgressionAsync failed: " + e.Message);
            }
        }

        public Sprite GetCurrentSprite() => avatarLibrary != null ? avatarLibrary.GetSprite(CurrentAvatarId) : null;
        public Sprite GetSprite(string avatarId) => avatarLibrary != null ? avatarLibrary.GetSprite(avatarId) : null;
    }

    // ── DTOs ──────────────────────────────────────────────────────────────────────

    [Serializable]
    internal class AvatarPriceEntry
    {
        public string id;
        public int    price;
    }

    [Serializable]
    internal class ProfileServiceData
    {
        public string              displayName;
        public string              avatarId;
        public List<string>        ownedAvatars;
        public List<AvatarPriceEntry> avatarPrices;
        public ProfileProgressionData progression;
        public List<ProfileMissionDefinition> missionDefinitions;
    }

    [Serializable]
    public class ProfileMissionProgress
    {
        public string missionId;
        public int currentProgress;
        public bool isCompleted;
    }

    [Serializable]
    public class ProfileMissionDefinition
    {
        public string missionId;
        public string title;
        public string description;
        public string goalType;
        public int target;
        public int rewardXp;
        public bool isRepeatable;
    }

    [Serializable]
    public class ProfileProgressionData
    {
        public int currentXp;
        public int currentLevel;
        public List<ProfileMissionProgress> missions = new List<ProfileMissionProgress>();
    }
}
