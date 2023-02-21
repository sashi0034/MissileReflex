using System;
using MissileReflex.Src.Battle;
using MissileReflex.Src.Connection;
using MissileReflex.Src.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace MissileReflex.Src
{
    public class GameContext : MonoBehaviour
    {
                    
        [SerializeField] private BattleContext battleContext;
        public BattleContext BattleContext => battleContext;

        [SerializeField] private NetworkManager networkManager;
        public NetworkManager Network => networkManager;
        
        
        [EventFunction]
        private void Start()
        {
#if !UNITY_EDITOR
            Application.targetFrameRate = 60;
#endif
        }
    }
}