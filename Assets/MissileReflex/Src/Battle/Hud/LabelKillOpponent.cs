using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MissileReflex.Src.Utils;
using TMPro;
using UnityEngine;

namespace MissileReflex.Src.Battle.Hud
{
    public class LabelKillOpponent : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textMesh;

        public async UniTask PerformMessage(string message, CancellationToken cancel)
        {
            textMesh.text = message;
            
            const float animDuration = 0.3f;
            var rect = GetComponent<RectTransform>();

            transform.localScale = Vector3.zero;
            var beforeSizeY = rect.sizeDelta.y;
            rect.sizeDelta = rect.sizeDelta.FixY(0);

            // 大きくなって
            transform.DOScale(1f, animDuration).SetEase(Ease.OutBack);
            await DOTween.To(
                () => rect.sizeDelta.y,
                y => rect.sizeDelta = rect.sizeDelta.FixY(y),
                beforeSizeY,
                animDuration).SetEase(Ease.OutBack);

            await UniTask.Delay(3.0f.ToIntMilli(), cancellationToken: cancel);

            // 小さくなる
            transform.DOScale(0f, animDuration).SetEase(Ease.InBack);
            await DOTween.To(
                () => rect.sizeDelta.y,
                y => rect.sizeDelta = rect.sizeDelta.FixY(y),
                0,
                animDuration).SetEase(Ease.InBack);

            Util.DestroyGameObject(gameObject);
        }
    }
}