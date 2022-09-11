using UnityEngine;

namespace NinjaBattle.General
{
    public class DontDestroyOnLoad : MonoBehaviour
    {
        #region BEHAVIORS

        private void Awake()
        {

            GameObject[] objs = GameObject.FindGameObjectsWithTag("GameController");

            if (objs.Length > 1)
            {
                Destroy(this.gameObject);
            }

            DontDestroyOnLoad(this.gameObject);
        }

        #endregion
    }
}
