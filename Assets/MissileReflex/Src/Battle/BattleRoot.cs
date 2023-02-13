using System;
using UnityEngine;

namespace MissileReflex.Src.Battle
{
    [DisallowMultipleComponent]
    public class BattleRoot : MonoBehaviour
    {
        [SerializeField] private MissileManager missileManager;
        public MissileManager MissileManager => missileManager;

        [SerializeField] private Player player;
        public Player Player => player;

        [SerializeField] private TankManager tankManager;
        public TankManager TankManager => tankManager;
        

        public void Start()
        {
            Init();            
        }

        public void Init()
        {
            missileManager.Init();
            player.Init();
            tankManager.Init();
        }
    }
}