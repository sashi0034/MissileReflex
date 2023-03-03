#nullable enable

using System;
using Fusion;
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

        public PlayerRating CalcNewRating(int resultOrder, int numKilled)
        {
            Debug.Assert(resultOrder is > 0 and <= ConstParam.NumTankTeam);
            const float halfOrder = (1f + ConstParam.NumTankTeam) / 2;
            float teamRatingDelta = (halfOrder - resultOrder) * ConstParam.RatingDeltaCriterion;
            int ratingDelta = (int)(teamRatingDelta + numKilled);

            return new PlayerRating(_value + ratingDelta);
        }
    }
}