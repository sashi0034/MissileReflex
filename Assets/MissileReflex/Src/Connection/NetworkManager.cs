using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using MissileReflex.Src.Battle;
using MissileReflex.Src.Params;
using MissileReflex.Src.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MissileReflex.Src.Connection
{
    public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
    {
        [SerializeField] private BattleRoot battleRoot;

        private readonly BoolFlag _pushedMouseRight = new();
        private readonly BoolFlag _pushedMouseLeft = new();

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if (runner.IsServer)
            {
                battleRoot.TankManager.SpawnPlayer(runner, player, battleRoot.TankManager.GetNextSpawnInfo());
                Debug.Log("connected player: " + player.PlayerId);
            }
            else
            {
                Debug.Log("runner is not server: " + runner);
            }
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
        }

        public void OnSceneLoadStart(NetworkRunner runner)
        {
        }

        private NetworkRunner _runner;

        private async UniTask StartGame(GameMode mode)
        {
            // Create the Fusion runner and let it know that we will be providing user input
            _runner = gameObject.AddComponent<NetworkRunner>();
            _runner.ProvideInput = true;

            // Start or join (depends on gamemode) a session with a specific name
            await _runner.StartGame(new StartGameArgs
            {
                GameMode = mode,
                SessionName = "TestRoom",
                Scene = SceneManager.GetActiveScene().buildIndex,
                SceneManager = gameObject.AddComponent<NetworkSceneManagerBase>()
            });

            for (int i = 0; i < 2 * ConstParam.NumTankTeam; ++i)
                battleRoot.TankManager.SpawnAi(_runner, battleRoot.TankManager.GetNextSpawnInfo());
        }

        private void OnGUI()
        {
            if (_runner == null)
            {
                if (GUI.Button(new Rect(0, 0, 200, 40), "Host")) StartGame(GameMode.Host);

                if (GUI.Button(new Rect(0, 40, 200, 40), "Join")) StartGame(GameMode.Client);
            }
        }

        private void Update()
        {
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