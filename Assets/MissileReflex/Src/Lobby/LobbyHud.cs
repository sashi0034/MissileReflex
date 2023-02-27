#nullable enable

using System;
using Cysharp.Threading.Tasks;
using Fusion;
using MissileReflex.Src.Params;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MissileReflex.Src.Lobby
{
    public class LobbyHud : MonoBehaviour
    {
#nullable disable
        [SerializeField] private GameRoot gameRoot;

        public GameRoot GameRoot => gameRoot;

        [SerializeField] private PanelStartMatching panelStartMatching;
        public PanelStartMatching PanelStartMatching => panelStartMatching;
        
                
#nullable enable

        private LobbySharedState? _sharedState;
        public LobbySharedState? SharedState => _sharedState;

        public void RegisterSharedState(LobbySharedState state)
        {
            _sharedState = state;
        }

        private void Awake() {}

        public void Init()
        {
            _sharedState = null;
        }
    }
}