#nullable enable

using System;
using Cysharp.Threading.Tasks;
using MissileReflex.Src.Utils;
using UnityEngine;

namespace MissileReflex.Src.Lobby
{
    public class PanelSelectMenu : MonoBehaviour
    {
#nullable disable
        [SerializeField] private LobbyHud lobbyHud;
        private SectionMenuContents sectionMenuContents => lobbyHud.SectionMenuContents;
        
        [SerializeField] private ButtonMenuCommon initialSelectedMenu;
        private ButtonMenuCommon _currSelectedMenu;
        public ButtonMenuCommon CurrSelectedMenu => _currSelectedMenu;
#nullable enable

        public void Init()
        {
            _currSelectedMenu = initialSelectedMenu;
            Util.CallDelayedAfterFrame(() =>
            {
                _currSelectedMenu.EnableSelect();
                Util.ActivateGameObjects(getSectionOf(_currSelectedMenu));
            });
        }

        public void ChangeSelectedMenu(ButtonMenuCommon newSelectedMenu)
        {
            if (_currSelectedMenu == newSelectedMenu) return;
            
            var (beforeSection, afterSection) = 
                (getSectionOf(_currSelectedMenu), getSectionOf(newSelectedMenu));

            animChangeSection(beforeSection, afterSection).Forget();

            _currSelectedMenu.DisableSelect();
            newSelectedMenu.EnableSelect();
            _currSelectedMenu = newSelectedMenu;
        }

        private MonoBehaviour getSectionOf(ButtonMenuCommon selectedMenu)
        {
            int index = selectedMenu.transform.GetSiblingIndex();
            var section = sectionMenuContents.ListSections()[index];
            return section;
        }

        private static async UniTask animChangeSection(
            MonoBehaviour beforeSection, 
            MonoBehaviour afterSection)
        {
            await HudUtil.AnimSmallOneToZeroX(beforeSection.transform, 0.1f);
            await HudUtil.AnimBigZeroToOneX(afterSection.transform, 0.1f);
        }

    }
}