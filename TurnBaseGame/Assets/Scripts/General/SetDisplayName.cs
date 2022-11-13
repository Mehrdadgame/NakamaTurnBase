using TMPro;
using UnityEngine;
using Nakama.Helpers;

namespace NinjaBattle.General
{
    public class SetDisplayName : MonoBehaviour
    {
        #region FIELDS

        private const float delay = 1f;

        [SerializeField] private TMP_InputField inputField = null;
        [SerializeField] private string[] firstPart = null;
        [SerializeField] private string[] secondPart = null;

        private NakamaUserManager nakamaUserManager = null;

        #endregion

        #region BEHAVIORS

       
        private void OnEnable()
        {
            nakamaUserManager = NakamaUserManager.Instance;
            nakamaUserManager.onLoaded += ObtainName;
            inputField.onValueChanged.AddListener(ValueChanged);
            if (nakamaUserManager.LoadingFinished)
                ObtainName();
        }

        private void OnDestroy()
        {
            inputField.onValueChanged.RemoveListener(ValueChanged);
            nakamaUserManager.onLoaded -= ObtainName;
        }

    /// <summary>
    /// If the user's display name is empty, then generate a random name from the firstPart and
    /// secondPart arrays. Otherwise, set the inputField.text to the user's display name
    /// </summary>
        private void ObtainName()
        {
            if (string.IsNullOrEmpty(nakamaUserManager.DisplayName))
                inputField.text = PlayerPrefs.GetString("USERNAME"); //firstPart[Random.Range(0, firstPart.Length)] + secondPart[Random.Range(0, secondPart.Length)];
            else
                inputField.text = PlayerPrefs.GetString("USERNAME");
        }

    /// <summary>
    /// If the user types a new character, wait for a second, then update the name
    /// </summary>
    /// <param name="newValue">The new value of the textbox</param>
        private void ValueChanged(string newValue)
        {
            CancelInvoke(nameof(UpdateName));
            Invoke(nameof(UpdateName), delay);
        }

     /// <summary>
     /// If the text in the input field is not the same as the display name, then update the display
     /// name
     /// </summary>
        private void UpdateName()
        {
            if (inputField.text != nakamaUserManager.DisplayName)
                nakamaUserManager.UpdateDisplayName(inputField.text);
        }

        #endregion
    }
}
