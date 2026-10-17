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

        private AudioClip _click, _diceRoll, _diceLand, _diceArrive, _tilePlace, _tileInvalid,
            _matchDouble, _matchTriple, _matchQuad, _eliminate, _mineExplode,
            _timerTick, _timerTimeout, _scorePoint, _yourTurn, _opponentTurn,
            _emotePop, _popupOpen, _popupClose, _coin, _chestOpen,
            _victory, _defeat, _draw, _music, _irisOpen, _irisClose;

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

            _click        = Resources.Load<AudioClip>(ClipRoot + "ui_click");
            _diceRoll     = Resources.Load<AudioClip>(ClipRoot + "dice_roll");
            _diceLand     = Resources.Load<AudioClip>(ClipRoot + "dice_land");
            _diceArrive   = Resources.Load<AudioClip>(ClipRoot + "dice_arrive");
            _tilePlace    = Resources.Load<AudioClip>(ClipRoot + "tile_place");
            _tileInvalid  = Resources.Load<AudioClip>(ClipRoot + "tile_invalid");
            _matchDouble  = Resources.Load<AudioClip>(ClipRoot + "match_double");
            _matchTriple  = Resources.Load<AudioClip>(ClipRoot + "match_triple");
            _matchQuad    = Resources.Load<AudioClip>(ClipRoot + "match_quad");
            _eliminate    = Resources.Load<AudioClip>(ClipRoot + "eliminate");
            _mineExplode  = Resources.Load<AudioClip>(ClipRoot + "mine_explode");
            _timerTick    = Resources.Load<AudioClip>(ClipRoot + "timer_tick");
            _timerTimeout = Resources.Load<AudioClip>(ClipRoot + "timer_timeout");
            _scorePoint   = Resources.Load<AudioClip>(ClipRoot + "score_point");
            _yourTurn     = Resources.Load<AudioClip>(ClipRoot + "your_turn");
            _opponentTurn = Resources.Load<AudioClip>(ClipRoot + "opponent_turn");
            _emotePop     = Resources.Load<AudioClip>(ClipRoot + "emote_pop");
            _popupOpen    = Resources.Load<AudioClip>(ClipRoot + "popup_open");
            _popupClose   = Resources.Load<AudioClip>(ClipRoot + "popup_close");
            _coin         = Resources.Load<AudioClip>(ClipRoot + "coin");
            _chestOpen    = Resources.Load<AudioClip>(ClipRoot + "chest_open");
            _victory      = Resources.Load<AudioClip>(ClipRoot + "victory");
            _defeat       = Resources.Load<AudioClip>(ClipRoot + "defeat");
            _draw         = Resources.Load<AudioClip>(ClipRoot + "draw");
            _music        = Resources.Load<AudioClip>(ClipRoot + "music_casual_loop");
            _irisOpen     = Resources.Load<AudioClip>(ClipRoot + "iris_open");
            _irisClose    = Resources.Load<AudioClip>(ClipRoot + "iris_close");

            SceneManager.sceneLoaded += OnSceneLoaded;
            ChestManager.OnChestClaimed += OnChestClaimed;
            ChestManager.OnPanelOpened += OnChestPanelOpened;
            ChestManager.OnPanelClosed += OnChestPanelClosed;

            // sceneLoaded never fires for the scene that was already open when we
            // bootstrapped (first boot, or playing a scene directly in the editor).
            OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            ChestManager.OnChestClaimed -= OnChestClaimed;
            ChestManager.OnPanelOpened -= OnChestPanelOpened;
            ChestManager.OnPanelClosed -= OnChestPanelClosed;
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
        private void PlayClickInternal() => PlayRandomPitch(_click, 0.95f, 1.05f);

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
            EnsureAudio()?.StopLoopSound();
        }

        private void OnRollStarted()
        {
            // Start continuous dice rolling rattle loop
            EnsureAudio()?.StartLoopSound(_diceRoll, 0.65f, 1.0f);
        }

        private void OnRollUp(bool rolled)
        {
            EnsureAudio()?.StopLoopSound();
            if (rolled) PlayRandomPitch(_diceLand, 0.95f, 1.05f);
        }

        private void OnTilePlaced(DataPlayer data)
        {
            PlayRandomPitch(_tilePlace, 0.96f, 1.04f);
        }

        private void OnEliminated(int line, int row) => Play(_eliminate);

        private void OnTurnChanged(bool mine)
        {
            if (mine && !_wasMyTurn) Play(_yourTurn);
            else if (!mine && _wasMyTurn) Play(_opponentTurn);
            _wasMyTurn = mine;
        }

        private void OnChestPanelOpened() => Play(_popupOpen);
        private void OnChestPanelClosed() => Play(_popupClose);

        private void OnChestClaimed(int remainingSeconds)
        {
            Play(_chestOpen);
            // The coin payout lands right after the lid opens.
            Invoke(nameof(PlayCoinDelayed), 0.35f);
        }

        private void PlayCoinDelayed() => Play(_coin);

        // ── Public API (for code that wants explicit sounds) ──────────────────

        /// <summary>Called by CalculterRowScore when a double/triple/quad lights up.</summary>
        public static void PlayMatch(int matchCount)
        {
            if (Instance == null) return;
            if (matchCount >= 4)
                Instance.Play(Instance._matchQuad != null ? Instance._matchQuad : Instance._matchTriple);
            else if (matchCount == 3)
                Instance.Play(Instance._matchTriple);
            else
                Instance.Play(Instance._matchDouble);
        }

        public static void PlayDiceArrive() => Instance?.Play(Instance._diceArrive);
        public static void PlayDiceRollStart() => Instance?.OnRollStarted();
        public static void PlayDiceLand() => Instance?.OnRollUp(true);
        public static void PlayTilePlace() => Instance?.PlayRandomPitch(Instance._tilePlace, 0.96f, 1.04f);
        public static void PlayTileInvalid() => Instance?.Play(Instance._tileInvalid);
        public static void PlayEliminate() => Instance?.Play(Instance._eliminate);
        public static void PlayMineExplode() => Instance?.Play(Instance._mineExplode);
        public static void PlayScore() => Instance?.PlayRandomPitch(Instance._scorePoint, 0.96f, 1.06f, 0.65f);
        public static void PlayYourTurn() => Instance?.Play(Instance._yourTurn);
        public static void PlayOpponentTurn() => Instance?.Play(Instance._opponentTurn);
        public static void PlaySticker() => Instance?.Play(Instance._emotePop);
        public static void PlayPopupOpen() => Instance?.Play(Instance._popupOpen);
        public static void PlayPopupClose() => Instance?.Play(Instance._popupClose);
        public static void PlayVictory() => Instance?.Play(Instance._victory);
        public static void PlayDefeat() => Instance?.Play(Instance._defeat);
        public static void PlayDraw() => Instance?.Play(Instance._draw);

        public static void PlayTimerTick(int second)
        {
            if (Instance == null || Instance._timerTick == null) return;
            // Pitch increases as timer runs out (5 -> 1.0, 4 -> 1.08, 3 -> 1.16, 2 -> 1.25, 1 -> 1.35)
            float pitch = 1.0f + (5 - Mathf.Clamp(second, 1, 5)) * 0.08f;
            var audio = Instance.EnsureAudio();
            if (audio != null) audio.PlaySound(Instance._timerTick, 0.8f, pitch);
        }

        public static void PlayTimerTimeout() => Instance?.Play(Instance._timerTimeout);

        public static void PlayCoin() => Instance?.Play(Instance._coin);
        public static void PlayClick() => Instance?.PlayRandomPitch(Instance._click, 0.95f, 1.05f);

        /// <summary>Called by CartoonIrisTransitionOverlay when the screen closes to black.</summary>
        public static void PlayIrisClose() => Instance?.Play(Instance._irisClose);

        /// <summary>Called by CartoonIrisTransitionOverlay when the screen reveals from black.</summary>
        public static void PlayIrisOpen() => Instance?.Play(Instance._irisOpen);

        /// <summary>Fallback click clip for AudioManager.PlayClickSound.</summary>
        public static AudioClip ClickClip => Instance != null ? Instance._click : null;

        private void Play(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            if (clip == null) return;
            var audio = EnsureAudio();
            if (audio != null) audio.PlaySound(clip, volume, pitch);
        }

        private void PlayRandomPitch(AudioClip clip, float minPitch = 0.95f, float maxPitch = 1.05f, float volume = 1f)
        {
            if (clip == null) return;
            var audio = EnsureAudio();
            if (audio != null) audio.PlaySoundRandomPitch(clip, minPitch, maxPitch, volume);
        }
    }
}
