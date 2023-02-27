#nullable enable

using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Fusion;
using MissileReflex.Src.Battle.Hud;
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

        public void RegisterSharedState(BattleSharedState state)
        {
            _battleSharedState = state;
        }


        public void DebugStartBattle(GameMode gameMode)
        {
            gameRoot.Network.DebugStartBattleNetwork(gameMode)
                .ContinueWith(startBattleInternal).RunTaskHandlingError();
        }
        
        public void StartBattle()
        {
            startBattleInternal().RunTaskHandlingError();
        }

        private async UniTask startBattleInternal()
        {
            var runner = gameRoot.Network.Runner;
            Debug.Assert(runner != null);
            if (runner == null) return;
            runner.SessionInfo.IsOpen = false;
            
            if (_battleSharedState != null) Util.DestroyGameObject(_battleSharedState.gameObject);
            battleRoot.Init();

            // 共有状態オブジェクトの作成            
            runner.Spawn(battleSharedStatePrefab);

            // プレイヤー召喚
            var sortedPlayers = runner.ActivePlayers.ToList();
            sortedPlayers.Sort((a, b) => a.PlayerId - b.PlayerId);
            foreach (var player in sortedPlayers)
                battleRoot.TankManager.SpawnPlayer(runner, player, battleRoot.TankManager.GetNextSpawnInfo(
                    $"Player [{player.PlayerId}]"));
            
            runner.ProvideInput = true;
            
            // AI召喚
            int numAi =  ConstParam.MaxTankAgent - sortedPlayers.Count;
            for (int i = 0; i < numAi; ++i)
                battleRoot.TankManager.SpawnAi(runner, battleRoot.TankManager.GetNextSpawnInfo($"AI [{i + 1}]"));
            
            // 共有状態オブジェクトがスポーンされるのを同期
            await UniTask.WaitUntil(() => _battleSharedState != null);
            updateHudTeamInfo();

            var state = _battleSharedState;

            // 制限時間が0になったら試合終了
            await decRemainingTimeUntilZero(state);

            battleRoot.TerminateCancelBattle();
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