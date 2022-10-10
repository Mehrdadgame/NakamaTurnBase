using UnityEngine;
using UnityEngine.Events;

namespace General
{
    public class OnStart : MonoBehaviour
    {
        #region EVENTS

        public UnityEvent onStart;

        #endregion

        #region BEHAVIORS

        private void Start()
        {
            onStart?.Invoke();
        }

        #endregion
    }
}
