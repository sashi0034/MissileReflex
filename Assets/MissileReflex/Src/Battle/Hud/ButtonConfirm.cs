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
    public class ButtonConfirm : 
        MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler
    {
#nullable disable
        [SerializeField] private Image bgImage;
        public Image BgImage => bgImage;
#nullable enable

        private Subject<Unit> _onConfirmed = new();
        public IObservable<Unit> OnConfirmed => _onConfirmed;
        private UniTask _animConfirm = UniTask.CompletedTask;

        public void Init()
        {
            transform.localScale = Vector3.one;
            bgImage.gameObject.SetActive(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_animConfirm.Status == UniTaskStatus.Pending) return;
            bgImage.gameObject.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_animConfirm.Status == UniTaskStatus.Pending) return;
            bgImage.gameObject.SetActive(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _animConfirm = HudUtil.AnimSmallOneToZero(transform);
            _onConfirmed.OnNext(Unit.Default);
        }
    }
}