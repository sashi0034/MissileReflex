using System;
using System.Collections.Generic;
using System.Threading;
using Fusion;
using MissileReflex.Src.Params;
using MissileReflex.Src.Utils;
using UnityEngine;

namespace MissileReflex.Src.Battle
{
    [DisallowMultipleComponent]
    public class BattleContext : MonoBehaviour
    {
        private static BattleContext _instance;
        public static BattleContext Instance => _instance;
        
        [SerializeField] private MissileManager missileManager;
        public MissileManager MissileManager => missileManager;

        [SerializeField] private TankManager tankManager;
        public TankManager TankManager => tankManager;

        [SerializeField] private BattleHud hud;
        public BattleHud Hud => hud;

        [SerializeField] private GameContext gameContext;
        public GameContext GameContext => gameContext;
        
        

        private CancellationTokenSource _cancelBattle = new CancellationTokenSource();
        public CancellationToken CancelBattle => _cancelBattle.Token;
        
        [EventFunction]
        private void Awake()
        {
            Debug.Assert(_instance == null);
            _instance = this;
            Init();
        }

        [EventFunction]
        private void Start()
        {
            
#if UNITY_EDITOR
            if (DebugParam.Instance.IsForceBattleOffline)
            {
                Debug.Log("start offline battle");
                gameContext.Network.StartBattle(GameMode.Single).RunTaskHandlingError();
            }
#endif
        }

        public void Init()
        {
            missileManager.Init();
            tankManager.Init();
        }
    }
}