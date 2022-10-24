using Nakama.Helpers;
using UnityEngine;

namespace NinjaBattle.Game
{
    public class InputManager : MonoBehaviour
    {
        #region FIELDS

        [SerializeField] private int delay = 0;
        [SerializeField] private KeyCode keyUp = KeyCode.None;
        [SerializeField] private KeyCode keyLeft = KeyCode.None;
        [SerializeField] private KeyCode keyRight = KeyCode.None;
        [SerializeField] private KeyCode keyDown = KeyCode.None;

        #endregion

        #region BEHAVIORS

        private void SendData(int tick, Direction direction)
        {
            MultiplayerManager.Instance.Send(MultiplayerManager.Code.PlayerInput, new InputData(tick, (int)direction));
        }

        #endregion
    }
}
