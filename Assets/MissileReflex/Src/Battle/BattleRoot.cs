#nullable enable

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
#nullable disable
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
        public BattleProgressManager Progress => battleProgressManager;

#nullable enable
        
        

        private CancellationTokenSource _cancelBattle = new CancellationTokenSource();
        public CancellationToken CancelBattle => _cancelBattle.Token;

        public void TerminateCancelBattle()
        {
            _cancelBattle.Cancel();
        }

        [EventFunction]
        private void Awake()
        {
            if (Util.EnsureSingleton(this, ref _instance) == false) return;
        }

        [EventFunction]
        private void Start()
        {
            
#if UNITY_EDITOR
            if (DebugParam.Instance.IsForceBattleOffline)
            {
                Debug.Log("start offline battle");
                battleProgressManager.DebugStartBattle(GameMode.Single);
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
                if (GUI.Button(new Rect(0, 0, 200, 40), "Host")) battleProgressManager.DebugStartBattle(GameMode.Host);

                if (GUI.Button(new Rect(0, 40, 200, 40), "Client")) battleProgressManager.DebugStartBattle(GameMode.Client);
                
                if (GUI.Button(new Rect(0, 80, 200, 40), "Shared")) battleProgressManager.DebugStartBattle(GameMode.Shared);
                
                if (GUI.Button(new Rect(0, 120, 200, 40), "AutoHostOrClient")) battleProgressManager.DebugStartBattle(GameMode.AutoHostOrClient);
            }
        }

        public void Init()
        {
            _cancelBattle = new CancellationTokenSource();
            gameObject.SetActive(true);
            missileManager.Init();
            tankManager.Init();
            battleProgressManager.Init();
            hud.Init();
        }
    }
}