﻿#nullable enable

using System;
using Cysharp.Threading.Tasks;
using Fusion;
using MissileReflex.Src.Params;
using MissileReflex.Src.Utils;
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

        [SerializeField] private SectionMenuContents sectionMenuContents;
        public SectionMenuContents SectionMenuContents => sectionMenuContents;
        
#nullable enable

        private LobbySharedState? _sharedState;
        public LobbySharedState? SharedState => _sharedState;

        public void RegisterSharedState(LobbySharedState state)
        {
            _sharedState = state;
        }

        private void Awake() {}

        // まだ通信接続していてSharedStateがあれば再利用する
        public void CleanRestart()
        {
            Util.ActivateGameObjects(
                panelStartMatching);

            if (_sharedState != null) _sharedState.CleanRestart();
            
            panelStartMatching.CleanRestart();
        }
    }
}