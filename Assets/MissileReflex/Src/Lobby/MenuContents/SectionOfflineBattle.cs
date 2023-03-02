#nullable enable

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MissileReflex.Src.Lobby.MenuContents
{
    public class SectionOfflineBattle : MonoBehaviour
    {
#nullable disable
        [SerializeField] private TextMeshProUGUI textNotAvailable;
        public TextMeshProUGUI TextNotAvailable => textNotAvailable;

        [SerializeField] private Button buttonStartOfflineBattle;
        public Button ButtonStartOfflineBattle => buttonStartOfflineBattle;
#nullable enable
    }
}