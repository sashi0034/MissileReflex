#nullable enable

using System;
using Cysharp.Threading.Tasks;
using MissileReflex.Src.Lobby;
using MissileReflex.Src.Params;
using MissileReflex.Src.Utils;
using UniRx;
using UnityEngine;

namespace MissileReflex.Src
{
    public class GameFlowchart : MonoBehaviour
    {
#nullable disable
        [SerializeField] private GameRoot gameRoot;
#nullable enable
        [EventFunction]
        private void Start()
        {
            flowGame().RunTaskHandlingError();
        }

        private async UniTask flowGame()
        {
            Util.ActivateGameObjects(
                gameRoot.Network, 
                gameRoot.LobbyHud);
            Util.DeactivateGameObjects(
                gameRoot.BattleRoot, 
                gameRoot.BattleHud);
            
            gameRoot.LobbyHud.Init();
            
            // マッチング終了まで待機
            var sharedState = await gameRoot.LobbyHud.PanelStartMatching.OnMatchingFinished.Take(1);
            
            // 部屋を閉じる
            gameRoot.Network.ModifyRunner(runner => runner.SessionInfo.IsOpen = false);

            // TODO: 新規ステージ
            const int arenaIndex = 1;
            await gameRoot.LoadScene(ConstParam.GetLiteralArena(arenaIndex));

            // プレイヤー全員がシーンロードを終わるまで待機
            sharedState.NotifyLocalLoadedArena();
            await UniTask.WaitUntil(() => sharedState.IsAllPlayersLoadedArena());

            HudUtil.AnimSmallOneToZero(gameRoot.LobbyHud.transform).Forget();
            
            // 試合開始
            gameRoot.BattleRoot.Progress.StartBattle();
            
        }
        
        
    }
}