#nullable enable

using System;
using Fusion;
using MissileReflex.Src.Battle;
using MissileReflex.Src.Params;
using UnityEngine;

namespace MissileReflex.Src
{
    [Serializable]
    public struct PlayerRating : INetworkStruct
    {
        private const int invalidValue = -1;
        [SerializeField] private int _value;
        public int Value => _value;
        
        public PlayerRating(int value)
        {
            _value = value;
        }

        public static readonly PlayerRating InvalidRating = new PlayerRating(invalidValue);

        public bool IsValid()
        {
            return _value != invalidValue;
        }

        public override string ToString()
        {
            return _value.ToString();
        }

        public PlayerRating CalcNewRating(BattleLocalPlayerResult playerResult, out int ratingDelta)
        {
            ratingDelta = calcNewRatingDelta(playerResult);

            return new PlayerRating(Mathf.Max(_value + ratingDelta, 0));
        }

        private static int calcNewRatingDelta(BattleLocalPlayerResult playerResult)
        {
            Debug.Assert(playerResult.TeamOrder is > 0 and <= ConstParam.NumTankTeam);
            const float halfOrder = (1f + ConstParam.NumTankTeam) / 2;
            float teamRatingDelta = (halfOrder - playerResult.TeamOrder) * ConstParam.RatingDeltaCriterion;
            
            float amplifier = playerResult.FinishedStatus switch
            {
                EBattleFinishedStatus.ErroredAtEarly => 0.2f,
                EBattleFinishedStatus.ErroredAtLastSpurt => 0.8f,
                EBattleFinishedStatus.Completed => 1f,
                _ => throw new ArgumentOutOfRangeException()
            };

            float attenuationOffline = playerResult.IsOnlineBattle ? 1f : 0.2f; 
            
            int ratingDelta = (int)(teamRatingDelta + playerResult.SelfScore);
            
            return (int)(ratingDelta * amplifier * attenuationOffline);
        }
    }
}