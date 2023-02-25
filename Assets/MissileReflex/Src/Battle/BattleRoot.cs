using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Fusion;
using MissileReflex.Src.Params;
using MissileReflex.Src.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        [SerializeField] private BattleProgressManager battleProgressManager;
        public BattleProgressManager BattleProgressManager => battleProgressManager;
        
        
        

        private CancellationTokenSource _cancelBattle = new CancellationTokenSource();
        public CancellationToken CancelBattle => _cancelBattle.Token;

        public void TerminateCancelBattle()
        {
            _cancelBattle.Cancel();
        }

        [EventFunction]
        private void Awake()
        {
            Debug.Assert(_instance == null);
            _instance = this;
        }

        [EventFunction]
        private void Start()
        {
            
#if UNITY_EDITOR
            if (DebugParam.Instance.IsForceBattleOffline)
            {
                Debug.Log("start offline battle");
                battleProgressManager.StartBattle(GameMode.Single);
            }
#endif
        }

        private void OnGUI()
        {
            if (
#if UNITY_EDITOR
                DebugParam.Instance.IsForceBattleOffline == false &&
#endif
                gameRoot.Network.IsRunningNetwork() == false)
            {
                if (GUI.Button(new Rect(0, 0, 200, 40), "Host")) battleProgressManager.StartBattle(GameMode.Host);

                if (GUI.Button(new Rect(0, 40, 200, 40), "Join")) battleProgressManager.StartBattle(GameMode.Client);
            }
        }

        public void Init()
        {
            missileManager.Init();
            tankManager.Init();
            hud.Init();
        }
    }
}