using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Fusion;
using MissileReflex.Src.Params;
using MissileReflex.Src.Utils;
using UnityEngine;

namespace MissileReflex.Src.Battle
{
    public class BattleProgressManager : MonoBehaviour
    {
        [SerializeField] private BattleRoot battleRoot;
        private GameRoot gameRoot => battleRoot.GameRoot;

        private int _remainingTime = 0;

        public void StartBattle(GameMode gameMode)
        {
            startBattleInternal(gameMode).RunTaskHandlingError();
        }

        private async UniTask startBattleInternal(GameMode gameMode)
        {
            battleRoot.Init();

            // ネットワーク準備
            await gameRoot.Network.StartBattleNetwork(gameMode);

            var runner = gameRoot.Network.Runner;
            Debug.Assert(runner != null);
            if (runner == null) return;

            // AI召喚
            int numAi = 2 * ConstParam.NumTankTeam;
            for (int i = 0; i < numAi; ++i)
                battleRoot.TankManager.SpawnAi(runner, battleRoot.TankManager.GetNextSpawnInfo($"AI [{i + 1}]"));

            _remainingTime = ConstParam.Instance.BattleTimeLimit;

            // 制限時間が0になったら試合終了
            await decRemainingTimeUntilZero();

            battleRoot.TerminateCancelBattle();
        }

        private async UniTask decRemainingTimeUntilZero()
        {
            _remainingTime++;
            
            while (_remainingTime > 0)
            {
                _remainingTime--;
                battleRoot.Hud.PanelRemainingTime.UpdateText(_remainingTime);
                await UniTask.Delay(1000);
            }
        }
    }
}