#nullable enable

using System;
using DG.Tweening;
using MissileReflex.Src.Params;
using MissileReflex.Src.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

namespace MissileReflex.Src.Lobby
{
    public class ButtonMenuCommon : 
        MonoBehaviour, 
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerClickHandler
    {
#nullable disable
        [SerializeField] private PanelSelectMenu parentPanel;
        private UnityEngine.UI.Image bgImage;
#nullable enable
        [EventFunction]
        private void Start()
        {
            bgImage = GetComponent<Image>();
            DisableSelect();
        }


        public void OnPointerEnter(PointerEventData eventData)
        {
            if (parentPanel.CurrSelectedMenu == this) return;
            bgImage.color = Color.black;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (parentPanel.CurrSelectedMenu == this) return;
            DisableSelect();
        }

        public void DisableSelect()
        {
            bgImage.color = ConstParam.Transparent;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            parentPanel.ChangeSelectedMenu(this);
        }

        public void EnableSelect()
        {
            bgImage.color = Util.ColourHex(0x0576b9);
        }
    }
}