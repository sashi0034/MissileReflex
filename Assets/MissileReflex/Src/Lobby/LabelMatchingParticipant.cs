#nullable enable

using MissileReflex.Src.Params;
using TMPro;
using UnityEngine;

namespace MissileReflex.Src.Lobby
{
    public class LabelMatchingParticipant : MonoBehaviour
    {
#nullable disable
        [SerializeField] private TextMeshProUGUI text;
        public TextMeshProUGUI Text => text;
#nullable enable
        public void SetText(int numParticipant)
        {
            text.text = $"{numParticipant} / {ConstParam.MaxTankAgent}";
        }
    }
}