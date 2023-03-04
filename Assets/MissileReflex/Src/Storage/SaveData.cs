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

        [SerializeField] private string playerName = getDefaultPlayerName();
        public string PlayerName => playerName;
        
        // 通信切断処理に実装
        [SerializeField] private bool isEnteredBattle = false;
        public bool IsEnteredBattle => isEnteredBattle;


        private static string getDefaultPlayerName()
        {
            return $"Player_{Guid.NewGuid().ToString()[..4]}";
        }
        
        public void SetPlayerRating(PlayerRating rating)
        {
            playerRating = rating;
        }
        
        public void SetPlayerName(string name)
        {
            playerName = name;
        }

        public void SetEnteredBattle(bool flag)
        {
            isEnteredBattle = flag;
        }
    }
}