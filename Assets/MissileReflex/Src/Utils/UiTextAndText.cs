#nullable enable

using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace MissileReflex.Src.Utils
{
    public class UiTextAndText : MonoBehaviour
    {
#nullable disable
        [SerializeField] private TextMeshProUGUI textFirst;
        public TextMeshProUGUI TextFirst => textFirst;

        [SerializeField] private TextMeshProUGUI textSecond;
        public TextMeshProUGUI TextSecond => textSecond;
#nullable enable
    }
}