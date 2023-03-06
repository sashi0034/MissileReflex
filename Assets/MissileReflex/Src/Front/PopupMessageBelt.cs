#nullable enable

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Fusion;
using MissileReflex.Src.Connection;
using MissileReflex.Src.Utils;
using TMPro;
using UnityEngine;

namespace MissileReflex.Src.Front
{
    
    
    public class PopupMessageBelt : MonoBehaviour
    {
#nullable disable
        [SerializeField] private TextMeshProUGUI message;
        public TextMeshProUGUI Message => message;
#nullable enable
        private UniTask _taskPerform = UniTask.CompletedTask;
        
        public void PerformPopupCautionFromException(Exception e)
        {
            switch (e)
            {
            case NetworkBattleUnfinishedException:
                pushPerformPopupCaution("バトルのルームが解散しました");
                break;
            default:
                pushPerformPopupCaution("予期せぬエラーが発生しました");
                break;
            }
        }
        public void PerformPopupCautionOnShutdown(ShutdownReason kind)
        {
            switch (kind)
            {
            case ShutdownReason.DisconnectedByPluginLogic:
                pushPerformPopupCaution("ホストが通信を切断しました");
                break;
            case ShutdownReason.Ok:
                pushPerformPopupCaution("通信接続に失敗しました");
                break;
            }
        }
        public void PerformPopupCautionOnPlayerLeft(string playerName)
        {
            pushPerformPopupCaution($"{playerName} が通信を切断しました");
        }

        private void pushPerformPopupCaution(string messageText)
        {
            Util.RunUniTask(async () =>
            {
                await UniTask.WaitUntil(() => _taskPerform.Status != UniTaskStatus.Pending);
                _taskPerform = performPopupCautionInternal(messageText);
            });
        }
        
        private async UniTask performPopupCautionInternal(string messageText)
        {
            SeManager.Instance.PlaySe(SeManager.Instance.SeSectionFront);

            Util.ActivateGameObjects(this);

            message.text = messageText;
            transform.localScale = new Vector3(0, 1, 1);
            await transform.DOScaleX(1f, 0.5f).SetEase(Ease.OutBack);
            await UniTask.Delay(3f.ToIntMilli());
            await transform.DOScaleX(0f, 0.5f).SetEase(Ease.InBack);
            
            Util.DeactivateGameObjects(this);
        }


    }
}