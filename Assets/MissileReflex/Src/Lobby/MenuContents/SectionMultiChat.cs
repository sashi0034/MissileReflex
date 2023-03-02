#nullable enable

using UnityEngine;
using UnityEngine.UI;

namespace MissileReflex.Src.Lobby.MenuContents
{
    public class SectionMultiChat : MonoBehaviour
    {
#nullable disable
        [SerializeField] private LabelPostedContent labelPostedContentPrefab;
        public LabelPostedContent LabelPostedContentPrefab => labelPostedContentPrefab;

        [SerializeField] private VerticalLayoutGroup scrollContent;
        public VerticalLayoutGroup ScrollContent => scrollContent;

        [SerializeField] private ScrollRect scrollView;
        public ScrollRect ScrollView => scrollView;

        [SerializeField] private PanelInputChatContent panelInputChatContent;
        public PanelInputChatContent PanelInputChatContent => panelInputChatContent;
        
#nullable enable
    }
}