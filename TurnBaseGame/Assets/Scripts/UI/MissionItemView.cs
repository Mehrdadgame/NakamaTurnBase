using NinjaBattle.Game;
using RTLTMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NinjaBattle.UI
{
    public class MissionItemView : MonoBehaviour
    {
        [SerializeField] private RTLTextMeshPro titleText;
        [SerializeField] private RTLTextMeshPro descriptionText;
        [SerializeField] private RTLTextMeshPro progressText;
        [SerializeField] private RTLTextMeshPro rewardText;
        [SerializeField] private RTLTextMeshPro completedText;
        [SerializeField] private Image progressFill;
        [SerializeField] private Image cardBackground;

        private Outline _cardOutline;

        private static readonly Color ActiveCardColor = new Color(0.12f, 0.08f, 0.04f, 0.96f);
        private static readonly Color CompletedCardColor = new Color(0.06f, 0.20f, 0.12f, 0.98f);
        private static readonly Color ActiveBorderColor = new Color(0.75f, 0.52f, 0.18f, 0.80f);
        private static readonly Color CompletedBorderColor = new Color(0.38f, 0.88f, 0.48f, 0.92f);
        private static readonly Color ActiveFillColor = new Color(0.98f, 0.74f, 0.20f, 1f);
        private static readonly Color CompletedFillColor = new Color(0.35f, 0.95f, 0.50f, 1f);

        public string MissionId { get; private set; }
        public bool IsCompleted { get; private set; }

        private void Awake()
        {
            foreach (RTLTextMeshPro text in GetComponentsInChildren<RTLTextMeshPro>(true))
                text.PreserveNumbers = true;

            if (cardBackground != null)
            {
                _cardOutline = cardBackground.GetComponent<Outline>();
                if (_cardOutline == null)
                    _cardOutline = cardBackground.gameObject.AddComponent<Outline>();
                _cardOutline.effectDistance = new Vector2(2f, -2f);
            }

            ConfigureProgressFill();
        }

        public void Bind(MissionState mission)
        {
            if (mission == null)
                return;

            ConfigureProgressFill();
            MissionId = mission.MissionId;
            IsCompleted = mission.IsCompleted;

            if (titleText != null)
            {
                titleText.text = mission.Title;
                titleText.color = mission.IsCompleted
                    ? new Color(0.90f, 1f, 0.92f, 1f)
                    : new Color(1f, 0.88f, 0.42f, 1f);
            }

            if (descriptionText != null)
            {
                descriptionText.text = mission.Description;
                descriptionText.color = new Color(0.96f, 0.92f, 0.82f, 0.90f);
            }

            if (progressText != null)
            {
                progressText.text = mission.IsCompleted
                    ? "تکمیل شد"
                    : $"{ToPersianDigits(mission.CurrentProgress)} / {ToPersianDigits(mission.Target)}";
                progressText.color = mission.IsCompleted
                    ? new Color(0.45f, 1f, 0.58f, 1f)
                    : new Color(1f, 0.95f, 0.80f, 1f);
            }

            if (rewardText != null)
            {
                rewardText.text = $"+{ToPersianDigits(mission.RewardXp)} XP";
                rewardText.color = mission.IsCompleted
                    ? new Color(0.55f, 1f, 0.65f, 1f)
                    : new Color(1f, 0.84f, 0.32f, 1f);
            }

            if (completedText != null)
                completedText.gameObject.SetActive(mission.IsCompleted);

            if (progressFill != null)
            {
                progressFill.fillAmount = mission.ProgressRatio;
                progressFill.color = mission.IsCompleted ? CompletedFillColor : ActiveFillColor;
            }

            if (cardBackground != null)
            {
                cardBackground.color = mission.IsCompleted ? CompletedCardColor : ActiveCardColor;
                if (_cardOutline != null)
                    _cardOutline.effectColor = mission.IsCompleted ? CompletedBorderColor : ActiveBorderColor;
            }
        }

        private void ConfigureProgressFill()
        {
            if (progressFill == null)
                return;

            progressFill.type = Image.Type.Filled;
            progressFill.fillMethod = Image.FillMethod.Horizontal;
            progressFill.fillOrigin = 0;
            progressFill.fillClockwise = true;
            progressFill.raycastTarget = false;
            progressFill.enabled = true;
            progressFill.gameObject.SetActive(true);
            progressFill.transform.SetAsLastSibling();
        }

        private static string ToPersianDigits(int value)
        {
            return value.ToString().Replace('0', '۰').Replace('1', '۱').Replace('2', '۲')
                .Replace('3', '۳').Replace('4', '۴').Replace('5', '۵').Replace('6', '۶')
                .Replace('7', '۷').Replace('8', '۸').Replace('9', '۹');
        }
    }
}
