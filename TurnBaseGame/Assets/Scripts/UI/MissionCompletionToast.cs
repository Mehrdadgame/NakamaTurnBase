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
    public sealed class MissionCompletionToast : MonoBehaviour
    {
        private static MissionCompletionToast _instance;

        [SerializeField] private TMP_FontAsset vazirFont;

        private CanvasGroup _canvasGroup;
        private GameObject _toastObject;
        private RectTransform _toastRect;
        private Transform _badgeTransform;
        private RTLTextMeshPro _titleText;
        private RTLTextMeshPro _missionText;
        private RTLTextMeshPro _rewardText;
        private Coroutine _queueRoutine;
        private MissionManager _missionManager;
        private readonly Queue<MissionState> _pendingToasts = new Queue<MissionState>();

        private const float HiddenY = 90f;
        private const float ShownY = -60f;

        public static void Ensure(MissionCompletionToast prefab = null)
        {
            if (_instance != null)
                return;

            _instance = prefab != null
                ? Instantiate(prefab)
                : new GameObject("MissionCompletionToast").AddComponent<MissionCompletionToast>();
            _instance.name = "MissionCompletionToast";
            DontDestroyOnLoad(_instance.gameObject);
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            if (vazirFont != null)
                ChatUiFactory.Font = vazirFont;

            BuildView();
            foreach (RTLTextMeshPro text in GetComponentsInChildren<RTLTextMeshPro>(true))
                text.PreserveNumbers = true;
        }

        private void Update()
        {
            if (_missionManager != null)
                return;

            _missionManager = MissionManager.Instance;
            if (_missionManager == null)
                return;

            _missionManager.OnMissionCompleted += ShowMissionCompleted;
        }

        private void OnDestroy()
        {
            if (_missionManager != null)
                _missionManager.OnMissionCompleted -= ShowMissionCompleted;

            if (_toastRect != null)
                _toastRect.DOKill();
            if (_canvasGroup != null)
                _canvasGroup.DOKill();

            if (_instance == this)
                _instance = null;
        }

        private void BuildView()
        {
            var canvasObject = new GameObject("MissionCompletionToastCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);

            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 300;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            // Compact, elegant banner that never blocks player clicks
            var toastImage = ChatUiFactory.Panel("MissionCompletionToast", canvasObject.transform,
                new Color(0.11f, 0.07f, 0.035f, 0.96f));
            toastImage.raycastTarget = false;
            _toastObject = toastImage.gameObject;
            _toastRect = toastImage.rectTransform;

            ChatUiFactory.Anchor(_toastRect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0f, HiddenY), new Vector2(580f, 88f));

            var outline = _toastObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.96f, 0.74f, 0.22f, 0.85f);
            outline.effectDistance = new Vector2(2f, -2f);

            _canvasGroup = _toastObject.AddComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
            _toastObject.SetActive(false);

            // Gold checkmark badge
            var badge = ChatUiFactory.Panel("CompleteBadge", _toastRect,
                new Color(0.85f, 0.55f, 0.12f, 1f));
            badge.raycastTarget = false;
            _badgeTransform = badge.transform;
            ChatUiFactory.Anchor(badge.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(1f, 0.5f), new Vector2(-16f, 0f), new Vector2(62f, 62f));
            var badgeText = ChatUiFactory.Text("BadgeText", badge.transform, "✓", 38,
                new Color(1f, 0.98f, 0.88f, 1f), TextAlignmentOptions.Center);
            badgeText.raycastTarget = false;
            ChatUiFactory.Stretch(badgeText.rectTransform);

            // Title: ماموریت انجام شد
            _titleText = ChatUiFactory.Text("Title", _toastRect, Localization.L("ماموریت انجام شد", "Mission complete"), 22,
                new Color(1f, 0.85f, 0.38f, 1f), TextAlignmentOptions.MidlineRight);
            _titleText.raycastTarget = false;
            ChatUiFactory.Anchor(_titleText.rectTransform, new Vector2(0f, 0.52f), new Vector2(1f, 0.94f),
                new Vector2(0.5f, 0.5f), new Vector2(-54f, 0f), new Vector2(-120f, 0f));

            // Mission description/title
            _missionText = ChatUiFactory.Text("Mission", _toastRect, "", 18,
                new Color(1f, 0.96f, 0.86f, 1f), TextAlignmentOptions.MidlineRight);
            _missionText.raycastTarget = false;
            ChatUiFactory.Anchor(_missionText.rectTransform, new Vector2(0f, 0.08f), new Vector2(0.72f, 0.52f),
                new Vector2(0.5f, 0.5f), new Vector2(-12f, 0f), Vector2.zero);

            // Reward text
            _rewardText = ChatUiFactory.Text("Reward", _toastRect, "", 17,
                new Color(0.46f, 0.96f, 0.56f, 1f), TextAlignmentOptions.MidlineLeft);
            _rewardText.raycastTarget = false;
            ChatUiFactory.Anchor(_rewardText.rectTransform, new Vector2(0f, 0.08f), new Vector2(0.35f, 0.52f),
                new Vector2(0f, 0.5f), new Vector2(20f, 0f), Vector2.zero);
        }

        private void ShowMissionCompleted(MissionState mission)
        {
            if (mission == null)
                return;

            _pendingToasts.Enqueue(mission);
            if (_queueRoutine == null)
                _queueRoutine = StartCoroutine(ProcessQueueRoutine());
        }

        private IEnumerator ProcessQueueRoutine()
        {
            while (_pendingToasts.Count > 0)
            {
                MissionState mission = _pendingToasts.Dequeue();
                _missionText.text = mission.Title;
                _rewardText.text = "+" + ToPersianDigits(mission.RewardXp) + " XP";

                _toastObject.SetActive(true);
                _toastRect.DOKill();
                _canvasGroup.DOKill();

                // Start from hidden position above screen
                _toastRect.anchoredPosition = new Vector2(0f, HiddenY);
                _canvasGroup.alpha = 0f;

                // Slide down into view with smooth spring
                _toastRect.DOAnchorPosY(ShownY, 0.35f).SetEase(Ease.OutBack).SetUpdate(true);
                _canvasGroup.DOFade(1f, 0.25f).SetUpdate(true);

                if (_badgeTransform != null)
                {
                    _badgeTransform.DOKill();
                    _badgeTransform.localScale = Vector3.one;
                    _badgeTransform.DOPunchScale(new Vector3(0.28f, 0.28f, 0f), 0.35f, 6, 0.5f).SetUpdate(true);
                }

                // Display duration
                yield return new WaitForSecondsRealtime(2.2f);

                // Slide up out of view
                _toastRect.DOAnchorPosY(HiddenY, 0.28f).SetEase(Ease.InBack).SetUpdate(true);
                yield return _canvasGroup.DOFade(0f, 0.24f).SetUpdate(true).WaitForCompletion();

                _toastObject.SetActive(false);
                yield return new WaitForSecondsRealtime(0.15f);
            }

            _queueRoutine = null;
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
