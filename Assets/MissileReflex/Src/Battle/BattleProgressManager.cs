#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Fusion;
using MissileReflex.Src.Battle.Hud;
using MissileReflex.Src.Connection;
using MissileReflex.Src.Front;
using MissileReflex.Src.Params;
using MissileReflex.Src.Utils;
using Sirenix.Utilities;
using UniRx;
using UnityEngine;

namespace MissileReflex.Src.Battle
{
    public enum EBattleFinishedStatus
    {
        ErroredAtEarly,
        ErroredAtLastSpurt,
        Completed,
    }
    
    public record BattleFinalResult(
        BattleTankScore[] TankScores,
        BattleTeamScore[] TeamScores,
        EBattleFinishedStatus FinishedStatus,
        bool IsOnlineBattle);

    public record BattleLocalPlayerResult(
        int TeamOrder,
        int SelfScore,
        EBattleFinishedStatus FinishedStatus,
        bool IsOnlineBattle);
    
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
        private readonly Subject<BattleLocalPlayerResult?> _onBattleCompleted = new();
        public IObservable<BattleLocalPlayerResult?> OnBattleCompleted => _onBattleCompleted;
        

        public void Init()
        {
            ClearBattle();
        }

        public void ClearBattle()
        {
            if (_battleSharedState != null)
                Util.DespawnNetworkObjectSurely(_battleSharedState).RunTaskHandlingError();
            _result = null;
        }
        
        public void RegisterSharedState(BattleSharedState state)
        {
            Debug.Assert(state == null);
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
            UniTaskUtil.LogTaskHandlingError(e);
            // エラーが起こった時にポップアップ表示
            gameRoot.FrontHud.PopupMessageBelt.PerformPopupCautionFromException(e);
        }

        private async UniTask flowBattleInternal()
        {
            try
            {
                await startBattleInternal();
            }
            catch (Exception e)
            {
                showUiHandlingError(e);
            }

            await performFinishBattle();
            
            Debug.Log("finish battle flowchart");
            
            _onBattleCompleted.OnNext(getLocalPlayerResult());
        }

        private BattleLocalPlayerResult? getLocalPlayerResult()
        {
            if (_result == null) return null;
            
            int playerTeamId = -1;
            int playerScore = 0;
            foreach (var score in _result.TankScores)
            {
                if (score.IsLocalPlayer == false) continue;
                playerTeamId = score.Team.TeamId;
                playerScore = score.Score.Score;
                break;
            }

            if (playerTeamId == -1) return null;
            var team = _result.TeamScores.FirstOrDefault(team => team.TeamId == playerTeamId);
            if (team == null) return null;
            int teamOrder = team.Order;

            return new BattleLocalPlayerResult(
                teamOrder, playerScore, _result.FinishedStatus, _result.IsOnlineBattle);
        }

        private async UniTask startBattleInternal()
        {
            var runner = gameRoot.Network.Runner;
            Debug.Assert(runner != null);
            if (runner == null) throw new NetworkObjectMissingException();

            while (true)
            {
                // 疑似ホストで共有状態オブジェクトの作成
                if (gameRoot.Network.IsLocalPlayerPseudoHost()) runner.Spawn(battleSharedStatePrefab);
                // 共有状態オブジェクトがスポーンされるのを同期
                if (_battleSharedState != null) break;
                await UniTask.DelayFrame(1);
            }

            // ホストでタンク召喚
            if (gameRoot.Network.IsLocalPlayerPseudoHost()) summonTanks(runner);

            updateHudTeamInfo();

            // スタートの表示
            battleRoot.Hud.PerformLabelBattleStart().Forget();
            SeManager.Instance.PlaySe(SeManager.Instance.SeBattleStart);
            
            runner.ProvideInput = true;

            var state = _battleSharedState;
            if (state == null) throw new NetworkObjectMissingException();

            // 制限時間が0になったら試合終了
            await decRemainingTimeUntilZero(state);
            SeManager.Instance.PlaySe(SeManager.Instance.SeBattleFinish);
            
            FinalizeResult();
            runner.ProvideInput = false;
            
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
            // players.Sort((a, b) => a.PlayerId - b.PlayerId);
            
            foreach (var player in players)
            {
                var playerInfo = gameRoot.LobbyHud.SharedState != null 
                    ? gameRoot.LobbyHud.SharedState.GetPlayerStatus(player).Info
                    : new PlayerGeneralInfo(); 
                
                battleRoot.TankManager.SpawnPlayer(runner, player, battleRoot.TankManager.GetNextSpawnInfo(playerInfo));
            }
            // AI召喚
            int numAi = ConstParam.MaxTankAgent - players.Count;
            for (int i = 0; i < numAi; ++i)
                battleRoot.TankManager.SpawnAi(runner, battleRoot.TankManager.GetNextSpawnInfo(
                    new PlayerGeneralInfo(PlayerRating.InvalidRating, $"AI [{i + 1}]")));
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

            // プレイヤーが得点したときは音鳴らす
            if (deltaScore > 0 && attacker.IsOwnerLocalPlayer()) 
                SeManager.Instance.PlaySe(SeManager.Instance.SePlayerScored);

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
            var aliveTanks = battleRoot.TankManager.List
                .Where(tank => tank != null).ToArray();
            
            if (aliveTanks.Length == 0) return;
            
            var tankScores = aliveTanks
                .Select(tank => new BattleTankScore(
                    tank.LocalId, tank.Team, tank.TankName, tank.IsOwnerLocalPlayer(), tank.EarnedScore, tank.PlayerRating));

            var teamScores = calcTeamScoreList();

            int remainingTime = _battleSharedState == null ? Int32.MaxValue : _battleSharedState.RemainingTime;
            var finishedState = remainingTime <= 1
                ? EBattleFinishedStatus.Completed
                : remainingTime <= ConstParam.Instance.BattleTimeLastSpurt
                    ? EBattleFinishedStatus.ErroredAtLastSpurt
                    : EBattleFinishedStatus.ErroredAtEarly;

            _result = new BattleFinalResult(
                tankScores.ToArray(), 
                teamScores,
                finishedState,
                _battleSharedState != null && _battleSharedState.Runner.GameMode != GameMode.Single);
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