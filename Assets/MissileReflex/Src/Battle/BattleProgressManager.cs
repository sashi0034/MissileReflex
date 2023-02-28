#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Fusion;
using MissileReflex.Src.Battle.Hud;
using MissileReflex.Src.Front;
using MissileReflex.Src.Params;
using MissileReflex.Src.Utils;
using Sirenix.Utilities;
using UnityEngine;

namespace MissileReflex.Src.Battle
{
    public record BattleFinalResult(
        BattleTankScore[] TankScores,
        BattleTeamScore[] TeamScores);
    
    public class BattleProgressManager : MonoBehaviour
    {
#nullable disable
        [SerializeField] private BattleRoot battleRoot;
        [SerializeField] private NetworkPrefabRef battleSharedStatePrefab;
#nullable enable
        private GameRoot gameRoot => battleRoot.GameRoot;
        private BattleSharedState? _battleSharedState;
        public BattleSharedState? SharedState => _battleSharedState;
        private BattleFinalResult? _result;

        public void Init()
        {
            _result = null;
        }
        
        public void RegisterSharedState(BattleSharedState state)
        {
            _battleSharedState = state;
        }


        public void DebugStartBattle(GameMode gameMode)
        {
            battleRoot.Init();
            gameRoot.Network.DebugStartBattleNetwork(gameMode)
                .ContinueWith(startBattleInternal)
                .RunTaskHandlingError();
        }
        
        public void FlowBattle()
        {
            flowBattleInternal().RunTaskHandlingError(showUiHandlingError);
        }

        private void showUiHandlingError(Exception e)
        {
            // エラーが起こった時にポップアップ表示
            gameRoot.FrontHud.PopupMessageBelt.PerformPopupCaution(
                PopupMessageBeltErrorKind.Handle(e, gameRoot.Network));
        }

        private async UniTask flowBattleInternal()
        {
            try
            {
                await startBattleInternal();
            }
            catch (Exception e)
            {
                UniTaskUtil.LogTaskHandlingError(e);
                showUiHandlingError(e);
            }

            await performFinishBattle();
            
            Debug.Log("finish battle flowchart");
        }

        private async UniTask startBattleInternal()
        {
            var runner = gameRoot.Network.Runner;
            Debug.Assert(runner != null);
            if (runner == null) throw new PopupMessageBeltErrorKind(EPopupMessageBeltKind.HostDisconnected);

            // ホストで共有状態オブジェクトの作成     
            if (runner.IsServer) runner.Spawn(battleSharedStatePrefab);
            updateHudTeamInfo();
                        
            // 共有状態オブジェクトがスポーンされるのを同期
            await UniTask.WaitUntil(() => _battleSharedState != null);
            
            // スタートの表示
            battleRoot.Hud.PerformLabelBattleStart().Forget();
            
            // ホストでタンク召喚
            if (runner.IsServer) summonTanks(runner);

            runner.ProvideInput = true;

            var state = _battleSharedState;
            if (state == null) throw new PopupMessageBeltErrorKind(EPopupMessageBeltKind.HostDisconnected);

            // 制限時間が0になったら試合終了
            await decRemainingTimeUntilZero(state);
            
            FinalizeResult();
            
            // キャンセルトークン発行
            battleRoot.TerminateCancelBattle();
        }

        private async UniTask performFinishBattle()
        {
            // フィニッシュを中央に表示
            await battleRoot.Hud.PerformLabelBattleFinish();
            
            foreach (var hud in battleRoot.Hud.ListHudOnPlaying())
            {
                // いらないHUDを消す
                HudUtil.AnimSmallOneToZero(hud.transform).Forget();
            }
            
            // リザルト表示
            if (_result != null) await battleRoot.Hud.SectionTeamResult.PerformResult(_result);
        }

        private void summonTanks(NetworkRunner runner)
        {
            // プレイヤー召喚
            var players = runner.ActivePlayers.ToList();
            players.ShuffleList();
            players.Sort((a, b) => a.PlayerId - b.PlayerId);
            foreach (var player in players)
                battleRoot.TankManager.SpawnPlayer(runner, player, battleRoot.TankManager.GetNextSpawnInfo(
                    $"Player [{player.PlayerId}]"));

            // AI召喚
            int numAi = ConstParam.MaxTankAgent - players.Count;
            for (int i = 0; i < numAi; ++i)
                battleRoot.TankManager.SpawnAi(runner, battleRoot.TankManager.GetNextSpawnInfo($"AI [{i + 1}]"));
        }

        private async UniTask decRemainingTimeUntilZero(BattleSharedState state)
        {
            state.AddRemainingTime(1);
            
            while (state.RemainingTime > 0)
            {
                state.AddRemainingTime(-1);
                battleRoot.Hud.PanelRemainingTime.UpdateText(state.RemainingTime);
                
                // ラストスパートではスコアを?にしたりする
                if (state.RemainingTime == ConstParam.Instance.BattleTimeLastSpurt) 
                    battleRoot.Hud.PanelCurrTeamInfoManager.EnterLastSpurt();
                
                await UniTask.Delay(1000);
            }
        }
        
        public void MutateScoreOnKill(TankFighter attacker, TankFighter killed)
        {
            if (_battleSharedState == null) return; 
            // if (killed.Team.IsSame(attacker.Team)) return;

            // 同士討ちは減点
            int deltaScore = killed.Team.IsSame(attacker.Team) ? -1 : 1;
            attacker.EarnedScore.IncScore(deltaScore);

            // HUD更新
            battleRoot.Hud.LabelScoreAdditionOnKillManager.BirthLabel(new LabelScoreAdditionOnKillArg(
                killed.transform.position, attacker.Team, deltaScore));
            updateHudTeamInfo();
        }

        private void updateHudTeamInfo()
        {
            if (_battleSharedState == null) return;
            var scores = calcTeamScoreList();
            battleRoot.Hud.PanelCurrTeamInfoManager.UpdateInfo(scores);
        }

        public void FinalizeResult()
        {
            var tankScores = battleRoot.TankManager.List
                .Where(tank => tank != null)
                .Select(tank => new BattleTankScore(tank.LocalId, tank.Team, tank.TankName, tank.EarnedScore));

            var teamScores = calcTeamScoreList();

            _result = new BattleFinalResult(tankScores.ToArray(), teamScores);
        }

        private BattleTeamScore[] calcTeamScoreList()
        {
            var scoreList = new BattleTeamScore[ConstParam.NumTankTeam];
            for (int i = 0; i < scoreList.Length; i++)
            {
                scoreList[i] = new BattleTeamScore(i, 0, 0);
            }
            foreach (var fighter in battleRoot.TankManager.List)
            {
                if (fighter == null) continue;
                int teamId = fighter.Team.TeamId;
                scoreList[teamId] = scoreList[teamId].AddScore(fighter.EarnedScore.Score);
            }
            
            // 順位順にして
            scoreList.Sort((a, b) => b.Score - a.Score);

            int checkingScore = -1;
            int checkingOrder = 0;
            for (var index = 0; index < scoreList.Length; index++)
            {
                var score = scoreList[index];

                if (checkingScore != (checkingScore = score.Score)) checkingOrder++;

                scoreList[index] = score.SetOrder(checkingOrder);
            }
            
            return scoreList;
        }
    }
}