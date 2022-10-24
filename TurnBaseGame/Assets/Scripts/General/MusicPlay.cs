using UnityEngine;

namespace NinjaBattle.General
{
    public class MusicPlay : MonoBehaviour
    {
        #region FIELDS

        [SerializeField] private AudioClip music = null;
        [SerializeField] private AudioClip DiceClip = null;
        #endregion

        #region BEHAVIORS

        private void Start()
        {
            AudioManager.Instance.PlayMusic(music);
        }

       
      
        #endregion
    }
}
