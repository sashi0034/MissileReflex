#nullable enable

using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MissileReflex.Src.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MissileReflex.Src.Lobby.MenuContents
{
    public class SectionOfflineBattle : MonoBehaviour
    {
#nullable disable
        [SerializeField] private LobbyHud lobbyHud;

        [SerializeField] private TextMeshProUGUI textNotAvailable;
        public TextMeshProUGUI TextNotAvailable => textNotAvailable;

        [SerializeField] private Button buttonStartOfflineBattle;
        public Button ButtonStartOfflineBattle => buttonStartOfflineBattle;
#nullable enable
        private UniTask _taskPerform = UniTask.CompletedTask;

        [EventFunction]
        private void Start()
        {
            buttonStartOfflineBattle.onClick.AddListener(onPushButtonStart);
        }

        [EventFunction]
        private void Update()
        {
            if (_taskPerform.Status == UniTaskStatus.Pending) return;
            
            // こちらはサブ的な扱いなので、PanelStartMatchingを外から触りながら操作していく
            changeAvailable(lobbyHud.PanelStartMatching.CanPushButton());
        }

        [EventFunction]
        private void OnEnable()
        {
            textNotAvailable.transform.localScale = Vector3.one;
            buttonStartOfflineBattle.transform.localScale = Vector3.one;
        }

        private void changeAvailable(bool isAvailable)
        {
            textNotAvailable.gameObject.SetActive(!isAvailable);
            buttonStartOfflineBattle.gameObject.SetActive(isAvailable);
        }

        private void onPushButtonStart()
        {
            _taskPerform = performStartOfflineBattle();
        }

        private async UniTask performStartOfflineBattle()
        {
            buttonStartOfflineBattle.interactable = false;
            lobbyHud.PanelStartMatching.StartOfflineBattle().RunTaskHandlingErrorAsync().Forget();

            await HudUtil.AnimSmallOneToZero(buttonStartOfflineBattle.transform);

            buttonStartOfflineBattle.interactable = true;
        }
    }
}