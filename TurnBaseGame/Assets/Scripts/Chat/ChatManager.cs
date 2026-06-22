using System.Collections;
using DG.Tweening;
using Nakama;
using NinjaBattle.Game;
using RTLTMPro;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nakama.Helpers
{
    /// <summary>
    /// In-game chat drawer. The UI is AUTHORED as real GameObjects in each battle scene by the
    /// "Tools ▸ Chat ▸ Author Chat Into Build Scenes" editor tool (so it is visible/editable in the
    /// Editor), and the serialized references below are wired to those objects. At runtime this
    /// component only wires behaviour (listeners, networking, gating) and fills the dynamic lists.
    /// If the references are NOT assigned (e.g. a scene without authored chat), it builds the same
    /// hierarchy at runtime as a fallback — so <see cref="ChatBootstrap"/> still works anywhere.
    ///
    /// Real opponent → text chat (presets + free text) + stickers. Bot → stickers only.
    /// Server can hide the whole feature via the global "chat_enabled" config (GetChatConfigRpc).
    /// </summary>
    public class ChatManager : MonoBehaviour
    {
        private const string GetChatConfigRpc = "GetChatConfigRpc";

        [Header("Quick-chat presets (Persian)")]
        [SerializeField]
        private string[] presets =
        {
            "سلام", "آفرین!", "خوش‌شانس بودی", "عجله کن",
            "خسته نباشی", "بازی خوبی بود", "نوبت منه", "نزدیک بود!",
        };

        [Header("Authored UI references (filled by Tools ▸ Chat ▸ Author)")]
        [SerializeField] private RectTransform root;
        [SerializeField] private CanvasGroup backdrop;
        [SerializeField] private Button backdropButton;
        [SerializeField] private RectTransform drawer;
        [SerializeField] private Button closeButton;
        [SerializeField] private GameObject tabBar;
        [SerializeField] private Button tabMessages;
        [SerializeField] private Button tabStickers;
        [SerializeField] private GameObject messagesPage;
        [SerializeField] private ScrollRect messagesScroll;
        [SerializeField] private RectTransform messagesContent;
        [SerializeField] private RectTransform presetRow;
        [SerializeField] private GameObject inputRow;
        [SerializeField] private TMP_InputField input;
        [SerializeField] private Button sendButton;
        [SerializeField] private GameObject stickersPage;
        [SerializeField] private RectTransform stickerContent;
        [SerializeField] private Button chatButton;
        [SerializeField] private GameObject unreadBadge;
        [SerializeField] private RTLTextMeshPro unreadText;
        [SerializeField] private float drawerHeight = 1180f;


        // ── Theme ───────────────────────────────────────────────────────────────────
        public static readonly Color ColPanel = new Color(0.09f, 0.11f, 0.17f, 0.98f);
        public static readonly Color ColHeader = new Color(0.13f, 0.16f, 0.24f, 1f);
        public static readonly Color ColAccent = new Color(0.20f, 0.70f, 0.58f, 1f);
        public static readonly Color ColAccent2 = new Color(0.95f, 0.40f, 0.38f, 1f);
        public static readonly Color ColMine = new Color(0.20f, 0.55f, 0.48f, 1f);
        public static readonly Color ColOpp = new Color(0.24f, 0.27f, 0.34f, 1f);
        public static readonly Color ColInputBg = new Color(0.16f, 0.19f, 0.27f, 1f);
        public static readonly Color ColTextLite = new Color(0.96f, 0.97f, 1f, 1f);
        public static readonly Color ColTextDim = new Color(0.70f, 0.74f, 0.82f, 1f);
        public static readonly Color ColBackdrop = new Color(0f, 0f, 0f, 0.55f);

        // ── Runtime state ─────────────────────────────────────────────────────────────
        private bool _wired;
        private bool _controlsWired;
        private bool _isOpen;
        private int _unread;
        private string _channelId;            // Nakama realtime chat channel for this match
        private Tween _badgePulse;
        private static Sprite[] _cachedStickers;
        private static GameObject _bubblePrefab; // Resources/Chat/ChatBubble (RTLTextMeshPro + Vazir)
        private bool _joining;            // true while a JoinChatAsync attempt is in flight
        private Canvas _rootCanvas;       // for converting the soft-keyboard pixel height → canvas UI units
        private float _kbOffset;          // current upward shift of the drawer so the input clears the keyboard

        // ── Lifecycle ────────────────────────────────────────────────────────────────

        private void Awake()
        {
            // The drawer is authored OPEN (visible/editable in the Editor). Snap it CLOSED the instant
            // the scene loads — before WaitThenInit's async waits — so it never flashes open during
            // matchmaking / scene load, and wire ALL controls immediately so close/send/tabs respond
            // right away (they must NOT wait for the opponent-resolve/config flow in WireRuntime).
            SnapClosed();
            WireControls();
        }

        private void SnapClosed()
        {
            _isOpen = false;
            if (drawer != null) drawer.anchoredPosition = new Vector2(0f, -drawerHeight - 40f);
            if (backdrop != null) { backdrop.alpha = 0f; backdrop.blocksRaycasts = false; }
        }

        // While the drawer is open, lift it by the soft-keyboard's height so the input field stays
        // visible above the keyboard (Unity does not do this for us on Android). When the keyboard is
        // hidden the offset returns to 0 and the drawer drops back to its normal open position.
        private void Update()
        {
            if (drawer == null) return;
            if (_rootCanvas == null) _rootCanvas = drawer.GetComponentInParent<Canvas>();

            float target = _isOpen ? GetKeyboardHeightUI() : 0f;
            if (Mathf.Approximately(_kbOffset, target)) return;

            _kbOffset = Mathf.MoveTowards(_kbOffset, target, Time.unscaledDeltaTime * 6000f);
            if (_isOpen)
            {
                drawer.DOKill(); // we own the Y while following the keyboard (open/close tweens are done by now)
                var p = drawer.anchoredPosition;
                drawer.anchoredPosition = new Vector2(p.x, _kbOffset);
            }
        }

        // Soft-keyboard height converted from screen pixels to this canvas's UI units (0 when hidden).
        private float GetKeyboardHeightUI()
        {
            if (!TouchScreenKeyboard.visible) return 0f;
            float px = TouchScreenKeyboard.area.height;
            if (px <= 1f) px = Screen.height * 0.40f; // some Android builds report 0 — fall back to an estimate
            float sf = (_rootCanvas != null && _rootCanvas.scaleFactor > 0f) ? _rootCanvas.scaleFactor : 1f;
            return px / sf;
        }

        /// <summary>
        /// Wires ALL chat controls (toggle, close, backdrop, send, tabs, input) and sets the input to
        /// RTL. Called from Awake for authored scenes (refs already present) and again from WireRuntime
        /// for the fallback build path. Runs at most once. These must be wired early — independent of the
        /// opponent-resolve/config flow — otherwise close/send/tabs do nothing until a match exists.
        /// Also disables the legacy "Button Sticker" PERSISTENT Inspector onClick (Animator.Play, which
        /// RemoveAllListeners can NOT remove) and hides the old sticker panel.
        /// </summary>
        private void WireControls()
        {
            if (_controlsWired || drawer == null) return;
            _controlsWired = true;

            // Toggle (repurpose "Button Sticker").
            if (chatButton == null)
            {
                var c = GameObject.Find("Canvas");
                var b = c != null ? c.transform.Find("Button Sticker") : null;
                if (b != null) chatButton = b.GetComponent<Button>();
            }
            if (chatButton != null)
            {
                var ce = chatButton.onClick;
                for (int i = ce.GetPersistentEventCount() - 1; i >= 0; i--)
                    ce.SetPersistentListenerState(i, UnityEngine.Events.UnityEventCallState.Off);
                ce.RemoveAllListeners();
                ce.AddListener(Toggle);
            }
            var canvas = GameObject.Find("Canvas");
            var sp = canvas != null ? canvas.transform.Find("StickerPanle") : null;
            if (sp != null) sp.gameObject.SetActive(false);

            // Drawer controls.
            if (closeButton != null)    closeButton.onClick.AddListener(() => SetOpen(false));
            if (backdropButton != null) backdropButton.onClick.AddListener(() => SetOpen(false));
            if (sendButton != null)     sendButton.onClick.AddListener(OnSendPressed);
            if (input != null)
                input.onSubmit.AddListener(_ => OnSendPressed());

            RemoveStickerSection(); // text-only chat — runs at Awake so it always applies
        }

        private void Start()
        {
            // CRITICAL: join the chat channel on its OWN loop. It only needs a live socket + matchId and
            // must NOT depend on the UI-wiring flow below — that flow used to wait on UiManager (a leftover
            // from the removed sticker feature) and `yield break` in battle scenes where UiManager is null,
            // which is why JoinChatChannel never ran (the on-screen diagnostic showed "join=-").
            StartCoroutine(JoinChatRetryLoop());
            StartCoroutine(WaitThenInit());
        }

        private IEnumerator WaitThenInit()
        {
            float timeout = 12f;
            while (timeout > 0f &&
                   (MultiplayerManager.Instance == null ||
                    NakamaManager.Instance == null || !NakamaManager.Instance.IsLoggedIn ||
                    GameObject.Find("Canvas") == null))
            {
                timeout -= Time.deltaTime;
                yield return null;
            }
            if (MultiplayerManager.Instance == null) yield break;

            // Wait until the opponent is resolvable before deciding bot-vs-real.
            float oppTimeout = 12f;
            while (oppTimeout > 0f && !OpponentResolved())
            {
                oppTimeout -= Time.deltaTime;
                yield return null;
            }

            CheckConfigThenWire();
        }

        private static bool OpponentResolved()
        {
            var mm = MultiplayerManager.Instance;
            var pm = PlayersManager.Instance;
            if (mm?.Self == null) return false;
            if (pm?.Players != null)
            {
                foreach (var p in pm.Players)
                {
                    if (p?.Presence == null) continue;
                    if (p.Presence.SessionId == mm.Self.SessionId) continue;
                    return true;
                }
            }
            return mm.Opp != null && !string.IsNullOrEmpty(mm.Opp.UserId);
        }

        private async void CheckConfigThenWire()
        {
            bool enabled = true;
            try
            {
                var rpc = await NakamaManager.Instance.SendRPC(GetChatConfigRpc, "{}");
                if (this == null) return; // scene may have unloaded during the await
                if (rpc != null && !string.IsNullOrEmpty(rpc.Payload))
                {
                    var cfg = rpc.Payload.Deserialize<ChatConfigResponse>();
                    if (cfg != null) enabled = cfg.enabled;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[Chat] config check failed, defaulting ON: " + e.Message);
            }

            if (this == null) return;
            if (MultiplayerManager.Instance == null) return;

            if (!enabled)
            {
                if (root != null) root.gameObject.SetActive(false);
                HideChatButton();
                return;
            }

            LogChatDiagnostics();
            WireRuntime();
        }

        /// <summary>
        /// One logcat line (filter "[Chat]") that makes the two real-world failure modes obvious:
        ///   • opponent is a BOT  → the Room channel has only YOU, so messages reach no one.
        ///   • the two devices show DIFFERENT "match=" → the players were never paired into the
        ///     same match (matchmaking race / each filled with a bot) → two separate Room channels.
        /// If both devices print the SAME match= AND isBot=False, real chat will work.
        /// </summary>
        private void LogChatDiagnostics()
        {
            var mm = MultiplayerManager.Instance;
            bool isBot = IsBotOpponent();
            Debug.Log("[Chat] DIAG match=" + (mm != null ? mm.CurrentMatchId : "?") +
                      " self=" + (mm != null && mm.Self != null ? mm.Self.UserId : "?") +
                      " opponent=" + OpponentUserIdDiag() + " isBot=" + isBot +
                      " channel=" + _channelId);
            if (isBot)
                Debug.LogWarning("[Chat] opponent is a BOT — this chat room has only you; nothing will reach " +
                                 "the other side. To test real chat, pair TWO real players into ONE match.");
        }

        // The opponent's Nakama userId (for diagnostics) — "bot_..." when playing a bot, "none" if unresolved.
        private string OpponentUserIdDiag()
        {
            var mm = MultiplayerManager.Instance;
            var pm = PlayersManager.Instance;
            if (mm?.Self != null && pm?.Players != null)
            {
                foreach (var p in pm.Players)
                {
                    if (p?.Presence == null) continue;
                    if (p.Presence.SessionId == mm.Self.SessionId) continue;
                    if (!string.IsNullOrEmpty(p.Presence.UserId)) return p.Presence.UserId;
                }
            }
            return mm?.Opp?.UserId ?? "none";
        }

        [System.Serializable] private class ChatConfigResponse { public bool enabled = true; }
        [System.Serializable] private class ChatContent { public string text; public string kind; }

        // ── Runtime wiring ─────────────────────────────────────────────────────────────

        private void WireRuntime()
        {
            if (_wired) return;

            CacheFont();

            // Fallback: scene has no authored UI → build it at runtime.
            if (drawer == null)
            {
                var canvas = GameObject.Find("Canvas");
                if (canvas == null) return;
                BuildHierarchy(canvas.transform);
            }
            if (drawer == null) return;
            _wired = true;

            WireControls(); // fallback path: refs now exist after BuildHierarchy → wire controls

            PopulatePresets();
            JoinChatChannel(); // safety: ensure the channel is joined (no-op if already joined)

            // Don't yank the drawer shut if the player opened it during the wait.
            if (!_isOpen) SetOpen(false, instant: true);
        }

        private void CacheFont()
        {
            ChatUiFactory.Font = null; // re-resolve each scene
            // Prefer the font already on our authored chat texts (Vazir, set at authoring time) so
            // runtime message bubbles shape Persian correctly via RTLTextMeshPro.
            if (unreadText != null && unreadText.font != null) { ChatUiFactory.Font = unreadText.font; return; }
            if (closeButton != null)
            {
                var lbl = closeButton.GetComponentInChildren<RTLTextMeshPro>();
                if (lbl != null && lbl.font != null) { ChatUiFactory.Font = lbl.font; return; }
            }
            var anyRtl = FindObjectOfType<RTLTextMeshPro>();
            if (anyRtl != null) { ChatUiFactory.Font = anyRtl.font; return; }
            var anyTmp = FindObjectOfType<TextMeshProUGUI>();
            if (anyTmp != null) ChatUiFactory.Font = anyTmp.font;
        }

        // ── Hierarchy builder (edit-mode + runtime safe) ─────────────────────────────────

        /// <summary>
        /// Builds the chat UI shell under <paramref name="canvasTf"/> and assigns the serialized
        /// references. Safe to call from the editor authoring tool OR at runtime (fallback).
        /// Does NOT touch runtime singletons or add listeners — those are wired in WireRuntime.
        /// Leaves the drawer in the OPEN position so it is visible while editing.
        /// </summary>
        public void BuildHierarchy(Transform canvasTf)
        {
            if (canvasTf == null) return;

            root = ChatUiFactory.Rect("ChatRoot", canvasTf);
            ChatUiFactory.Stretch(root);
            root.SetAsLastSibling();

            // Backdrop.
            var bd = ChatUiFactory.Panel("Backdrop", root, ColBackdrop, rounded: false);
            ChatUiFactory.Stretch(bd.rectTransform);
            backdrop = bd.gameObject.AddComponent<CanvasGroup>();
            backdropButton = bd.gameObject.AddComponent<Button>();
            backdropButton.transition = Selectable.Transition.None;

            // Drawer (bottom sheet).
            var panel = ChatUiFactory.Panel("ChatDrawer", root, ColPanel);
            drawer = panel.rectTransform;
            ChatUiFactory.Anchor(drawer, new Vector2(0f, 0f), new Vector2(1f, 0f),
                new Vector2(0.5f, 0f), new Vector2(0f, 0f), new Vector2(-24f, drawerHeight));

            BuildHeader();

            var content = ChatUiFactory.Rect("Content", drawer);
            content.anchorMin = new Vector2(0f, 0f);
            content.anchorMax = new Vector2(1f, 1f);
            content.offsetMin = new Vector2(18f, 18f);
            content.offsetMax = new Vector2(-18f, -118f);

            BuildTabs(content, out var pagesArea);
            messagesPage = BuildMessagesPage(pagesArea);
            stickersPage = BuildStickersPage(pagesArea);

            BuildToggleAndBadge(canvasTf);
        }

        private void BuildHeader()
        {
            var header = ChatUiFactory.Panel("Header", drawer, ColHeader);
            ChatUiFactory.Anchor(header.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0f, 0f), new Vector2(0f, 100f));

            var title = ChatUiFactory.Text("Title", header.rectTransform, "گفتگو", 40, ColTextLite,
                TextAlignmentOptions.Right);
            ChatUiFactory.Anchor(title.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f),
                new Vector2(1f, 0.5f), new Vector2(-28f, 0f), new Vector2(-40f, 0f));

            var close = ChatUiFactory.RoundButton("Close", header.rectTransform, ColAccent2, "X",
                Color.white, 34, 64f);
            ChatUiFactory.Anchor(close.GetComponent<RectTransform>(), new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(46f, 0f), new Vector2(64f, 64f));
            closeButton = close;
        }

        private void BuildTabs(RectTransform parent, out RectTransform pagesArea)
        {
            var bar = ChatUiFactory.Rect("TabBar", parent);
            ChatUiFactory.Anchor(bar, new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0f, 0f), new Vector2(0f, 84f));
            var hl = bar.gameObject.AddComponent<HorizontalLayoutGroup>();
            hl.spacing = 14; hl.childForceExpandWidth = true; hl.childForceExpandHeight = true;
            hl.childControlWidth = true; hl.childControlHeight = true;
            tabBar = bar.gameObject;

            tabMessages = ChatUiFactory.PillButton("TabMessages", bar, ColAccent, "پیام‌ها",
                Color.white, 30, out _);
            tabStickers = ChatUiFactory.PillButton("TabStickers", bar, ColOpp, "استیکر",
                ColTextLite, 30, out _);

            pagesArea = ChatUiFactory.Rect("Pages", parent);
            pagesArea.anchorMin = new Vector2(0f, 0f);
            pagesArea.anchorMax = new Vector2(1f, 1f);
            pagesArea.offsetMin = new Vector2(0f, 0f);
            pagesArea.offsetMax = new Vector2(0f, -98f);
        }

        private GameObject BuildMessagesPage(RectTransform parent)
        {
            var page = ChatUiFactory.Rect("MessagesPage", parent);
            ChatUiFactory.Stretch(page);

            var scrollGo = ChatUiFactory.Rect("Scroll", page);
            scrollGo.anchorMin = new Vector2(0f, 0f);
            scrollGo.anchorMax = new Vector2(1f, 1f);
            scrollGo.offsetMin = new Vector2(0f, 196f);
            scrollGo.offsetMax = new Vector2(0f, 0f);
            messagesScroll = scrollGo.gameObject.AddComponent<ScrollRect>();
            messagesScroll.horizontal = false;
            scrollGo.gameObject.AddComponent<RectMask2D>();

            var c = ChatUiFactory.Rect("MsgContent", scrollGo);
            c.anchorMin = new Vector2(0f, 1f); c.anchorMax = new Vector2(1f, 1f);
            c.pivot = new Vector2(0.5f, 1f); c.anchoredPosition = Vector2.zero;
            var vlg = c.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 14; vlg.padding = new RectOffset(8, 8, 8, 8);
            vlg.childControlWidth = true; vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;
            var csf = c.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            messagesContent = c;
            messagesScroll.content = c;
            messagesScroll.viewport = scrollGo;

            BuildPresetRow(page);
            inputRow = BuildInputRow(page);
            return page.gameObject;
        }

        private void BuildPresetRow(RectTransform page)
        {
            var presetScroll = ChatUiFactory.Rect("PresetScroll", page);
            presetScroll.anchorMin = new Vector2(0f, 0f);
            presetScroll.anchorMax = new Vector2(1f, 0f);
            presetScroll.pivot = new Vector2(0.5f, 0f);
            presetScroll.anchoredPosition = new Vector2(0f, 104f);
            presetScroll.sizeDelta = new Vector2(0f, 82f);
            var sr = presetScroll.gameObject.AddComponent<ScrollRect>();
            sr.vertical = false; sr.horizontal = true; sr.scrollSensitivity = 18f;
            presetScroll.gameObject.AddComponent<RectMask2D>();

            var row = ChatUiFactory.Rect("PresetRow", presetScroll);
            row.anchorMin = new Vector2(0f, 0f); row.anchorMax = new Vector2(0f, 1f);
            row.pivot = new Vector2(0f, 0.5f); row.anchoredPosition = Vector2.zero;
            var hl = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            hl.spacing = 12; hl.padding = new RectOffset(4, 4, 6, 6);
            hl.childControlWidth = true; hl.childControlHeight = true;
            hl.childForceExpandWidth = false; hl.childForceExpandHeight = true;
            var csf = row.gameObject.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            sr.content = row; sr.viewport = presetScroll;
            presetRow = row;
        }

        private GameObject BuildInputRow(RectTransform page)
        {
            var rowRt = ChatUiFactory.Rect("InputRow", page);
            rowRt.anchorMin = new Vector2(0f, 0f); rowRt.anchorMax = new Vector2(1f, 0f);
            rowRt.pivot = new Vector2(0.5f, 0f); rowRt.anchoredPosition = new Vector2(0f, 0f);
            rowRt.sizeDelta = new Vector2(0f, 92f);

            var send = ChatUiFactory.PillButton("Send", rowRt, ColAccent2, "ارسال", Color.white, 28, out _);
            ChatUiFactory.Anchor(send.GetComponent<RectTransform>(), new Vector2(0f, 0.5f),
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(44f, 0f), new Vector2(150f, 80f));
            sendButton = send;

            input = BuildInputField(rowRt, out var inputRt);
            ChatUiFactory.Anchor(inputRt, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(0.5f, 0.5f), new Vector2(72f, 0f), new Vector2(-184f, 84f));
            return rowRt.gameObject;
        }

        private TMP_InputField BuildInputField(Transform parent, out RectTransform rt)
        {
            var bg = ChatUiFactory.Panel("Input", parent, ColInputBg);
            rt = bg.rectTransform;
            var field = bg.gameObject.AddComponent<TMP_InputField>();

            var area = ChatUiFactory.Rect("TextArea", rt);
            ChatUiFactory.Stretch(area);
            area.offsetMin = new Vector2(22f, 8f);
            area.offsetMax = new Vector2(-22f, -8f);
            area.gameObject.AddComponent<RectMask2D>();

            var placeholder = ChatUiFactory.Text("Placeholder", area, "پیام بنویس...", 30, ColTextDim,
                TextAlignmentOptions.Right);
            ChatUiFactory.Stretch(placeholder.rectTransform);

            // RTLTextMeshPro editable component → Persian is shaped (joined + RTL) while typing.
            var textComp = ChatUiFactory.Text("Text", area, "", 30, ColTextLite, TextAlignmentOptions.Right);
            ChatUiFactory.Stretch(textComp.rectTransform);

            field.textViewport = area;
            field.textComponent = textComp;
            field.placeholder = placeholder;
            field.lineType = TMP_InputField.LineType.SingleLine;
            field.characterLimit = 200;
            return field;
        }

        private GameObject BuildStickersPage(RectTransform parent)
        {
            var page = ChatUiFactory.Rect("StickersPage", parent);
            ChatUiFactory.Stretch(page);

            var scrollGo = ChatUiFactory.Rect("StickerScroll", page);
            ChatUiFactory.Stretch(scrollGo);
            var sr = scrollGo.gameObject.AddComponent<ScrollRect>();
            sr.horizontal = false;
            scrollGo.gameObject.AddComponent<RectMask2D>();

            var c = ChatUiFactory.Rect("StickerContent", scrollGo);
            c.anchorMin = new Vector2(0f, 1f); c.anchorMax = new Vector2(1f, 1f);
            c.pivot = new Vector2(0.5f, 1f); c.anchoredPosition = Vector2.zero;
            var grid = c.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(150f, 150f);
            grid.spacing = new Vector2(16f, 16f);
            grid.padding = new RectOffset(10, 10, 10, 10);
            var csf = c.gameObject.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            sr.content = c; sr.viewport = scrollGo;
            stickerContent = c;
            return page.gameObject;
        }

        private void BuildToggleAndBadge(Transform canvasTf)
        {
            // Repurpose the existing "Button Sticker" if present; else create a floating button.
            var btnTf = canvasTf.Find("Button Sticker");
            if (btnTf != null)
            {
                chatButton = btnTf.GetComponent<Button>();
            }
            else
            {
                var fab = ChatUiFactory.RoundButton("ChatButton", root, ColAccent, "چت", Color.white, 36, 124f);
                ChatUiFactory.Anchor(fab.GetComponent<RectTransform>(), new Vector2(1f, 0f),
                    new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-102f, 75f), new Vector2(124f, 124f));
                chatButton = fab;
            }
            if (chatButton != null) BuildUnreadBadge(chatButton.transform);
        }

        private void BuildUnreadBadge(Transform parent)
        {
            var badge = ChatUiFactory.RoundButton("UnreadBadge", parent, ColAccent2, "", Color.white, 1, 44f);
            badge.transition = Selectable.Transition.None;
            badge.interactable = false;
            var img = badge.GetComponent<Image>();
            if (img != null) img.raycastTarget = false;
            ChatUiFactory.Anchor(badge.GetComponent<RectTransform>(), new Vector2(1f, 1f),
                new Vector2(1f, 1f), new Vector2(0.5f, 0.5f), new Vector2(-6f, -6f), new Vector2(44f, 44f));
            unreadText = ChatUiFactory.Text("Count", badge.transform, "", 26, Color.white, TextAlignmentOptions.Center);
            unreadText.raycastTarget = false;
            ChatUiFactory.Stretch(unreadText.rectTransform);
            unreadBadge = badge.gameObject;
            unreadBadge.SetActive(false);
        }

        // ── Dynamic population ─────────────────────────────────────────────────────────

        private void PopulatePresets()
        {
            if (presetRow == null) return;
            ClearChildren(presetRow);
            foreach (var p in presets)
            {
                string msg = p;
                var chip = ChatUiFactory.PillButton("Preset", presetRow, ColInputBg, p, ColTextLite, 28, out var lbl);
                var le = chip.gameObject.AddComponent<LayoutElement>();
                le.preferredWidth = Mathf.Clamp(lbl.GetPreferredValues(p).x + 48f, 120f, 460f);
                le.minWidth = le.preferredWidth;
                chip.onClick.AddListener(() => SendText(msg, "preset"));
            }
        }

        private void PopulateStickers()
        {
            if (stickerContent == null) return;
            var atlas = UiManager.instance != null ? UiManager.instance.AllAssets : null;
            if (atlas == null) return;

            if (_cachedStickers == null)
            {
                var arr = new Sprite[atlas.spriteCount];
                atlas.GetSprites(arr);
                _cachedStickers = arr;
            }
            ClearChildren(stickerContent);

            foreach (var s in _cachedStickers)
            {
                if (s == null) continue;
                string clean = s.name.Replace("(Clone)", "").Trim();
                if (clean.StartsWith("Sprite-Dice")) continue;

                var cell = ChatUiFactory.Rect("Sticker", stickerContent);
                var img = cell.gameObject.AddComponent<Image>();
                img.sprite = s; img.preserveAspect = true;
                var btn = cell.gameObject.AddComponent<Button>();
                btn.targetGraphic = img;
                string name = clean;
                btn.onClick.AddListener(() => SendSticker(name));
            }
        }

        private static void ClearChildren(Transform t)
        {
            for (int i = t.childCount - 1; i >= 0; i--)
                Destroy(t.GetChild(i).gameObject);
        }

        // ── Sending ──────────────────────────────────────────────────────────────────

        private void OnSendPressed()
        {
            if (input == null) return;
            SendText(input.text, "free");
            input.text = "";
            input.ActivateInputField();
        }

        private async void SendText(string raw, string kind)
        {
            if (string.IsNullOrWhiteSpace(raw)) return;

            string text = ChatProfanityFilter.Mask(raw.Trim());
            if (text.Length > 200) text = text.Substring(0, 200);

            AddBubble(text, isMine: true); // show locally immediately

            // Deliver via Nakama's realtime chat channel (joined per-match) — no custom server relay.
            try
            {
                var socket = NakamaManager.Instance?.Socket;
                if (socket != null && !string.IsNullOrEmpty(_channelId))
                {
                    var content = new ChatContent { text = text, kind = kind }.Serialize();
                    await socket.WriteChatMessageAsync(_channelId, content);
                    Debug.Log("[Chat] SENT to channel=" + _channelId + " text=" + text);
                }
                else
                {
                    Debug.LogWarning("[Chat] NOT sent — channelId empty (channel not joined). channelId=" + _channelId);
                }
            }
            catch (System.Exception e) { Debug.LogWarning("[Chat] send failed: " + e.Message); }
        }

        private void SendSticker(string stickerName)
        {
            if (MultiplayerManager.Instance?.Self == null || UiManager.instance == null) return;

            var sprite = UiManager.instance.AllAssets.GetSprite(stickerName);
            if (sprite != null && UiManager.instance.StickerShow != null)
            {
                UiManager.instance.StickerShow.GetComponent<Image>().sprite = sprite;
                UiManager.instance.StickerShow.Play("showSticker", 0, 0);
            }

            var sd = new StickerData
            {
                StickerName = stickerName,
                ID = MultiplayerManager.Instance.Self.UserId,
            };
            MultiplayerManager.Instance.Send(MultiplayerManager.Code.SendSticker, sd);
            SetOpen(false);
        }

        // Keeps trying to join the per-match chat channel until it succeeds (or we leave the scene).
        // Runs independently of the UI flow — it only needs a live socket + matchId. Handles the
        // real-world failure modes: matchId not ready yet (race) or JoinChatAsync hanging (timeout).
        private IEnumerator JoinChatRetryLoop()
        {
            float t = 90f;
            while (t > 0f && string.IsNullOrEmpty(_channelId))
            {
                var nm = NakamaManager.Instance;
                var mm = MultiplayerManager.Instance;
                if (nm != null && nm.IsLoggedIn && mm != null && !string.IsNullOrEmpty(mm.CurrentMatchId))
                    JoinChatChannel(); // guarded by _joining + _channelId, so this is safe to spam
                yield return new WaitForSeconds(1.5f);
                t -= 1.5f;
            }
        }

        private async void JoinChatChannel()
        {
            if (!string.IsNullOrEmpty(_channelId)) return; // already joined
            if (_joining) return;                          // an attempt is already in flight
            _joining = true;
            try
            {
                var socket = NakamaManager.Instance?.Socket;
                var matchId = MultiplayerManager.Instance?.CurrentMatchId;
                Debug.Log("[Chat] joining channel for match=" + matchId + " socket=" + (socket != null));
                if (socket == null || string.IsNullOrEmpty(matchId))
                {
                    Debug.LogWarning("[Chat] channel NOT joined — socket or matchId missing");
                    return;
                }

                socket.ReceivedChannelMessage -= OnChannelMessage;
                socket.ReceivedChannelMessage += OnChannelMessage;

                // JoinChatAsync can silently hang on a flaky socket — bound it so the retry loop can
                // try again instead of waiting forever.
                var joinTask = socket.JoinChatAsync(matchId, ChannelType.Room, true, false);
                var winner = await System.Threading.Tasks.Task.WhenAny(
                    joinTask, System.Threading.Tasks.Task.Delay(8000));
                if (this == null) return;
                if (winner != joinTask)
                {
                    Debug.LogWarning("[Chat] join TIMED OUT after 8s (will retry)");
                    return;
                }

                var channel = await joinTask; // completed within 8s → get result (or throw)
                if (this == null) return;
                _channelId = channel.Id;
                Debug.Log("[Chat] JOINED channel id=" + _channelId);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[Chat] join channel failed: " + e.Message);
            }
            finally { _joining = false; }
        }

        private void OnChannelMessage(IApiChannelMessage m)
        {
            Debug.Log("[Chat] RECEIVED channel msg ch=" + (m != null ? m.ChannelId : "null") + " mine=" + _channelId +
                      " from=" + (m != null ? m.SenderId : "?"));
            if (m == null || string.IsNullOrEmpty(_channelId) || m.ChannelId != _channelId) return;
            if (m.SenderId == MultiplayerManager.Instance?.Self?.UserId) return; // ignore own message

            ChatContent c = null;
            try { c = m.Content.Deserialize<ChatContent>(); } catch { }
            if (c == null || string.IsNullOrEmpty(c.text)) return;

            AddBubble(c.text, isMine: false);
            if (!_isOpen) SetUnread(_unread + 1);
        }

        private void AddBubble(string text, bool isMine)
        {
            if (messagesContent == null) return;

            var row = ChatUiFactory.Rect(isMine ? "RowMine" : "RowOpp", messagesContent);
            var rowLE = row.gameObject.AddComponent<LayoutElement>();

            if (_bubblePrefab == null) _bubblePrefab = Resources.Load<GameObject>("Chat/ChatBubble");

            Image bubble;
            RTLTextMeshPro tmp;
            if (_bubblePrefab != null)
            {
                // Use the authored prefab — its RTLTextMeshPro carries the Vazir font + Farsi shaping,
                // so Persian renders correctly regardless of runtime font lookups.
                var go = Instantiate(_bubblePrefab, row);
                bubble = go.GetComponent<Image>();
                tmp = go.GetComponentInChildren<RTLTextMeshPro>(true);
                bubble.color = isMine ? ColMine : ColOpp;
                tmp.alignment = isMine ? TextAlignmentOptions.Right : TextAlignmentOptions.Left;
                tmp.text = text;
            }
            else
            {
                // Fallback (prefab missing): build procedurally.
                bubble = ChatUiFactory.Panel("Bubble", row, isMine ? ColMine : ColOpp);
                tmp = ChatUiFactory.Text("Msg", bubble.rectTransform, text, 30, ColTextLite,
                    isMine ? TextAlignmentOptions.Right : TextAlignmentOptions.Left);
                var t = tmp.rectTransform;
                t.anchorMin = Vector2.zero; t.anchorMax = Vector2.one;
                t.offsetMin = new Vector2(24f, 14f); t.offsetMax = new Vector2(-24f, -14f);
            }

            var brt = bubble.rectTransform;
            const float maxW = 600f, padX = 48f, padY = 28f;
            float w = Mathf.Clamp(tmp.GetPreferredValues(text, maxW, 0f).x, 40f, maxW);
            float h = tmp.GetPreferredValues(text, w, 0f).y;
            float bw = w + padX, bh = h + padY;

            brt.anchorMin = brt.anchorMax = new Vector2(isMine ? 1f : 0f, 0.5f);
            brt.pivot = new Vector2(isMine ? 1f : 0f, 0.5f);
            brt.sizeDelta = new Vector2(bw, bh);
            brt.anchoredPosition = new Vector2(isMine ? -6f : 6f, 0f);

            rowLE.preferredHeight = bh; rowLE.minHeight = bh;

            StartCoroutine(ScrollToBottomNextFrame());
        }

        private IEnumerator ScrollToBottomNextFrame()
        {
            yield return null;
            Canvas.ForceUpdateCanvases();
            if (messagesContent != null) LayoutRebuilder.ForceRebuildLayoutImmediate(messagesContent);
            if (messagesScroll != null) messagesScroll.verticalNormalizedPosition = 0f;
            Canvas.ForceUpdateCanvases();
        }

        // ── Open / close / tabs / badge ──────────────────────────────────────────────

        // Stickers were removed — hide the sticker tab/page + the (now single-page) tab bar and let
        // the message list use the full drawer height. Works on the existing authored scenes.
        private void RemoveStickerSection()
        {
            if (tabStickers != null) tabStickers.gameObject.SetActive(false);
            if (tabBar != null) tabBar.SetActive(false);
            if (stickersPage != null) stickersPage.SetActive(false);
            if (messagesPage != null)
            {
                messagesPage.SetActive(true);
                var pages = messagesPage.transform.parent as RectTransform; // the "Pages" area below the (now hidden) tab bar
                if (pages != null) pages.offsetMax = new Vector2(pages.offsetMax.x, 0f);
            }
        }

        private void ShowPage(bool messages)
        {
            if (messagesPage != null) messagesPage.SetActive(messages);
            if (stickersPage != null) stickersPage.SetActive(!messages);
        }

        private void SetTabColors(bool messages)
        {
            if (tabMessages != null) tabMessages.GetComponent<Image>().color = messages ? ColAccent : ColOpp;
            if (tabStickers != null) tabStickers.GetComponent<Image>().color = messages ? ColOpp : ColAccent;
        }

        private void Toggle() => SetOpen(!_isOpen);

        private void SetOpen(bool open, bool instant = false)
        {
            _isOpen = open;
            if (open) SetUnread(0);
            if (drawer == null || backdrop == null) return;

            drawer.DOKill();
            backdrop.DOKill();

            float closedY = -drawerHeight - 40f;
            if (instant)
            {
                drawer.anchoredPosition = new Vector2(0f, open ? 0f : closedY);
                backdrop.alpha = open ? 1f : 0f;
                backdrop.blocksRaycasts = open;
                return;
            }
            backdrop.blocksRaycasts = open;
            backdrop.DOFade(open ? 1f : 0f, 0.22f);
            drawer.DOAnchorPosY(open ? 0f : closedY, 0.32f).SetEase(open ? Ease.OutCubic : Ease.InCubic);
        }

        private void SetUnread(int n)
        {
            _unread = Mathf.Max(0, n);
            if (unreadBadge == null) return;
            bool show = _unread > 0;
            unreadBadge.SetActive(show);
            if (unreadText != null) unreadText.text = _unread > 9 ? "9+" : _unread.ToString();
            UpdateBadgePulse(show);
        }

        /// <summary>
        /// Slow "you have a message" pulse: the red dot next to the chat button gently scales
        /// up/down while there are unread messages, and stops (resets) once the chat is opened.
        /// </summary>
        private void UpdateBadgePulse(bool on)
        {
            if (unreadBadge == null) return;
            var t = unreadBadge.transform;
            if (on)
            {
                if (_badgePulse == null || !_badgePulse.IsActive())
                {
                    t.localScale = Vector3.one;
                    _badgePulse = t.DOScale(1.4f, 0.65f)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetEase(Ease.InOutSine);
                }
            }
            else
            {
                if (_badgePulse != null) { _badgePulse.Kill(); _badgePulse = null; }
                t.localScale = Vector3.one;
            }
        }

        private void HideChatButton()
        {
            if (chatButton != null) { chatButton.gameObject.SetActive(false); return; }
            var canvas = GameObject.Find("Canvas");
            var btnTf = canvas != null ? canvas.transform.Find("Button Sticker") : null;
            if (btnTf != null) btnTf.gameObject.SetActive(false);
        }

        private bool IsBotOpponent()
        {
            var mm = MultiplayerManager.Instance;
            var pm = PlayersManager.Instance;
            if (mm?.Self != null && pm?.Players != null)
            {
                foreach (var p in pm.Players)
                {
                    if (p?.Presence == null) continue;
                    if (p.Presence.SessionId == mm.Self.SessionId) continue;
                    return !string.IsNullOrEmpty(p.Presence.UserId) &&
                           p.Presence.UserId.StartsWith("bot_", System.StringComparison.OrdinalIgnoreCase);
                }
            }
            if (mm?.Opp != null && !string.IsNullOrEmpty(mm.Opp.UserId))
                return mm.Opp.UserId.StartsWith("bot_", System.StringComparison.OrdinalIgnoreCase);
            return false;
        }

        private void OnDestroy()
        {
            var socket = NakamaManager.Instance?.Socket;
            if (socket != null)
            {
                socket.ReceivedChannelMessage -= OnChannelMessage;
                if (!string.IsNullOrEmpty(_channelId))
                {
                    try { _ = socket.LeaveChatAsync(_channelId); } catch { }
                }
            }
            if (_badgePulse != null) { _badgePulse.Kill(); _badgePulse = null; }
            if (drawer != null) drawer.DOKill();
            if (backdrop != null) backdrop.DOKill();
        }
    }
}
