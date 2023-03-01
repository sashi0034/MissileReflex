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
#if DEBUG
            if (DebugParam.Instance.IsForceBattleOffline || DebugParam.Instance.IsGuiDebugStartBattle)
            {
                debugBattle();
                return;
            }
#endif
            init();
            loopGame().RunTaskHandlingError();
        }

        private void init()
        {
            Util.ActivateGameObjects(
                gameRoot.Network,
                gameRoot.LobbyHud,
                gameRoot.FrontHud);
            Util.DeactivateGameObjects(
                gameRoot.BattleRoot,
                gameRoot.BattleHud);
        }

#if DEBUG
        private void debugBattle()
        {
            Util.ActivateGameObjects(
                gameRoot.BattleRoot,
                gameRoot.BattleHud,
                gameRoot.Network);
            Util.DeactivateGameObjects(
                gameRoot.LobbyHud,
                gameRoot.FrontHud);
        }
#endif


        private async UniTask loopGame()
        {
            while (true)
            {
                try
                {
                    await flowGame();
                }
                catch (Exception e)
                {
                    UniTaskUtil.LogTaskHandlingError(e);
                }
            }
        }

        private async UniTask flowGame()
        {
            gameRoot.LobbyHud.CleanRestart();
            
            // マッチング終了まで待機
            var sharedState = await gameRoot.LobbyHud.PanelStartMatching.OnMatchingFinished.Take(1);
            
            // 部屋を閉じる
            gameRoot.Network.ModifyRunner(runner => runner.SessionInfo.IsOpen = false);

            // TODO: 新規ステージ
            const int arenaIndex = 1;
            await gameRoot.LoadScene(ConstParam.GetLiteralArena(arenaIndex));

            // プレイヤー全員がシーンロードを終わるまで待機
            if (sharedState != null) sharedState.NotifyLocalLoadedArena();
            gameRoot.BattleRoot.Init();
            await UniTask.WaitUntil(() => sharedState == null || sharedState.IsAllPlayersLoadedArena());
            if (sharedState != null) sharedState.NotifyEnteredBattle();

            HudUtil.AnimSmallOneToZero(gameRoot.LobbyHud.transform).Forget();
            
            // 試合開始
            gameRoot.BattleRoot.Progress.FlowBattle();

            // 終了まで待つ
            await gameRoot.BattleRoot.Progress.OnBattleCompleted.Take(1);

            // ロビーを表示
            await HudUtil.AnimBigZeroToOne(gameRoot.LobbyHud.transform);
            
            // バトルの後片付け
            gameRoot.BattleRoot.ClearBattle();
            
            await gameRoot.LoadScene(ConstParam.LiteralMainScene);
            
            // 部屋をまた開ける
            gameRoot.Network.ModifyRunner(runner => runner.SessionInfo.IsOpen = true);
        }
        
        
    }
}