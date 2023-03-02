#nullable enable

using TMPro;
using UnityEngine;

namespace MissileReflex.Src.Lobby.MenuContents
{
    public class LabelPostedContent : MonoBehaviour
    {
#nullable disable
        [SerializeField] private TextMeshProUGUI textIndex;
        public TextMeshProUGUI TextIndex => textIndex;

        [SerializeField] private TextMeshProUGUI textCaption;
        public TextMeshProUGUI TextCaption => textCaption;

        [SerializeField] private TextMeshProUGUI textContent;
        public TextMeshProUGUI TextContent => textContent;
#nullable enable
    }
}