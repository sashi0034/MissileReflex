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

        private void Awake() {}

        public async UniTask StartBattle()
        {
            gameObject.SetActive(false);

            // TODO: 新規ステージ
            const int arenaIndex = 1;
            await gameRoot.LoadScene(ConstParam.GetLiteralArena(arenaIndex));
            
            gameRoot.BattleRoot.Progress.StartBattle(GameMode.Single);
        }
        
#nullable enable
    }
}