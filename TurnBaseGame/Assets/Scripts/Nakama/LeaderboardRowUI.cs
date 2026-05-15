using RTLTMPro;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nakama.Helpers
{
    /// <summary>
    /// Component on each row prefab in the leaderboard list.
    /// Assign all fields in the prefab Inspector.
    /// </summary>
    public class LeaderboardRowUI : MonoBehaviour
    {
        [SerializeField] public Image           avatarImage;
        [SerializeField] public RTLTextMeshPro  rankText;
        [SerializeField] public RTLTextMeshPro  nameText;
        [SerializeField] public RTLTextMeshPro  scoreText;
        [SerializeField] public RTLTextMeshPro  rewardText;     // جایزه رتبه (فقط top-10)
        [SerializeField] public Image           rowBackground;  // tinted to highlight own row
    }
}
