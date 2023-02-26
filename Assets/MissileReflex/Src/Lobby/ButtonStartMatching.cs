#nullable enable

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
            lobbyHud.StartBattle().RunTaskHandlingError();
        }
    }
}