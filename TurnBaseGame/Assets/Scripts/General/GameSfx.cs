using Nakama.Helpers;
using NinjaBattle.Game;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NinjaBattle.General
{
    /// <summary>
    /// Central game audio: casual background music per scene + one SFX per gameplay action.
    ///
    /// Self-bootstraps (no scene wiring) and hooks the events the game already raises:
    ///   roll start/land, tile placed, double/triple match, elimination, turn change,
    ///   chest claim, match end. Clips load from Resources/Audio/Casual — replace any
    ///   file there with a better one (same name) and the game picks it up, no code edits.
    /// </summary>
    public class GameSfx : MonoBehaviour
    {
        private const string ClipRoot = "Audio/Casual/";

        public static GameSfx Instance { get; private set; }

        private AudioClip _click, _diceRoll, _diceLand, _tilePlace, _matchDouble,
            _matchTriple, _eliminate, _yourTurn, _coin, _chestOpen, _music,
            _irisOpen, _irisClose;

        private DiceRoller _hookedRoller;
        private PlayersManager _hookedPlayers;
        private bool _wasMyTurn;

        // Every Button gets a click sound automatically — including ones created at
        // runtime (popups, chat, avatar grid). Tracked so each is wired exactly once.
        private readonly System.Collections.Generic.HashSet<UnityEngine.UI.Button> _wiredButtons = new();
        private float _nextButtonSweep;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (Instance != null) return;
            var go = new GameObject("GameSfx");
            DontDestroyOnLoad(go);
            go.AddComponent<GameSfx>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            _click       = Resources.Load<AudioClip>(ClipRoot + "ui_click");
            _diceRoll    = Resources.Load<AudioClip>(ClipRoot + "dice_roll");
            _diceLand    = Resources.Load<AudioClip>(ClipRoot + "dice_land");
            _tilePlace   = Resources.Load<AudioClip>(ClipRoot + "tile_place");
            _matchDouble = Resources.Load<AudioClip>(ClipRoot + "match_double");
            _matchTriple = Resources.Load<AudioClip>(ClipRoot + "match_triple");
            _eliminate   = Resources.Load<AudioClip>(ClipRoot + "eliminate");
            _yourTurn    = Resources.Load<AudioClip>(ClipRoot + "your_turn");
            _coin        = Resources.Load<AudioClip>(ClipRoot + "coin");
            _chestOpen   = Resources.Load<AudioClip>(ClipRoot + "chest_open");
            _music       = Resources.Load<AudioClip>(ClipRoot + "music_casual_loop");
            _irisOpen    = Resources.Load<AudioClip>(ClipRoot + "iris_open");
            _irisClose   = Resources.Load<AudioClip>(ClipRoot + "iris_close");

            SceneManager.sceneLoaded += OnSceneLoaded;
            ChestManager.OnChestClaimed += OnChestClaimed;

            // sceneLoaded never fires for the scene that was already open when we
            // bootstrapped (first boot, or playing a scene directly in the editor).
            OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            ChestManager.OnChestClaimed -= OnChestClaimed;
            UnhookBattle();
            if (Instance == this) Instance = null;
        }

        // ── Scene / music ─────────────────────────────────────────────────────

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            UnhookBattle();
            TryHookBattle();

            // One cheerful loop everywhere except the boot scenes.
            if (scene.buildIndex >= (int)Scenes.Home)
                PlayMusicIfChanged();
        }

        private void PlayMusicIfChanged()
        {
            var audio = EnsureAudio();
            if (audio == null || _music == null) return;
            if (audio.IsMusicPlaying(_music)) return;   // keep it seamless across scenes
            audio.PlayMusic(_music);
        }

        /// <summary>
        /// The boot flow creates AudioManager in the Initializer scene, but when a battle
        /// scene is played directly in the editor it never exists — create one locally.
        /// </summary>
        private AudioManager EnsureAudio()
        {
            if (AudioManager.Instance == null)
                gameObject.AddComponent<AudioManager>();
            return AudioManager.Instance;
        }

        // ── Battle hooks ──────────────────────────────────────────────────────

        private void Update()
        {
            // Managers appear a few frames into the battle scene — hook lazily.
            if (_hookedRoller == null || _hookedPlayers == null)
                TryHookBattle();

            // Sweep for new buttons twice a second (covers runtime-created UI).
            if (Time.unscaledTime >= _nextButtonSweep)
            {
                _nextButtonSweep = Time.unscaledTime + 0.5f;
                WireAllButtons();
            }
        }

        private void WireAllButtons()
        {
            var buttons = FindObjectsByType<UnityEngine.UI.Button>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            _wiredButtons.RemoveWhere(b => b == null);   // destroyed buttons
            foreach (var button in buttons)
            {
                if (_wiredButtons.Contains(button)) continue;
                _wiredButtons.Add(button);
                button.onClick.AddListener(PlayClickInternal);
            }
        }

        // AudioManager's per-clip cooldown dedupes this against UIButtonJuice's own click.
        private void PlayClickInternal() => Play(_click);

        private void TryHookBattle()
        {
            if (_hookedRoller == null && GameManager.Instance != null && GameManager.Instance.diceRoller != null)
            {
                _hookedRoller = GameManager.Instance.diceRoller;
                _hookedRoller.RollStarted += OnRollStarted;
                _hookedRoller.RollUp += OnRollUp;
            }

            if (_hookedPlayers == null && PlayersManager.Instance != null)
            {
                _hookedPlayers = PlayersManager.Instance;
                _hookedPlayers.onSetDataInTurn += OnTilePlaced;
                _hookedPlayers.onSetDataInRowMe += OnEliminated;
                _hookedPlayers.onSetDataInRowOpp += OnEliminated;
                _hookedPlayers.IsTurn += OnTurnChanged;
            }
        }

        private void UnhookBattle()
        {
            if (_hookedRoller != null)
            {
                _hookedRoller.RollStarted -= OnRollStarted;
                _hookedRoller.RollUp -= OnRollUp;
                _hookedRoller = null;
            }

            if (_hookedPlayers != null)
            {
                _hookedPlayers.onSetDataInTurn -= OnTilePlaced;
                _hookedPlayers.onSetDataInRowMe -= OnEliminated;
                _hookedPlayers.onSetDataInRowOpp -= OnEliminated;
                _hookedPlayers.IsTurn -= OnTurnChanged;
                _hookedPlayers = null;
            }

            _wasMyTurn = false;
        }

        private void OnRollStarted() => Play(_diceRoll);
        private void OnRollUp(bool rolled) { if (rolled) Play(_diceLand); }
        private void OnTilePlaced(DataPlayer data) => Play(_tilePlace);
        private void OnEliminated(int line, int row) => Play(_eliminate);

        private void OnTurnChanged(bool mine)
        {
            if (mine && !_wasMyTurn) Play(_yourTurn);
            _wasMyTurn = mine;
        }

        private void OnChestClaimed(int remainingSeconds)
        {
            Play(_chestOpen);
            // The coin payout lands right after the lid opens.
            Invoke(nameof(PlayCoinDelayed), 0.35f);
        }

        private void PlayCoinDelayed() => Play(_coin);

        // ── Public API (for code that wants explicit sounds) ──────────────────

        /// <summary>Called by CalculterRowScore when a double/triple lights up.</summary>
        public static void PlayMatch(int matchCount)
        {
            if (Instance == null) return;
            Instance.Play(matchCount >= 3 ? Instance._matchTriple : Instance._matchDouble);
        }

        public static void PlayCoin() => Instance?.Play(Instance._coin);
        public static void PlayClick() => Instance?.Play(Instance._click);

        /// <summary>Called by CartoonIrisTransitionOverlay when the screen closes to black.</summary>
        public static void PlayIrisClose() => Instance?.Play(Instance._irisClose);

        /// <summary>Called by CartoonIrisTransitionOverlay when the screen reveals from black.</summary>
        public static void PlayIrisOpen() => Instance?.Play(Instance._irisOpen);

        /// <summary>Fallback click clip for AudioManager.PlayClickSound.</summary>
        public static AudioClip ClickClip => Instance != null ? Instance._click : null;

        private void Play(AudioClip clip)
        {
            if (clip == null) return;
            var audio = EnsureAudio();
            if (audio != null) audio.PlaySound(clip);
        }
    }
}
