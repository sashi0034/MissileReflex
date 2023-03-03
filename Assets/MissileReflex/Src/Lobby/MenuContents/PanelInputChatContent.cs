#nullable enable

using System;
using MissileReflex.Src.Utils;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using WebGLInput = WebGLSupport.WebGLInput;

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
        private readonly Subject<string> _onSubmitInput = new();
        public IObservable<string> OnSubmitInput => _onSubmitInput;
        

        [EventFunction]
        private void Start()
        {
            inputField.onSubmit.AddListener((input) =>
            {
                _onSubmitInput.OnNext(input);
            });
            buttonPost.onClick.AddListener(() =>
            {
                _onSubmitInput.OnNext(inputField.text);
            });
            
            toggleEnableWebGlInput.onValueChanged.AddListener(isOn =>
            {
                inputField.GetComponent<WebGLInput>().enabled = isOn;
            });
            toggleEnableWebGlInput.isOn = false;
        }

        public void CleanInputContent()
        {
            inputField.text = "";
            inputField.ActivateInputField();
        }
    }
}