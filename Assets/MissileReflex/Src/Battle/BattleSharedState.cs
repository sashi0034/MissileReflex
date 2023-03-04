#nullable enable

using System.Collections.Generic;
using System.Linq;
using Fusion;
using MissileReflex.Src.Params;
using MissileReflex.Src.Utils;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace MissileReflex.Src.Battle
{
    public record BattleTankScore(
        TankFighterId Id,
        TankFighterTeam Team,
        string TankName,
        bool IsLocalPlayer,
        TankScore Score,
        PlayerRating PlayerRating);

    public record BattleTeamScore(
        int TeamId,
        int Score,
        int Order /* 1-index */)
    {
        public BattleTeamScore AddScore(int score)
        {
            return this with { Score = Score + score };
        }

        public BattleTeamScore SetOrder(int order)
        {
            return this with { Order = order };
        }
    }

    public class BattleSharedState : NetworkBehaviour
    {
        [Networked] private int _remainingTime { get; set; }
        public int RemainingTime => _remainingTime;

        // [Networked] [Capacity(ConstParam.NumTankTeam)]
        // private NetworkArray<BattleTeamState> _teamStates { get; } = MakeInitializer(new BattleTeamState[ConstParam.NumTankTeam]);

        public void AddRemainingTime(int amount)
        {
            _remainingTime += amount;
            if (_remainingTime < 0) _remainingTime = 0;
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