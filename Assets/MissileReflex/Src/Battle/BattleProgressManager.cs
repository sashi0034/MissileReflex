#nullable enable

using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Fusion;
using MissileReflex.Src.Battle.Hud;
using MissileReflex.Src.Front;
using MissileReflex.Src.Params;
using MissileReflex.Src.Utils;
using UnityEngine;

namespace MissileReflex.Src.Battle
{
    public class BattleProgressManager : MonoBehaviour
    {
#nullable disable
        [SerializeField] private BattleRoot battleRoot;
        [SerializeField] private NetworkPrefabRef battleSharedStatePrefab;
#nullable enable
        private GameRoot gameRoot => battleRoot.GameRoot;
        private BattleSharedState? _battleSharedState;
        public BattleSharedState? SharedState => _battleSharedState;

        public void RegisterSharedState(BattleSharedState state)
        {
            _battleSharedState = state;
        }


        public void DebugStartBattle(GameMode gameMode)
        {
            battleRoot.Init();
            gameRoot.Network.DebugStartBattleNetwork(gameMode)
                .ContinueWith(startBattleInternal).RunTaskHandlingError();
        }
        
        public void StartBattle()
        {
            startBattleInternal().RunTaskHandlingError(ex =>
            {
                gameRoot.FrontHud.PopupMessageBelt.PerformPopupCaution(PopupMessageBeltErrorKind.Handle(ex, gameRoot.Network));
            });
        }

        private async UniTask startBattleInternal()
        {
            var runner = gameRoot.Network.Runner;
            Debug.Assert(runner != null);
            if (runner == null) throw new PopupMessageBeltErrorKind(EPopupMessageBeltKind.HostDisconnected);

            // ホストがいろいろ生成
            if (runner.IsServer)
            {
                // プレイヤー召喚
                summonTanks(runner);
                
                // 共有状態オブジェクトの作成            
                runner.Spawn(battleSharedStatePrefab);
            }
            
            // 共有状態オブジェクトがスポーンされるのを同期
            await UniTask.WaitUntil(() => _battleSharedState != null);
            updateHudTeamInfo();

            runner.ProvideInput = true;

            var state = _battleSharedState;
            if (state == null) throw new PopupMessageBeltErrorKind(EPopupMessageBeltKind.HostDisconnected);

            // 制限時間が0になったら試合終了
            await decRemainingTimeUntilZero(state);

            battleRoot.TerminateCancelBattle();
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
            _battleSharedState.MutTeamStatesAt(attacker.Team.TeamId).IncScore(deltaScore);

            // HUD更新
            battleRoot.Hud.LabelScoreAdditionOnKillManager.BirthLabel(new LabelScoreAdditionOnKillArg(
                killed.transform.position, attacker.Team, deltaScore));
            updateHudTeamInfo();
        }

        private void updateHudTeamInfo()
        {
            var stateWithId = new BattleTeamStateWithId[_battleSharedState.GetTeamStatesLength()];
            for (var index = 0; index < _battleSharedState.GetTeamStatesLength(); index++)
            {
                var state = _battleSharedState.MutTeamStatesAt(index);
                stateWithId[index] = new BattleTeamStateWithId(index, state);
            }
            battleRoot.Hud.PanelCurrTeamInfoManager.UpdateInfo(stateWithId);
        }
    }
}