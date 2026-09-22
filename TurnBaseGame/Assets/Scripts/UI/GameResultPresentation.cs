using RTLTMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

namespace NinjaBattle.UI
{
    /// <summary>Skins the existing end-game data with the Figma result layout.</summary>
    public sealed class GameResultPresentation : MonoBehaviour
    {
        [SerializeField] private RTLTextMeshPro title;
        [SerializeField] private Image trophy;
        [SerializeField] private Image panel;
        [SerializeField] private Sprite winSprite;
        [SerializeField] private Sprite loseSprite;
        [SerializeField] private Color winColor = new Color32(255, 221, 162, 255);
        [SerializeField] private Color lossColor = new Color32(244, 205, 196, 255);

        public void Configure(RTLTextMeshPro titleText, Image trophyImage, Image panelImage, Sprite win = null, Sprite lose = null)
        {
            title = titleText;
            trophy = trophyImage;
            panel = panelImage;
            if (win != null) winSprite = win;
            if (lose != null) loseSprite = lose;
        }

        public void Refresh(string result)
        {
            bool won = result != null && (result.Contains("برد") || result.Contains("VICTORY") || result.Contains("برنده"));
            bool draw = result != null && (result.Contains("مساوی") || result.Contains("DRAW"));
            if (title != null)
            {
                title.text = won ? "برنده شدی!" : draw ? "بازی مساوی شد" : "باختی!";
                title.color = won ? new Color32(119, 72, 11, 255) : new Color32(126, 56, 42, 255);
            }
            if (panel != null) panel.color = won || draw ? winColor : lossColor;
            if (trophy != null)
            {
                if (won || draw)
                {
                    if (winSprite != null) trophy.sprite = winSprite;
                }
                else
                {
                    if (loseSprite != null) trophy.sprite = loseSprite;
                    else if (winSprite != null) trophy.sprite = winSprite;
                }
                trophy.color = Color.white;
            }

            transform.DOKill();
            transform.localScale = Vector3.zero;
            transform.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack).SetUpdate(true);

            if (trophy != null)
            {
                trophy.rectTransform.DOKill();
                trophy.rectTransform.localScale = Vector3.one;
                if (won)
                    trophy.rectTransform.DOPunchScale(Vector3.one * 0.25f, 0.45f, 5, 0.5f).SetDelay(0.2f).SetUpdate(true);
                else
                    trophy.rectTransform.DOShakePosition(0.35f, new Vector3(8f, 0f, 0f), 10, 90f).SetDelay(0.2f).SetUpdate(true);
            }
        }

        public void ReturnHome()
        {
            SceneManager.LoadScene("2-Home");
        }
    }
}
