#nullable enable

using MissileReflex.Src.Lobby.MenuContents;
using UnityEngine;

namespace MissileReflex.Src.Lobby
{
    public class SectionMenuContents : MonoBehaviour
    {
#nullable disable
        [SerializeField] private SectionPlayerInfo sectionPlayerInfo;
        public SectionPlayerInfo SectionPlayerInfo => sectionPlayerInfo;

        [SerializeField] private SectionMultiChat sectionMultiChat;
        public SectionMultiChat SectionMultiChat => sectionMultiChat;
        
        [SerializeField] private SectionOfflineBattle sectionOfflineBattle;
        public SectionOfflineBattle SectionOfflineBattle => sectionOfflineBattle;
        
        [SerializeField] private SectionHelp sectionHelp;
        public SectionHelp SectionHelp => sectionHelp;
#nullable enable
    }
}