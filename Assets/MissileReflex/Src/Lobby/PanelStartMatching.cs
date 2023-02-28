#nullable enable

using System;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Fusion;
using MissileReflex.Src.Front;
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

        private Subject<LobbySharedState?> _onMatchingFinished = new ();
        public IObservable<LobbySharedState?> OnMatchingFinished => _onMatchingFinished;
        

        [EventFunction]
        private void Start()
        {
            Util.ActivateAndResetScale(button);
            Util.DeactivateGameObjects(message, labelMatchingParticipant);
        }

        [EventFunction]
        public void OnPushButton()
        {
            onPushButtonInternal().RunTaskHandlingError(ex =>
            {
                gameRoot.FrontHud.PopupMessageBelt.PerformPopupCaution(PopupMessageBeltErrorKind.Handle(ex, gameRoot.Network));
                Util.DeactivateGameObjects(message, labelMatchingParticipant);
                HudUtil.AnimBigZeroToOne(button.transform).Forget();
            });
        }

        private async UniTask onPushButtonInternal()
        {
            await HudUtil.AnimSmallOneToZero(button.transform);
            HudUtil.AnimBigZeroToOne(message.transform).Forget();
            message.text = "サーバーに接続しています...";
            
            // 接続開始
            await lobbyHud.GameRoot.Network.StartMatching(GameMode.AutoHostOrClient);

            if (tryGetNetworkRunner(out var runner) == false || runner == null) throw new PopupMessageBeltErrorKind(EPopupMessageBeltKind.HostDisconnected);

            runner.Spawn(lobbySharedStatePrefab, onBeforeSpawned: (_, obj) =>
            {
                obj.GetComponent<LobbySharedState>().Init();
            });
            
            // 共有状態オブジェクトがスポーンされるのを同期
            var sharedState = await syncSpawnLobbySharedState();
            HudUtil.AnimBigZeroToOne(labelMatchingParticipant.transform).Forget();

            // 人が集まるまで待機
            while (true)
            {
                if (runner == null) throw new PopupMessageBeltErrorKind(EPopupMessageBeltKind.HostDisconnected);

                sharedState.DecRemainingCount();
                message.text = $"対戦相手を探しています...\n{sharedState.MatchingRemainingCount}";
                int numParticipants = runner.ActivePlayers.Count();
                labelMatchingParticipant.SetText(numParticipants);
                if (sharedState.MatchingRemainingCount <= 0) break;
                if (numParticipants == ConstParam.MaxTankAgent) break;
                DOTween.Sequence()
                    .Append(message.transform.DOScale(1.05f, 0.5f).SetEase(Ease.OutBack))
                    .Append(message.transform.DOScale(1.0f, 0.5f).SetEase(Ease.InSine));
                await UniTask.Delay(1000);
            }

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