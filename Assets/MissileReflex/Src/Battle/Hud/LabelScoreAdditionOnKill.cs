#nullable enable

using System;
using DG.Tweening;
using MissileReflex.Src.Params;
using MissileReflex.Src.Utils;
using TMPro;
using UnityEngine;

namespace MissileReflex.Src.Battle.Hud
{
    public record LabelScoreAdditionOnKillArg(
        Vector3 Pos,
        TankFighterTeam Team,
        int ScoreAddition);
    
    public class LabelScoreAdditionOnKill : MonoBehaviour
    {
#nullable disable
        private RectTransform selfRect;
        [SerializeField] private TextMeshProUGUI textMesh;
#nullable enable
        
        private Vector3 _worldPos;
        private const float showingLifetime = 2.5f;
        private float _passedTime = 0;

        [EventFunction]
        private void Awake()
        {
            selfRect = GetComponent<RectTransform>();
        }

        public void SetupView(LabelScoreAdditionOnKillArg arg)
        {
            _worldPos = arg.Pos;
            textMesh.color = ConstParam.Instance.MatTeamColor[arg.Team.TeamId].color;
            string prefix = arg.ScoreAddition > 0 ? "+" : "";
            textMesh.text = $"{prefix}{arg.ScoreAddition}";

            DOTween.Sequence(transform)
                .Append(transform.DOScale(1.1f, 0.1f).SetEase(Ease.OutBack))
                .Append(transform.DOScale(1.0f, 0.1f).SetEase(Ease.InSine))
                .SetLoops(-1)
                .SetLink(gameObject);
        }

        [EventFunction]
        public void Update()
        {
            if ((_passedTime += Time.deltaTime) > showingLifetime)
            {
                Util.DestroyGameObject(gameObject);
                return;
            }

            selfRect.position =
                RectTransformUtility.WorldToScreenPoint(Camera.main, _worldPos);
        }
    }
}