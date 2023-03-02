#nullable enable

using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace MissileReflex.Src.Utils
{
    public static class HudUtil
    {
        public static async UniTask AnimSmallOneToZero(Transform target, float duration = 0.3f)
        {
            target.localScale = Vector3.one;
            await target.DOScale(0f, duration).SetEase(Ease.InBack);
            target.gameObject.SetActive(false);
        }
        public static async UniTask AnimSmallOneToZeroX(Transform target, float duration = 0.3f)
        {
            target.localScale = Vector3.one;
            await target.DOScaleX(0f, duration).SetEase(Ease.InBack);
            target.gameObject.SetActive(false);
        }
        public static async UniTask AnimBigZeroToOne(Transform target, float duration = 0.3f)
        {
            target.gameObject.SetActive(true);
            target.localScale = Vector3.zero;
            await target.DOScale(1f, duration).SetEase(Ease.OutBack);
        }
        public static async UniTask AnimBigZeroToOneX(Transform target, float duration = 0.3f)
        {
            target.gameObject.SetActive(true);
            target.localScale = new Vector3(0, 1, 1);
            await target.DOScaleX(1f, duration).SetEase(Ease.OutBack);
        }
        public static async UniTask AnimRectTransformSizeZeroToBeforeY(RectTransform rect, float animDuration)
        {
            float beforeY = rect.sizeDelta.y;
            rect.sizeDelta = rect.sizeDelta.FixY(0);

            await DOTween.To(
                () => rect.sizeDelta.y,
                y => rect.sizeDelta = rect.sizeDelta.FixY(y),
                beforeY,
                animDuration).SetEase(Ease.OutBack);
        }

    }
}