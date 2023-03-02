#nullable enable

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MissileReflex.Src.Lobby.MenuContents
{
    public class PanelInputChatContent : MonoBehaviour
    {
#nullable disable
        [SerializeField] private TMP_InputField inputField;
        public TMP_InputField InputField => inputField;

        [SerializeField] private Button buttonPost;
        public Button ButtonPost => buttonPost;

        [SerializeField] private Toggle toggleEnableWebGlInput;
        public Toggle ToggleEnableWebGlInput => toggleEnableWebGlInput;
#nullable enable
    }
}