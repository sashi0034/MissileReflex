#nullable enable

using MissileReflex.Src.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MissileReflex.Src.Battle.Hud
{
    public class LabelTeamResultInfo : MonoBehaviour
    {
#nullable disable
        [SerializeField] private TextMeshProUGUI textOrder;
        public TextMeshProUGUI TextOrder => textOrder;
        
        [SerializeField] private Image imageTeam;
        public Image ImageTeam => imageTeam;

        [SerializeField] private TextMeshProUGUI textScore;
        public TextMeshProUGUI TextScore => textScore;
        
        [SerializeField] private UiTextAndText[] textScoreAndPlayerList;
        public UiTextAndText[] TextScoreAndPlayerList => textScoreAndPlayerList;
#nullable enable
    }
}