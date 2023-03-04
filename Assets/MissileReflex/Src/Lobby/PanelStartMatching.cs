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
            var (runner, sharedState) = await startConnectNetwork(GameMode.AutoHostOrClient);
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

            runner.Spawn(lobbySharedStatePrefab, onBeforeSpawned: (_, obj) => { obj.GetComponent<LobbySharedState>().Init(); });

            // 共有状態オブジェクトがスポーンされるのを同期
            var sharedState = await syncSpawnLobbySharedState();
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

                sharedState.DecRemainingCount();
                int numParticipants = runner.ActivePlayers.Count();
                bool isGathered = sharedState.MatchingRemainingCount <= 0 || numParticipants == ConstParam.MaxTankAgent;

                message.text = isGathered
                    ? "ホストを待っています"
                    : $"対戦相手を探しています...\n{sharedState.MatchingRemainingCount}";
                labelMatchingParticipant.SetText(numParticipants);

                // ホストがバトルからロビーに戻って来てなかったらダメ
                // そろってたら始める
                if (sharedState.HasEnteredBattle == false && isGathered) break;
                
                DOTween.Sequence()
                    .Append(message.transform.DOScale(1.05f, 0.5f).SetEase(Ease.OutBack))
                    .Append(message.transform.DOScale(1.0f, 0.5f).SetEase(Ease.InSine));
                await UniTask.Delay(1000);
            }

            SeManager.Instance.PlaySe(SeManager.Instance.SeMatchingEnd);
            message.text = $"ゲームの準備をしています";
            _onMatchingFinished.OnNext(sharedState);
        }

        private async UniTask<LobbySharedState> syncSpawnLobbySharedState()
        {
            await UniTask.WaitUntil(() => lobbyHud.SharedState != null);
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