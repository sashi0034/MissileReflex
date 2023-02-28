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

    public class PopupMessageBeltErrorKind : Exception
    {
        public readonly EPopupMessageBeltKind Kind;

        public PopupMessageBeltErrorKind(EPopupMessageBeltKind kind)
        {
            Kind = kind;
        }

        public static PopupMessageBeltErrorKind Handle(Exception ex, NetworkManager networkManager)
        {
            if (ex is PopupMessageBeltErrorKind expected) return expected;
            return networkManager.LastShutdownReason switch
            {
                ShutdownReason.DisconnectedByPluginLogic => new PopupMessageBeltErrorKind(EPopupMessageBeltKind.HostDisconnected),
                _ => new PopupMessageBeltErrorKind(EPopupMessageBeltKind.Unexpected)
            };
        }
    }
    
    public class PopupMessageBelt : MonoBehaviour
    {
#nullable disable
        [SerializeField] private TextMeshProUGUI message;
        public TextMeshProUGUI Message => message;
#nullable enable
        private UniTask _taskPerform = UniTask.CompletedTask;
        
        public void PerformPopupCaution(PopupMessageBeltErrorKind kind)
        {
            if (_taskPerform.Status == UniTaskStatus.Pending) return;
            _taskPerform = performPopupCautionInternal(kind);
        }
        
        private async UniTask performPopupCautionInternal(PopupMessageBeltErrorKind kind)
        {
            Util.ActivateGameObjects(this);
            
            message.text = kind.Kind switch
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