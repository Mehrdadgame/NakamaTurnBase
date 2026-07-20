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

        public string MissionId { get; private set; }
        public bool IsCompleted { get; private set; }

        public void Bind(MissionState mission)
        {
            MissionId = mission.MissionId;
            IsCompleted = mission.IsCompleted;

            if (titleText != null)
                titleText.text = mission.Title;
            if (descriptionText != null)
                descriptionText.text = mission.Description;
            if (progressText != null)
            {
                progressText.text = mission.IsCompleted && !mission.IsRepeatable
                    ? "تکمیل شد"
                    : $"{ToPersianDigits(mission.CurrentProgress)} / {ToPersianDigits(mission.Target)}";
            }
            if (rewardText != null)
                rewardText.text = $"{ToPersianDigits(mission.RewardXp)} XP";
            if (completedText != null)
                completedText.gameObject.SetActive(mission.IsCompleted);
            if (progressFill != null)
                progressFill.fillAmount = mission.ProgressRatio;
            if (cardBackground != null)
            {
                cardBackground.color = mission.IsCompleted
                    ? new Color(0.07f, 0.38f, 0.20f, 0.98f)
                    : new Color(0.015f, 0.16f, 0.12f, 0.98f);
            }
        }

        private static string ToPersianDigits(int value)
        {
            string text = value.ToString();
            char[] persianDigits = { '۰', '۱', '۲', '۳', '۴', '۵', '۶', '۷', '۸', '۹' };
            for (int index = 0; index < persianDigits.Length; index++)
                text = text.Replace((char)('0' + index), persianDigits[index]);
            return text;
        }
    }
}
