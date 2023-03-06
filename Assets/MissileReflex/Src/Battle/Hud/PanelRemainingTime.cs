using MissileReflex.Src.Params;
using TMPro;
using UnityEngine;

namespace MissileReflex.Src.Battle.Hud
{
    public class PanelRemainingTime : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        public TextMeshProUGUI Text => text;

        public void Init()
        {
            text.text = "?";
        }

        public void UpdateText(int seconds)
        {
            text.text = seconds.ToString();
        }
    }
}