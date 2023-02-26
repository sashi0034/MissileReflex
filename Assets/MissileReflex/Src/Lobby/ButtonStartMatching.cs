﻿#nullable enable

using Cysharp.Threading.Tasks;
using MissileReflex.Src.Utils;
using UnityEngine;

namespace MissileReflex.Src.Lobby
{
    public class ButtonStartMatching : MonoBehaviour
    {
#nullable disable
        [SerializeField] private LobbyHud lobbyHud;
        
#nullable enable
        
        [EventFunction]
        public void OnPushButton()
        {
            onPushButtonInternal().RunTaskHandlingError();
        }

        private async UniTask onPushButtonInternal()
        {
            await HudUtil.AnimSmallOneToZero(transform);
            await lobbyHud.StartBattle();
        }
    }
}