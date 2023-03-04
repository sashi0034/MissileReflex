#nullable enable

using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using MissileReflex.Src.Battle;
using MissileReflex.Src.Lobby;
using MissileReflex.Src.Params;
using MissileReflex.Src.Storage;
using MissileReflex.Src.Utils;
using Sirenix.OdinInspector;
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
            if (DebugParam.Instance.IsForceOfflineBattle || DebugParam.Instance.IsGuiDebugStartBattle)
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
            
            gameRoot.LobbyHud.Init();
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
            await UniTask.DelayFrame(1);
            gameRoot.ResetSaveData();
            gameRoot.ReadSaveData();
            correctSaveDataAfterRead();
            
            gameRoot.LobbyHud.CleanRestart();

            while (true)
            {
                try
                {
                    await flowGame();
                }
                catch (Exception e)
                {
                    UniTaskUtil.LogTaskHandlingError(e);
                    init();
                    gameRoot.FrontHud.PopupMessageBelt.PerformPopupCautionFromException(e);
                }
            }
        }

        private void correctSaveDataAfterRead()
        {
            if (gameRoot.SaveData.IsEnteredBattle == false) return;

            // 前回ゲームを切っていたら最下位として減点する
            updatePlayerRatingFromResult(new BattleLocalPlayerResult(
                ConstParam.NumTankTeam,
                0,
                EBattleFinishedStatus.Completed,
                true));
            gameRoot.SaveData.SetEnteredBattle(false);
        }

        private async UniTask flowGame()
        {
            // マッチング終了まで待機
            var sharedState = await gameRoot.LobbyHud.PanelStartMatching.OnMatchingFinished.Take(1);
            
            // 部屋を閉じる
            gameRoot.Network.ModifyRunner(runner => runner.SessionInfo.IsOpen = false);

            // TODO: 新規ステージ
            const int arenaIndex = 1;
            await gameRoot.LoadScene(ConstParam.GetLiteralArena(arenaIndex));

            if (sharedState != null) sharedState.NotifyLocalLoadedArena();
            gameRoot.BattleRoot.Init();
            
            // プレイヤー全員がシーンロードを終わるまで待機
            await waitForAllPlayersLoadedArena(sharedState, gameRoot.SaveData);
            
            if (sharedState != null) sharedState.NotifyEnteredBattle();
            
            gameRoot.SaveData.SetEnteredBattle(true);
            gameRoot.WriteSaveData();

            HudUtil.AnimSmallOneToZero(gameRoot.LobbyHud.transform).Forget();
            
            // 試合開始
            gameRoot.BattleRoot.Progress.FlowBattle();

            // 終了まで待つ
            var playerResult = await gameRoot.BattleRoot.Progress.OnBattleCompleted.Take(1);
            
            // 結果を保存
            updatePlayerRatingFromResult(playerResult);
            gameRoot.SaveData.SetEnteredBattle(false);
            gameRoot.WriteSaveData();

            // バトルの後片付け
            gameRoot.BattleRoot.ClearBattle();
            
            await gameRoot.LoadScene(ConstParam.LiteralMainScene);

            // ロビーを表示
            gameRoot.LobbyHud.CleanRestart();
            await HudUtil.AnimBigZeroToOne(gameRoot.LobbyHud.transform);

            // 部屋をまた開ける
            gameRoot.Network.ModifyRunner(runner => runner.SessionInfo.IsOpen = true);
        }

        private static async UniTask waitForAllPlayersLoadedArena(LobbySharedState? sharedState, SaveData saveData)
        {
            float failureCheckingTime = 0;
            
            while (true)
            {
                if (sharedState == null ||
                    // プレイヤー全員がシーンロードを終わるまで待機
                    sharedState.IsAllPlayersLoadedArena() ||
                    // まれにぎりぎりで参戦すると同期がずれるので一応これでもOK
                    sharedState.HasEnteredBattle) break;
                
                // ぎりぎり参戦とかだとたまに同期がおかしいので一応確認しておく
                if ((failureCheckingTime += Time.deltaTime) >= 1f)
                {
                    failureCheckingTime = 0;
                    if (sharedState.GetPlayerStatus(sharedState.Runner.LocalPlayer).HasLoadedArena == false)
                    {
                        sharedState.NotifyPlayerInfoFromSaveData(saveData);
                        sharedState.NotifyLocalLoadedArena();
                    }
                }

                await UniTask.DelayFrame(1);
            }
        }

        private void updatePlayerRatingFromResult(BattleLocalPlayerResult? playerResult)
        {
            if (playerResult == null) return;

            var newRating = gameRoot.SaveData.PlayerRating.CalcNewRating(playerResult, out int ratingDelta);
            
            gameRoot.SaveData.SetPlayerRating(newRating);
            gameRoot.LobbyHud.SectionMenuContents.SectionPlayerInfo.SetRatingDeltaByLastBattle(ratingDelta);
        }

#if UNITY_EDITOR
        [Button]
        public void TestCalcNewRating(
            int currRating,
            int teamOrder, 
            int selfScore, 
            EBattleFinishedStatus finishedStatus)
        {
            var newRating =
                new PlayerRating(currRating).CalcNewRating(new BattleLocalPlayerResult(
                    teamOrder, 
                    selfScore, 
                    finishedStatus,
                    true), 
                    out int ratingDelta);
            Debug.Log($"{currRating} -> {newRating} (delta: {ratingDelta})");
        }
#endif

        
        
    }
}