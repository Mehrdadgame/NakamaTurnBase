using System.Collections;
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
        private RTLTextMeshPro _titleText;
        private RTLTextMeshPro _missionText;
        private RTLTextMeshPro _rewardText;
        private Coroutine _displayRoutine;
        private MissionManager _missionManager;

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

            if (_instance == this)
                _instance = null;
        }

        private void BuildView()
        {
            var canvasObject = new GameObject("MissionCompletionToastCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);

            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            var toastImage = ChatUiFactory.Panel("MissionCompletionToast", canvasObject.transform,
                new Color(0.015f, 0.11f, 0.075f, 0.98f));
            _toastObject = toastImage.gameObject;
            var toastRect = toastImage.rectTransform;
            ChatUiFactory.Anchor(toastRect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0.5f, 1f), new Vector2(0f, -100f), new Vector2(720f, 150f));

            var outline = _toastObject.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 0.76f, 0.24f, 0.75f);
            outline.effectDistance = new Vector2(2f, -2f);

            _canvasGroup = _toastObject.AddComponent<CanvasGroup>();
            _canvasGroup.alpha = 0f;
            _toastObject.SetActive(false);

            var badge = ChatUiFactory.Panel("CompleteBadge", toastRect,
                new Color(0.78f, 0.48f, 0.08f, 1f));
            ChatUiFactory.Anchor(badge.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f),
                new Vector2(1f, 0.5f), new Vector2(-26f, 0f), new Vector2(112f, 112f));
            var badgeText = ChatUiFactory.Text("BadgeText", badge.transform, "✓", 52,
                new Color(1f, 0.97f, 0.82f, 1f), TextAlignmentOptions.Center);
            ChatUiFactory.Stretch(badgeText.rectTransform);

            _titleText = ChatUiFactory.Text("Title", toastRect, "ماموریت انجام شد", 27,
                new Color(1f, 0.84f, 0.34f, 1f), TextAlignmentOptions.MidlineRight);
            ChatUiFactory.Anchor(_titleText.rectTransform, new Vector2(0f, 0.58f), new Vector2(1f, 0.95f),
                new Vector2(0.5f, 0.5f), new Vector2(-88f, 0f), new Vector2(-150f, 0f));

            _missionText = ChatUiFactory.Text("Mission", toastRect, "", 21,
                new Color(1f, 0.96f, 0.82f, 1f), TextAlignmentOptions.MidlineRight);
            ChatUiFactory.Anchor(_missionText.rectTransform, new Vector2(0f, 0.18f), new Vector2(0.78f, 0.62f),
                new Vector2(0.5f, 0.5f), new Vector2(-20f, 0f), Vector2.zero);

            _rewardText = ChatUiFactory.Text("Reward", toastRect, "", 18,
                new Color(0.62f, 1f, 0.72f, 1f), TextAlignmentOptions.MidlineRight);
            ChatUiFactory.Anchor(_rewardText.rectTransform, new Vector2(0f, 0.02f), new Vector2(0.78f, 0.32f),
                new Vector2(0.5f, 0.5f), new Vector2(-20f, 0f), Vector2.zero);
        }

        private void ShowMissionCompleted(MissionState mission)
        {
            if (mission == null)
                return;

            _missionText.text = mission.Title;
            _rewardText.text = "+" + ToPersianDigits(mission.RewardXp) + " XP";
            _toastObject.SetActive(true);

            if (_displayRoutine != null)
                StopCoroutine(_displayRoutine);
            _displayRoutine = StartCoroutine(DisplayRoutine());
        }

        private IEnumerator DisplayRoutine()
        {
            yield return FadeTo(1f, 0.2f);
            yield return new WaitForSecondsRealtime(2.8f);
            yield return FadeTo(0f, 0.3f);
            _toastObject.SetActive(false);
            _displayRoutine = null;
        }

        private IEnumerator FadeTo(float target, float duration)
        {
            float start = _canvasGroup.alpha;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                _canvasGroup.alpha = Mathf.Lerp(start, target, elapsed / duration);
                yield return null;
            }
            _canvasGroup.alpha = target;
        }

        private static string ToPersianDigits(int value)
        {
            return value.ToString().Replace('0', '۰').Replace('1', '۱').Replace('2', '۲')
                .Replace('3', '۳').Replace('4', '۴').Replace('5', '۵').Replace('6', '۶')
                .Replace('7', '۷').Replace('8', '۸').Replace('9', '۹');
        }
    }
}
