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

        public bool IsSleeping => gameObject.activeSelf == false; 
        
        

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
            if (DebugParam.Instance.IsForceOfflineBattle)
            {
                Debug.Log("start offline battle");
                battleProgressManager.DebugStartBattle(GameMode.Single);
            }
#endif
        }

        private void OnGUI()
        {
#if DEBUG
            if (DebugParam.Instance.IsGuiDebugStartBattle == false) return;
            if (DebugParam.Instance.IsForceOfflineBattle || gameRoot.Network.IsRunningNetwork()) return;

            const int w = 200;
            const int h = 40;
            if (GUI.Button(new Rect(0, h * 0, w, h), "Host")) battleProgressManager.DebugStartBattle(GameMode.Host);
            if (GUI.Button(new Rect(0, h * 1, w, h), "Client")) battleProgressManager.DebugStartBattle(GameMode.Client);
            if (GUI.Button(new Rect(0, h * 2, w, h), "Shared")) battleProgressManager.DebugStartBattle(GameMode.Shared);
            if (GUI.Button(new Rect(0, h * 3, w, h), "AutoHostOrClient")) battleProgressManager.DebugStartBattle(GameMode.AutoHostOrClient);
#endif
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

        public void ClearBattle()
        {
            missileManager.ClearBattle();
            tankManager.ClearBattle();
            battleProgressManager.ClearBattle();
        }
    }
}