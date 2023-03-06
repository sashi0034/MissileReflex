#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Fusion;
using Fusion.Photon.Realtime;
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
    
    public class NetworkBattleUnfinishedException : Exception
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

        public PlayerRef PseudoHostRef => _lifetimeObject != null ? _lifetimeObject.PseudoHost : PlayerRef.None;

        public void ModifyRunner(Action<NetworkRunner> modifier)
        {
            if (Runner == null)
            {
                Debug.LogWarning("Runner is null");
                return;
            }
            modifier(Runner);
        }

        public bool IsLocalPlayerPseudoHost()
        {
            return _lifetimeObject != null && 
                   PseudoHostRef == _lifetimeObject.NetworkRunner.LocalPlayer;
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
                SceneManager = _lifetimeObject.SceneManager,
                CustomPhotonAppSettings = getPhotonSetting()
            });

            await UniTask.WhenAll(taskSceneLoad, taskStartGame.AsUniTask());
        }

        private AppSettings getPhotonSetting()
        {
            var photonSettings = Fusion.Photon.Realtime.PhotonAppSettings.Instance.AppSettings.GetCopy();
#if DEBUG
            photonSettings.AppIdFusion = ConstParam.FusionAppIdDebug;
#else
            photonSettings.AppIdFusion = ConstParam.FusionAppIdRelease;
#endif
            return photonSettings;
        }

        private void subscribeLifetimeObject(NetworkLifetimeObject lifetimeObject)
        {
            lifetimeObject.OnEndShutdown.Subscribe(reason =>
            {
                // この時点では各NetworkObjectsは生きているはず
                Debug.Log("shutdown: " + reason);
                if (Runner != null && Runner.GameMode == GameMode.Single) return; 
                
                gameRoot.FrontHud.PopupMessageBelt.PerformPopupCautionOnShutdown(reason);
                gameRoot.LobbyHud.SectionMultiChatRef.PostInfoMessageLocal("ホストがルームを解散しました");
                battleRoot.Progress.FinalizeResult();
            });

            lifetimeObject.OnEndPlayerJoin.Subscribe(player =>
            {
                if (gameRoot.LobbyHud.SharedState != null) gameRoot.LobbyHud.SharedState.CleanPlayer(player);
            });

            lifetimeObject.OnEndPlayerLeft.Subscribe(player =>
            {
                string playerName = gameRoot.LobbyHud.SharedState != null
                    ? gameRoot.LobbyHud.SharedState.GetPlayerStatus(player).Info.Name
                    : "?";
                gameRoot.LobbyHud.SectionMultiChatRef.PostInfoMessageLocal($"{playerName}がルームを抜けました");
                if (gameRoot.LobbyHud.SharedState != null) gameRoot.LobbyHud.SharedState.RemovePlayer(player);
                
                if (gameRoot.SaveData.IsEnteredBattle == false) return;
                if (gameRoot.LobbyHud.SharedState == null) return;
                
                // バトル中でプレイヤーが抜けたなら警告を出す
                gameRoot.FrontHud.PopupMessageBelt.PerformPopupCautionOnPlayerLeft(
                    playerName);
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
                SceneManager = _lifetimeObject.SceneManager,
                CustomPhotonAppSettings = getPhotonSetting()
            });
            
            await UniTask.WhenAll(taskStartGame.AsUniTask());
        }

        public bool IsRunningNetwork()
        {
            return Runner != null;
        }
    }
}