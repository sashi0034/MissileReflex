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
    public class BattleRoot : MonoBehaviour
    {
        private static BattleRoot _instance;
        public static BattleRoot Instance => _instance;
        
        [SerializeField] private MissileManager missileManager;
        public MissileManager MissileManager => missileManager;

        [SerializeField] private TankManager tankManager;
        public TankManager TankManager => tankManager;

        [SerializeField] private BattleHud hud;
        public BattleHud Hud => hud;

        [SerializeField] private GameRoot gameRoot;
        public GameRoot GameRoot => gameRoot;
        
        

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
                gameRoot.Network.StartBattle(GameMode.Single).RunTaskHandlingError();
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