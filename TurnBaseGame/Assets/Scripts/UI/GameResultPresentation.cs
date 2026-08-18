using RTLTMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NinjaBattle.UI
{
    /// <summary>Skins the existing end-game data with the Figma result layout.</summary>
    public sealed class GameResultPresentation : MonoBehaviour
    {
        [SerializeField] private RTLTextMeshPro title;
        [SerializeField] private Image trophy;
        [SerializeField] private Image panel;
        [SerializeField] private Color winColor = new Color32(255, 221, 162, 255);
        [SerializeField] private Color lossColor = new Color32(244, 205, 196, 255);

        public void Configure(RTLTextMeshPro titleText, Image trophyImage, Image panelImage)
        {
            title = titleText;
            trophy = trophyImage;
            panel = panelImage;
        }

        public void Refresh(string result)
        {
            bool won = result != null && result.Contains("برد");
            bool draw = result != null && result.Contains("مساوی");
            if (title != null)
            {
                title.text = won ? "برنده شدی!" : draw ? "بازی مساوی شد" : "این بار نشد";
                title.color = won ? new Color32(119, 72, 11, 255) : new Color32(126, 56, 42, 255);
            }
            if (panel != null) panel.color = won || draw ? winColor : lossColor;
            if (trophy != null) trophy.color = won || draw ? Color.white : new Color(0.72f, 0.72f, 0.72f, 1f);
        }

        public void ReturnHome()
        {
            SceneManager.LoadScene("2-Home");
        }
    }
}
