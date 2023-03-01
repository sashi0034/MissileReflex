#nullable enable

using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Fusion;
using MissileReflex.Src.Connection;
using MissileReflex.Src.Utils;
using TMPro;
using UnityEngine;

namespace MissileReflex.Src.Front
{
    public enum EPopupMessageBeltKind
    {
        Unexpected,
        ConnectionFailed,
        HostDisconnected
    }

    
    public class PopupMessageBelt : MonoBehaviour
    {
#nullable disable
        [SerializeField] private TextMeshProUGUI message;
        public TextMeshProUGUI Message => message;
#nullable enable
        private UniTask _taskPerform = UniTask.CompletedTask;
        
        public void PerformPopupCautionFromException(Exception e)
        {
            if (_taskPerform.Status == UniTaskStatus.Pending) return;
            _taskPerform = performPopupCautionInternal(EPopupMessageBeltKind.Unexpected);
        }
        public void PerformPopupCautionOnShutdown(ShutdownReason kind)
        {
            if (_taskPerform.Status == UniTaskStatus.Pending) return;
            _taskPerform = kind switch
            {
                ShutdownReason.DisconnectedByPluginLogic  => performPopupCautionInternal(EPopupMessageBeltKind.HostDisconnected),
                ShutdownReason.Ok => UniTask.CompletedTask,
                _ => performPopupCautionInternal(EPopupMessageBeltKind.ConnectionFailed),
            };
        }
        
        private async UniTask performPopupCautionInternal(EPopupMessageBeltKind kind)
        {
            Util.ActivateGameObjects(this);
            
            message.text = kind switch
            {
                EPopupMessageBeltKind.Unexpected => "予期せぬエラーが発生しました",
                EPopupMessageBeltKind.ConnectionFailed => "通信接続に失敗しました",
                EPopupMessageBeltKind.HostDisconnected => "ホストが通信を切断しました",
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
            };
            transform.localScale = new Vector3(0, 1, 1);
            await transform.DOScaleX(1f, 0.5f).SetEase(Ease.OutBack);
            await UniTask.Delay(3f.ToIntMilli());
            await transform.DOScaleX(0f, 0.5f).SetEase(Ease.InBack);
            
            Util.DestroyComponent(this);
        }


    }
}