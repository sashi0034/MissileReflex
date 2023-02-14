using System;
using System.Threading;
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

        private CancellationTokenSource _cancelBattle = new CancellationTokenSource();
        public CancellationToken CancelBattle => _cancelBattle.Token;
        

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