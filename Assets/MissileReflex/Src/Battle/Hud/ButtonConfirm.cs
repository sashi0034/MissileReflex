#nullable enable

using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MissileReflex.Src.Utils;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MissileReflex.Src.Battle.Hud
{
    public class ButtonConfirm : MonoBehaviour
    {
#nullable disable
        [SerializeField] private Image bgImage;
        public Image BgImage => bgImage;
#nullable enable

        private Subject<Unit> _onConfirmed = new();
        public IObservable<Unit> OnConfirmed => _onConfirmed;
        private UniTask _animConfirm = UniTask.CompletedTask;

        public void Init()
        {}

        [EventFunction]
        public void OnPushButton()
        {
            if (_animConfirm.Status == UniTaskStatus.Pending) return;
            
            SeManager.Instance.PlaySe(SeManager.Instance.SeSectionConfirm);
            
            _animConfirm = HudUtil.AnimSmallOneToZero(transform);
            _onConfirmed.OnNext(Unit.Default);
        }
    }
}