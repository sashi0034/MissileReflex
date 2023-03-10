#nullable enable

using System;
using Cysharp.Threading.Tasks;
using Fusion;
using MissileReflex.Src.Lobby.MenuContents;
using MissileReflex.Src.Params;
using MissileReflex.Src.Utils;
using Newtonsoft.Json.Bson;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace MissileReflex.Src.Lobby
{
    public class LobbyHud : MonoBehaviour
    {
#nullable disable
        [SerializeField] private GameRoot gameRoot;

        public GameRoot GameRoot => gameRoot;

        [SerializeField] private PanelStartMatching panelStartMatching;
        public PanelStartMatching PanelStartMatching => panelStartMatching;

        [SerializeField] private SectionMenuContents sectionMenuContents;
        public SectionMenuContents SectionMenuContents => sectionMenuContents;

        [SerializeField] private PanelSelectMenu panelSelectMenu;
        public PanelSelectMenu PanelSelectMenu => panelSelectMenu;
        
        
#nullable enable

        private LobbySharedState? _sharedState;
        public LobbySharedState? SharedState => _sharedState;
        
        public SectionMultiChat SectionMultiChatRef => sectionMenuContents.SectionMultiChat;


        public void RegisterSharedState(LobbySharedState state)
        {
            Debug.Assert(_sharedState == null);
            if (_sharedState != null)
            {
                // 万一重複してしまったら、PlayerIdの小さい方を採用
                if (_sharedState.Object.StateAuthority.PlayerId < state.Object.StateAuthority.PlayerId)
                {
                    state.Runner.Despawn(state.Object);
                    return;
                }
                _sharedState.Runner.Despawn(_sharedState.Object);
            }
            _sharedState = state;
        }

        public void Init()
        {
            if (_sharedState != null) _sharedState.Runner.Shutdown();
            
            panelStartMatching.Init();
                
            sectionMenuContents.Init();
            panelSelectMenu.Init();
        }

        // まだ通信接続していてSharedStateがあれば再利用する
        public void CleanRestart()
        {
            Util.ActivateGameObjects(
                panelStartMatching);

            if (_sharedState != null) _sharedState.CleanRestart();
            
            panelStartMatching.CleanRestart();
            panelSelectMenu.CleanRestart();
            sectionMenuContents.CleanRestart();
        }
    }
}