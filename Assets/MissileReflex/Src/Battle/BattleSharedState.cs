#nullable enable

using System.Collections.Generic;
using System.Linq;
using Fusion;
using MissileReflex.Src.Params;
using MissileReflex.Src.Utils;
using UnityEngine;

namespace MissileReflex.Src.Battle
{
    public struct BattleTeamState : INetworkStruct
    {
        private int _score;
        public int Score => _score;

        public BattleTeamState IncScore(int delta)
        {
            _score += delta;
            return this;
        }
    }

    public record BattleTeamStateWithId(
        int Id,
        BattleTeamState State
        )
    {}

    public class BattleSharedState : NetworkBehaviour
    {
        [Networked] private int _remainingTime { get; set; }
        public int RemainingTime => _remainingTime;

        [Networked] [Capacity(ConstParam.NumTankTeam)]
        private NetworkArray<BattleTeamState> _teamStates { get; } = MakeInitializer(new BattleTeamState[ConstParam.NumTankTeam]);

        public void AddRemainingTime(int amount)
        {
            _remainingTime += amount;
        }
        
        public ref BattleTeamState MutTeamStatesAt(int index)
        {
            return ref _teamStates.GetRef(index);
        }

        public int GetTeamStatesLength()
        {
            return _teamStates.Length;
        }
        

        private static BattleRoot battleRoot => BattleRoot.Instance; 

        public override void Spawned()
        {
            battleRoot.Progress.RegisterSharedState(this);
            transform.parent = battleRoot.Progress.transform;

            _remainingTime = ConstParam.Instance.BattleTimeLimit;
        }
    }
}