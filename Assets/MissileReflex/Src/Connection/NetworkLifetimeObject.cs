#nullable enable

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using MissileReflex.Src.Battle;
using MissileReflex.Src.Utils;
using UniRx;
using UnityEngine;

namespace MissileReflex.Src.Connection
{
    public class NetworkLifetimeObject : MonoBehaviour, INetworkRunnerCallbacks 
    {
#nullable disable
        [SerializeField] private NetworkRunner networkRunner;
        public NetworkRunner NetworkRunner => networkRunner;

        [SerializeField] private NetworkSceneManagerBase sceneManager;
        public NetworkSceneManagerBase SceneManager => sceneManager;
        private BattleRoot battleRoot;
#nullable enable
        private readonly Subject<Unit> _onEndSceneLoadDone = new Subject<Unit>();
        public Subject<Unit> OnEndSceneLoadDone => _onEndSceneLoadDone;

        private readonly Subject<PlayerRef> _onEndPlayerJoin = new Subject<PlayerRef>();
        public IObservable<PlayerRef> OnEndPlayerJoin => _onEndPlayerJoin;

        private readonly Subject<PlayerRef> _onEndPlayerLeft = new Subject<PlayerRef>();
        public IObservable<PlayerRef> OnEndPlayerLeft => _onEndPlayerLeft;

        private readonly Subject<ShutdownReason> _onEndShutdown = new();
        public IObservable<ShutdownReason> OnEndShutdown => _onEndShutdown;
        

        private readonly BoolFlag _pushedMouseRight = new();
        private readonly BoolFlag _pushedMouseLeft = new();

        // 共有モードで疑似ホストを決定しておく
        private PlayerRef _pseudoHost = PlayerRef.None;
        public PlayerRef PseudoHost => _pseudoHost;

        public void Init(BattleRoot root)
        {
            battleRoot = root;
        }
        
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log("player joined: " + player.PlayerId);
            _onEndPlayerJoin.OnNext(player);
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            Debug.Log("player left: " + player.PlayerId);
            _onEndPlayerLeft.OnNext(player);
        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {}

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            _onEndShutdown.OnNext(shutdownReason);
        }

        public void OnConnectedToServer(NetworkRunner runner)
        {}

        public void OnDisconnectedFromServer(NetworkRunner runner)
        {}

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
        {}

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {}

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
        {}

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {}

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        {}

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
            throw new NotImplementedException();
        }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
        {}

        public void OnSceneLoadDone(NetworkRunner runner)
        {
            _onEndSceneLoadDone.OnNext(Unit.Default);
        }

        public void OnSceneLoadStart(NetworkRunner runner)
        {}
        
        [EventFunction]
        private void Update()
        {
            // 一応念のため疑似ホストを更新しておく
            _pseudoHost = FindPseudoHost(networkRunner.ActivePlayers);
            
            checkPlayerInput();
        }

        private void checkPlayerInput()
        {
            if (networkRunner.ProvideInput == false) return;
            if (Input.GetMouseButtonDown(0)) _pushedMouseLeft.UpFlag();
            if (Input.GetMouseButtonDown(1)) _pushedMouseRight.UpFlag();
        }
        
        public static PlayerRef FindPseudoHost(IEnumerable<PlayerRef>? playerRefs)
        {
            if (playerRefs == null) return PlayerRef.None;
            var pseudoHost = PlayerRef.None;
            foreach (var player in playerRefs)
            {
                if (pseudoHost == PlayerRef.None ||
                    pseudoHost.PlayerId > player.PlayerId)
                {
                    pseudoHost = player;
                }
            }
            return pseudoHost;
        }

        public void OnInput(NetworkRunner _, NetworkInput input)
        {
            var direction = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            
            // ローカルプレイヤーを基準にマウスのワールド座標を求める
            var mouseWorldPos = calcMouseWorldPosFromPlayer();

            var button = new PlayerInputButton((byte)
                (_pushedMouseLeft.PeekFlag() ? PlayerInputButton.BitMouseLeft : 0 |
                    (_pushedMouseRight.PeekFlag() ? PlayerInputButton.BitMouseRight : 0)
                ));

            input.Set(new PlayerInputData(direction, mouseWorldPos, button));
        }

        private Vector3 calcMouseWorldPosFromPlayer()
        {
            var player = battleRoot.TankManager.FindLocalPlayerTank();
            if (player == null) return Vector3.zero;
            var playerPos = player.transform.position;
            var mainCamera = Camera.main;
            if (mainCamera == null) return Vector3.zero;
            var distancePlayerCamera = Vector3.Distance(playerPos, mainCamera.transform.position); 
            var mousePos = Input.mousePosition.FixZ(distancePlayerCamera);
            return mainCamera.ScreenToWorldPoint(mousePos);
        }

    }
}