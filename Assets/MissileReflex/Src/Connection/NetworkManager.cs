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
    public class NetworkManager : MonoBehaviour
    {
#nullable disable
        [SerializeField] private BattleRoot battleRoot;
        [SerializeField] private NetworkLifetimeObject networkLifetimeObjectPrefab;
#nullable enable
        private NetworkLifetimeObject? _lifetimeObject;
        public NetworkRunner? Runner => _lifetimeObject != null ? _lifetimeObject.NetworkRunner : null;
        private ShutdownReason _lastLastShutdownReason = ShutdownReason.Ok;
        public ShutdownReason LastShutdownReason => _lastLastShutdownReason;


        public void ModifyRunner(Action<NetworkRunner> modifier)
        {
            Debug.Assert(Runner != null);
            if (Runner == null) return;
            modifier(Runner);
        }
        
        public async UniTask DebugStartBattleNetwork(GameMode mode)
        {
            _lifetimeObject = Instantiate(networkLifetimeObjectPrefab, transform);
            _lifetimeObject.Init(battleRoot);
            subscribeLifetimeObject(_lifetimeObject);

            var runner = _lifetimeObject.NetworkRunner;
            runner.ProvideInput = true;

            var taskSceneLoad = _lifetimeObject.OnEndSceneLoadDone.Take(1).ToUniTask();
            
            var taskStartGame = runner.StartGame(new StartGameArgs
            {
                GameMode = mode,
                SessionName = "TestRoom",
                PlayerCount = ConstParam.MaxTankAgent,
                Scene = SceneManager.GetActiveScene().buildIndex,
                SceneManager = _lifetimeObject.SceneManager
            });

            await UniTask.WhenAll(taskSceneLoad, taskStartGame.AsUniTask());
        }

        private void subscribeLifetimeObject(NetworkLifetimeObject lifetimeObject)
        {
            lifetimeObject.OnEndShutdown.Subscribe(reason => _lastLastShutdownReason = reason);
        }

        public async UniTask StartMatching(GameMode mode)
        {
            _lifetimeObject = Instantiate(networkLifetimeObjectPrefab, transform);
            _lifetimeObject.Init(battleRoot);
            subscribeLifetimeObject(_lifetimeObject);
            
            var runner = _lifetimeObject.NetworkRunner;
            runner.ProvideInput = false;

            var taskStartGame = runner.StartGame(new StartGameArgs
            {
                GameMode = mode,
                PlayerCount = ConstParam.MaxTankAgent,
                SceneManager = _lifetimeObject.SceneManager
            });
            
            await UniTask.WhenAll(taskStartGame.AsUniTask());
            
            // TODO: 多分これで切断処理
            // _runner.Disconnect(_runner.LocalPlayer);
            // Util.DestroyComponent(gameObject.GetComponent<NetworkSceneManagerBase>());
            // Util.DestroyComponent(_runner);
            // Shutdown()かも?
        }

        public bool IsRunningNetwork()
        {
            return Runner != null;
        }
    }
}