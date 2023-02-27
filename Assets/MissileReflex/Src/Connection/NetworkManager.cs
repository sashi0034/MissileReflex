#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using MissileReflex.Src.Battle;
using MissileReflex.Src.Params;
using MissileReflex.Src.Utils;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace MissileReflex.Src.Connection
{
    public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
    {
#nullable disable
        [SerializeField] private BattleRoot battleRoot;
#nullable enable

        private Subject<Unit> _onEndSceneLoadDone = new Subject<Unit>();
        public Subject<Unit> OnEndSceneLoadDone => _onEndSceneLoadDone;
        
        private NetworkRunner? _runner;
        public NetworkRunner? Runner => _runner;

        private readonly BoolFlag _pushedMouseRight = new();
        private readonly BoolFlag _pushedMouseLeft = new();

        public void ModifyRunner(Action<NetworkRunner> modifier)
        {
            Debug.Assert(_runner != null);
            if (_runner == null) return;
            modifier(_runner);
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if (runner.IsServer)
                Debug.Log("connected player: " + player.PlayerId);
            else
                Debug.Log("runner is not server: " + runner);
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            // Find and remove the players avatar
            Debug.Log("player leaves: " + player);
        }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
        {
        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
        }

        public void OnConnectedToServer(NetworkRunner runner)
        {
        }

        public void OnDisconnectedFromServer(NetworkRunner runner)
        {
        }

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
        {
        }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
        {
        }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
        {
        }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
        }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        {
        }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
            throw new NotImplementedException();
        }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
        {
        }

        public void OnSceneLoadDone(NetworkRunner runner)
        {
            _onEndSceneLoadDone.OnNext(Unit.Default);
        }

        public void OnSceneLoadStart(NetworkRunner runner)
        {
        }

        public async UniTask DebugStartBattleNetwork(GameMode mode)
        {
            // Create the Fusion runner and let it know that we will be providing user input
            Debug.Assert(TryGetComponent<NetworkRunner>(out _) == false);
            _runner = gameObject.AddComponent<NetworkRunner>();
            _runner.ProvideInput = true;

            var taskSceneLoad = _onEndSceneLoadDone.Take(1).ToUniTask();
            
            // Start or join (depends on gamemode) a session with a specific name
            var taskStartGame = _runner.StartGame(new StartGameArgs
            {
                GameMode = mode,
                SessionName = "TestRoom",
                PlayerCount = ConstParam.MaxTankAgent,
                Scene = SceneManager.GetActiveScene().buildIndex,
                SceneManager = gameObject.AddComponent<NetworkSceneManagerBase>()
            });

            await UniTask.WhenAll(taskSceneLoad, taskStartGame.AsUniTask());
        }
        
        public async UniTask StartMatching(GameMode mode)
        {
            // Create the Fusion runner and let it know that we will be providing user input
            Debug.Assert(TryGetComponent<NetworkRunner>(out _) == false);
            _runner = gameObject.AddComponent<NetworkRunner>();
            _runner.ProvideInput = false;
            
            // TODO: runnerを本当にAddComponentする必要があるか確認 (Serializeでいける?)
            
            // Start or join (depends on gamemode) a session with a specific name
            var taskStartGame = _runner.StartGame(new StartGameArgs
            {
                GameMode = mode,
                PlayerCount = ConstParam.MaxTankAgent,
                SceneManager = gameObject.AddComponent<NetworkSceneManagerBase>()
            });
            
            await UniTask.WhenAll(taskStartGame.AsUniTask());
            
            // TODO: 多分これで切断処理
            // _runner.Disconnect(_runner.LocalPlayer);
            // Util.DestroyComponent(gameObject.GetComponent<NetworkSceneManagerBase>());
            // Util.DestroyComponent(_runner);
        }

        public bool IsRunningNetwork()
        {
            return _runner != null;
        }

        private void Update()
        {
            if (_runner == null || _runner.ProvideInput == false) return;
            if (Input.GetMouseButtonDown(0)) _pushedMouseLeft.UpFlag();
            if (Input.GetMouseButtonDown(1)) _pushedMouseRight.UpFlag();
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