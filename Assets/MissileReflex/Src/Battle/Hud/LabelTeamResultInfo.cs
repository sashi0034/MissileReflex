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
        [SerializeField] private Image imageTeam;
        [SerializeField] private TextMeshProUGUI textScore;
        [SerializeField] private UiTextAndText[] textScoreAndPlayerList;
#nullable enable
    }
}