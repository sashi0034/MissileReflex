#nullable enable

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MissileReflex.Src.Utils
{
    public class UiTextAndImage : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        public TextMeshProUGUI Text => text;

        [SerializeField] private Image image;
        public Image Image => image;
    }
}