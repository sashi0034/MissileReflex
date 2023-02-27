#nullable enable

using System;
using MissileReflex.Src.Utils;
using TMPro;
using UnityEngine;

namespace MissileReflex.Src.Front
{
    public class FrontHud : MonoBehaviour
    {
#nullable disable
        [SerializeField] private PopupMessageBelt popupMessageBelt;
        public PopupMessageBelt PopupMessageBelt => popupMessageBelt;
#nullable enable
        public void Start()
        {
            Util.DeactivateGameObjects(
                popupMessageBelt);
        }
    }
}