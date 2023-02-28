#nullable enable

using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace MissileReflex.Src.Utils
{
    public static class HudUtil
    {
        public static async UniTask AnimSmallOneToZero(Transform target)
        {
            target.localScale = Vector3.one;
            await target.DOScale(0f, 0.3f).SetEase(Ease.InBack);
            target.gameObject.SetActive(false);
        }
        public static async UniTask AnimBigZeroToOne(Transform target, float duration = 0.3f)
        {
            target.gameObject.SetActive(true);
            target.localScale = Vector3.zero;
            await target.DOScale(1f, duration).SetEase(Ease.OutBack);
        }

    }
}