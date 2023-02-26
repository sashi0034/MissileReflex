#nullable enable

using System;
using MissileReflex.Src.Utils;
using UnityEngine;

namespace MissileReflex.Src.Lobby
{
    public class PanelSelectMenu : MonoBehaviour
    {
#nullable disable
        [SerializeField] private ButtonMenuCommon initialSelectedMenu;
        private ButtonMenuCommon _currSelectedMenu;
        public ButtonMenuCommon CurrSelectedMenu => _currSelectedMenu;
#nullable enable

        private void Start()
        {
            _currSelectedMenu = initialSelectedMenu;
            Util.CallDelayedAfterFrame(_currSelectedMenu.EnableSelect);
        }

        public void ChangeSelectedMenu(ButtonMenuCommon currSelectedMenu)
        {
            _currSelectedMenu.DisableSelect();
            _currSelectedMenu = currSelectedMenu;
        }
    }
}