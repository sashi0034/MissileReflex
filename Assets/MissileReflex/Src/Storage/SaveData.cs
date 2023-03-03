#nullable enable

using System;
using MissileReflex.Src.Params;
using UnityEngine;

namespace MissileReflex.Src.Storage
{
    [Serializable]
    public class SaveData
    {
        [SerializeField] private PlayerRating playerRating = new(ConstParam.DefaultPlayerRating);
        public PlayerRating PlayerRating => playerRating;

        private static readonly string defaultPlayerName = $"Player_{Guid.NewGuid().ToString()[..4]}"; 
        [SerializeField] private string playerName = defaultPlayerName;
        public string PlayerName => playerName;
        
        // 通信切断処理に実装
        [SerializeField] private bool isEnteredBattle = false;
        public bool IsEnteredBattle => isEnteredBattle;


        public void SetPlayerRating(PlayerRating rating)
        {
            playerRating = rating;
        }
        
        public void SetPlayerName(string name)
        {
            playerName = name;
        }
    }
}