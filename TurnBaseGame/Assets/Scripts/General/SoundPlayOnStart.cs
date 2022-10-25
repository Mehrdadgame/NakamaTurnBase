using UnityEngine;

namespace NinjaBattle.General
{
    /// <summary>
    /// this calss for play sound start game 
    /// </summary>
    public class SoundPlayOnStart : MonoBehaviour
    {
        #region FIELDS

        [SerializeField] private AudioClip sound = null;

        #endregion

        #region BEHAVIORS

        private void Start()
        {
            AudioManager.Instance.PlaySound(sound);
        }

        #endregion
    }
}
