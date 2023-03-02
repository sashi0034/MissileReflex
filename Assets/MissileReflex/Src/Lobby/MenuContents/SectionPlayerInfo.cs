#nullable enable

using TMPro;
using UnityEngine;

namespace MissileReflex.Src.Lobby.MenuContents
{
    public class SectionPlayerInfo : MonoBehaviour
    {
#nullable disable
        [SerializeField] private TMP_InputField inputPlayerName;
        public TMP_InputField InputPlayerName => inputPlayerName;

        [SerializeField] private TextMeshProUGUI textPlayerRating;
        public TextMeshProUGUI TextPlayerRating => textPlayerRating;

        [SerializeField] private TextMeshProUGUI textRatingDelta;
        public TextMeshProUGUI TextRatingDelta => textRatingDelta;
        
#nullable enable
    }
}