using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NinjaBattle.General
{
    public class SceneChanger : MonoBehaviour
    {
        #region FIELDS

        [SerializeField] private float delay = 0f;
        [SerializeField] private Scenes scene;

        #endregion

        #region BEHAVIORS

       /// <summary>
       /// It starts a coroutine that changes the scene.
       /// </summary>
        public void ChangeScene()
        {
            StartCoroutine(ChangeSceneCoroutine());
        }

 /// <summary>
 /// Wait for a few seconds, then load the scene that's specified in the scene variable.
 /// </summary>
        private IEnumerator ChangeSceneCoroutine()
        {
            yield return new WaitForSeconds(delay);
            SceneManager.LoadScene((int)scene);
        }

        #endregion
    }
}
