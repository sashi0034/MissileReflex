#nullable enable

using DG.Tweening;
using MissileReflex.Src.Params;
using MissileReflex.Src.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MissileReflex.Src.Battle.Hud
{
    public class PanelCurrTeamInfo : MonoBehaviour
    {
#nullable disable
        [SerializeField] private TextMeshProUGUI textOrder;
        public TextMeshProUGUI TextOrder => textOrder;

        [SerializeField] private TextMeshProUGUI textScore;
        public TextMeshProUGUI TextScore => textScore;

        [SerializeField] private Image imageIcon;
        public Image ImageIcon => imageIcon;

        [SerializeField] private Animator iconAnimator;
        public Animator Animator => iconAnimator;

#nullable enable
        private const int invalidIndex = -1;
        private int _currOrder = invalidIndex;
        private Tween? _animMove;

        public void Init()
        {
            _animMove?.Kill();
            _currOrder = invalidIndex;
        }

        public void EnterLastSpurt()
        {
            textScore.text = "???";
            textOrder.text = "?th";
            iconAnimator.speed = 3f;
        }

        public void UpdateInfo(
            PanelCurrTeamInfoManager manager, 
            BattleTeamScore teamScore, 
            int order, 
            int viewIndex)
        {
            textScore.text = teamScore.Score.ToString();
            textOrder.text = Util.StringifyOrder(order);
            iconAnimator.speed = 0.5f + 0.5f * (ConstParam.NumTankTeam - order);

            int beforeOrder = _currOrder;
            _currOrder = viewIndex;
            
            if (beforeOrder == invalidIndex)
            {
                // 初期位置
                transform.localPosition = manager.PanelLocalPosList[viewIndex];
            }
            else if (beforeOrder != viewIndex)
            {
                // 順位変動
                _animMove?.Kill();
                _animMove = transform
                    .DOLocalMove(manager.PanelLocalPosList[viewIndex], 0.3f)
                    .SetEase(Ease.InOutSine);
            }
        }
    }
}