using TMPro;
using UnityEngine;

namespace MissileReflex.Src.Battle.Hud
{
    public class PanelCurrTeamInfo : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textOrder;
        public TextMeshProUGUI TextOrder => textOrder;

        [SerializeField] private TextMeshProUGUI textScore;
        public TextMeshProUGUI TextScore => textScore;
        
    }
}