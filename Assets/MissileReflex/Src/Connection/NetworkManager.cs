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
    public class NetworkObjectMissingException : Exception
    { }
    
    public class NetworkObjectAlreadyExistException : Exception
    { }
    
    public class NetworkManager : MonoBehaviour
    {
#nullable disable
        [SerializeField] private BattleRoot battleRoot;
        private GameRoot gameRoot => battleRoot.GameRoot;
        [SerializeField] private NetworkLifetimeObject networkLifetimeObjectPrefab;
#nullable enable
        private NetworkLifetimeObject? _lifetimeObject;
        public NetworkRunner? Runner => _lifetimeObject != null ? _lifetimeObject.NetworkRunner : null;


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
            lifetimeObject.OnEndShutdown.Subscribe(reason =>
            {
                // この時点では各NetworkObjectsは生きているはず
                gameRoot.FrontHud.PopupMessageBelt.PerformPopupCautionOnShutdown(reason);
                gameRoot.LobbyHud.SectionMultiChatRef.PostInfoMessageLocal("ホストがルームを解散しました");
                battleRoot.Progress.FinalizeResult();
            });
        }

        public async UniTask StartMatching(GameMode mode)
        {
            Debug.Assert(_lifetimeObject == null);
            if (_lifetimeObject != null) return;
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
        }

        public bool IsRunningNetwork()
        {
            return Runner != null;
        }
    }
}