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

        private void Awake()
        {
            foreach (RTLTextMeshPro text in GetComponentsInChildren<RTLTextMeshPro>(true))
                text.PreserveNumbers = true;

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
                titleText.text = mission.Title;
            if (descriptionText != null)
                descriptionText.text = mission.Description;
            if (progressText != null)
            {
                progressText.text = mission.IsCompleted && !mission.IsRepeatable
                    ? "تکمیل شد"
                    : $"{mission.CurrentProgress} / {mission.Target}";
            }
            if (rewardText != null)
                rewardText.text = $"{mission.RewardXp} XP";
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

      
    }
}
