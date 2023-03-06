#nullable enable

using System;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Fusion;
using MissileReflex.Src.Connection;
using MissileReflex.Src.Front;
using MissileReflex.Src.Lobby.MenuContents;
using MissileReflex.Src.Params;
using MissileReflex.Src.Utils;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;

namespace MissileReflex.Src.Lobby
{
    public class PanelStartMatching : MonoBehaviour
    {
#nullable disable
        [SerializeField] private LobbyHud lobbyHud;
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI message;
        [SerializeField] private LabelMatchingParticipant labelMatchingParticipant;
        [SerializeField] private NetworkPrefabRef lobbySharedStatePrefab;
#nullable enable
        private GameRoot gameRoot => lobbyHud.GameRoot;

        private readonly Subject<LobbySharedState?> _onMatchingFinished = new ();
        public IObservable<LobbySharedState?> OnMatchingFinished => _onMatchingFinished;
        

        
        public void Init()
        {
            if (lobbyHud.SharedState != null) lobbyHud.SharedState.Runner.Shutdown();
            CleanRestart();
        }
        public void CleanRestart()
        {
            Util.ActivateAndResetScale(button);
            Util.DeactivateGameObjects(
                message, 
                labelMatchingParticipant);
            
            // 接続済みの部屋があれば再開
            if (gameRoot.Network.Runner != null && lobbyHud.SharedState != null)
                ResumeSameRoom(gameRoot.Network.Runner, lobbyHud.SharedState);
        }

        private void ResumeSameRoom(NetworkRunner runner, LobbySharedState sharedState)
        {
            // オフラインモードなら終了
            if (runner.GameMode == GameMode.Single)
            {
                runner.Shutdown();
                return;
            }
            
            Util.DeactivateGameObjects(button);
            Util.ActivateAndResetScale(
                message, 
                labelMatchingParticipant);
            
            sharedState.NotifyPlayerInfoFromSaveData(gameRoot.SaveData);
            processAfterConnectSucceeded(runner, sharedState).RunTaskHandlingError(handleMatchingError);
        }

        [EventFunction]
        public void OnPushButton()
        {
            onPushButtonInternal().RunTaskHandlingError(handleMatchingError);
        }

        private void handleMatchingError(Exception ex)
        {
            gameRoot.FrontHud.PopupMessageBelt.PerformPopupCautionFromException(ex);
            Util.DeactivateGameObjects(message, labelMatchingParticipant);
            HudUtil.AnimBigZeroToOne(button.transform).Forget();
        }

        public bool CanPushButton()
        {
            return button.interactable && button.gameObject.activeSelf;
        }

        private async UniTask onPushButtonInternal()
        {
            await disablePushButton();

            HudUtil.AnimBigZeroToOne(message.transform).Forget();
            message.text = "サーバーに接続しています...";
            
            // 接続開始
            var (runner, sharedState) = await startConnectNetwork(GameMode.Shared);
            HudUtil.AnimBigZeroToOne(labelMatchingParticipant.transform).Forget();
            
            lobbyHud.SectionMultiChatRef.PostInfoMessageAuto(sharedState.Runner.ActivePlayers.Count() == 1
                ? $"{gameRoot.SaveData.PlayerName} がルームを作成しました"
                : $"{gameRoot.SaveData.PlayerName} がルームに参加しました");

            // 人が集まるまで待機
            await processAfterConnectSucceeded(runner, sharedState);
        }

        public async UniTask StartOfflineBattle()
        {
            await disablePushButton();
            var (_, sharedState) = await startConnectNetwork(GameMode.Single);
            _onMatchingFinished.OnNext(sharedState);
        }

        private async UniTask<(NetworkRunner, LobbySharedState)> startConnectNetwork(GameMode gameMode)
        {
            if (lobbyHud.SharedState != null) throw new NetworkObjectAlreadyExistException();
            await lobbyHud.GameRoot.Network.StartMatching(gameMode);

            if (tryGetNetworkRunner(out var runner) == false || runner == null) throw new NetworkObjectMissingException();
            
            // 共有状態オブジェクトがスポーンされるのを同期
            var sharedState = await syncSpawnLobbySharedState(runner);
            sharedState.NotifyPlayerInfoFromSaveData(gameRoot.SaveData);
            return (runner, sharedState);
        }

