#nullable enable

using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Fusion;
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

        private Subject<LobbySharedState> _onMatchingFinished = new Subject<LobbySharedState>();
        public IObservable<LobbySharedState> OnMatchingFinished => _onMatchingFinished;
        

        [EventFunction]
        private void Start()
        {
            Util.ActivateAndResetScale(button);
            Util.DeactivateGameObjects(message, labelMatchingParticipant);
        }

        [EventFunction]
        public void OnPushButton()
        {
            onPushButtonInternal().RunTaskHandlingError();
        }

        private async UniTask onPushButtonInternal()
        {
            await HudUtil.AnimSmallOneToZero(button.transform);
            HudUtil.AnimBigZeroToOne(message.transform).Forget();
            message.text = "サーバーに接続しています...";
            
            // 接続開始
            await lobbyHud.GameRoot.Network.StartMatching(GameMode.AutoHostOrClient);

            if (tryGetNetworkRunner(out var runner) == false || runner == null) return;

            runner.Spawn(lobbySharedStatePrefab, onBeforeSpawned: (_, obj) =>
            {
                obj.GetComponent<LobbySharedState>().Init();
            });
            
            // 共有状態オブジェクトがスポーンされるのを同期
            var sharedState = await syncSpawnLobbySharedState();
            HudUtil.AnimBigZeroToOne(labelMatchingParticipant.transform).Forget();

            while (true)
            {
                sharedState.DecRemainingCount();
                message.text = $"対戦相手を探しています...\n{sharedState.MatchingRemainingCount}";
                labelMatchingParticipant.SetText(runner.SessionInfo.PlayerCount);
                if (sharedState.MatchingRemainingCount <= 0) break;
                if (runner.SessionInfo.PlayerCount == ConstParam.MaxTankAgent) break;
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
            
            Util.DeactivateGameObjects(message, labelMatchingParticipant);
            Util.ActivateAndResetScale(button);
            return false;

        }
    }
}