        private async UniTask disablePushButton()
        {
            button.interactable = false;

            SeManager.Instance.PlaySe(SeManager.Instance.SeMatchingStart);
            await HudUtil.AnimSmallOneToZero(button.transform);
            button.interactable = true;
        }

        private async UniTask processAfterConnectSucceeded(NetworkRunner runner, LobbySharedState sharedState)
        {
            while (true)
            {
                if (runner == null) throw new NetworkObjectMissingException();

                float perWaiting = 1f / sharedState.RoomSetting.MatchingSpeed;
                if (updateCheckOtherPlayersJoin(runner, sharedState, perWaiting)) break;
                await UniTask.Delay(perWaiting.ToIntMilli());
            }

            SeManager.Instance.PlaySe(SeManager.Instance.SeMatchingEnd);
            message.text = $"ゲームの準備をしています";
            _onMatchingFinished.OnNext(sharedState);
        }

        private bool updateCheckOtherPlayersJoin(NetworkRunner runner, LobbySharedState sharedState, float perWaiting)
        {
            sharedState.DecRemainingCount();
            int numParticipants = runner.ActivePlayers.Count();
            bool isGathered = sharedState.MatchingRemainingCount <= 0 || numParticipants == ConstParam.MaxTankAgent;
            var battleSharedState = gameRoot.BattleRoot.Progress.SharedState;
            bool isBattleStartedAlready = sharedState.CanJoinBattle == false;

            if (isBattleStartedAlready && 
                battleSharedState!= null && battleSharedState.HasStateAuthority)
            {
                // バトルをしてる人たちが切断したならとりあえずエラーにしておく
                runner.Shutdown();
                throw new NetworkBattleUnfinishedException();
            }
            
            // クリーン前にAuthorityの人が切断するとたまにリセットされないまま進んでしまうかもしれないので一応ここでリセット
            if (sharedState.HasStateAuthority && sharedState.HasEnteredBattle) 
                sharedState.CleanRestart();
            
            message.text = isGathered
                ? isBattleStartedAlready && battleSharedState != null
                    ? $"進行中のゲームがあります... 終了まで\n{battleSharedState.RemainingTime}"
                    : "ホストを待っています"
                // 通常待機
                : $"対戦相手を探しています...\n{sharedState.MatchingRemainingCount}";
            labelMatchingParticipant.SetText(numParticipants);

            if (
                // 既にバトル始まってるならダメ
                isBattleStartedAlready == false &&
                // バトル後ホストがバトルからロビーに戻って来てなかったらダメ
                sharedState.HasEnteredBattle == false &&
                // そろってたら始める
                isGathered
            )
                return true;

            DOTween.Sequence()
                .Append(message.transform.DOScale(1.05f, perWaiting / 2).SetEase(Ease.OutBack))
                .Append(message.transform.DOScale(1.0f, perWaiting / 2).SetEase(Ease.InSine));
            return false;
        }

        private async UniTask<LobbySharedState> syncSpawnLobbySharedState(NetworkRunner runner)
        {
            while (true)
            {
                await UniTask.DelayFrame(1);
                if (lobbyHud.SharedState == null && gameRoot.Network.IsLocalPlayerPseudoHost())
                {
                    runner.Spawn(
                        lobbySharedStatePrefab,
                        inputAuthority: runner.LocalPlayer,
                        onBeforeSpawned: (_, obj) => { obj.GetComponent<LobbySharedState>().Init(); });
                }
                if (lobbyHud.SharedState != null) break;
            }
            
            return lobbyHud.SharedState!;
        }

        private bool tryGetNetworkRunner(out NetworkRunner? runner)
        {
            runner = gameRoot.Network.Runner;
            Debug.Assert(runner != null);
            if (runner != null) return true;
            
            return false;
        }
    }
}