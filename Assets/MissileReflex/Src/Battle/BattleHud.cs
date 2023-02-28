using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using MissileReflex.Src.Battle.Hud;
using MissileReflex.Src.Utils;
using TMPro;
using UnityEngine;

namespace MissileReflex.Src.Battle
{
    public class BattleHud : MonoBehaviour
    {
#nullable disable
        [SerializeField] private LabelTankNameManager labelTankNameManager;
        public LabelTankNameManager LabelTankNameManager => labelTankNameManager;

        [SerializeField] private PanelRemainingTime panelRemainingTime;
        public PanelRemainingTime PanelRemainingTime => panelRemainingTime;

        [SerializeField] private PanelCurrTeamInfoManager panelCurrTeamInfoManager;
        public PanelCurrTeamInfoManager PanelCurrTeamInfoManager => panelCurrTeamInfoManager;

        [SerializeField] private LabelKillOpponentManager labelKillOpponentManager;
        public LabelKillOpponentManager LabelKillOpponentManager => labelKillOpponentManager;

        [SerializeField] private LabelScoreAdditionOnKillManager labelScoreAdditionOnKillManager;
        public LabelScoreAdditionOnKillManager LabelScoreAdditionOnKillManager => labelScoreAdditionOnKillManager;

        [SerializeField] private SectionTeamResult sectionTeamResult;
        public SectionTeamResult SectionTeamResult => sectionTeamResult;

        [SerializeField] private TextMeshProUGUI labelBattleFinish;
        public TextMeshProUGUI LabelBattleFinish => labelBattleFinish;

        [SerializeField] private TextMeshProUGUI labelBattleStart;
        public TextMeshProUGUI LabelBattleStart => labelBattleStart;
        
#nullable enable

        public void Init()
        {
            Util.ActivateGameObjects(
                this,
                labelTankNameManager,
                panelRemainingTime,
                panelCurrTeamInfoManager,
                labelKillOpponentManager,
                labelScoreAdditionOnKillManager);
            Util.DeactivateGameObjects(
                sectionTeamResult,
                labelBattleFinish,
                labelBattleStart);
            
            labelTankNameManager.Init();
            panelRemainingTime.Init();
            panelCurrTeamInfoManager.Init();
            labelKillOpponentManager.Init();
            labelScoreAdditionOnKillManager.Init();
        }

        public IEnumerable<MonoBehaviour> ListHudOnPlaying()
        {
            return new MonoBehaviour[]
            {
                panelRemainingTime,
                panelCurrTeamInfoManager
            };
        }

        public async UniTask PerformLabelBattleStart()
        {
            await performAppearCenterLabel(labelBattleStart.transform);
        }
        public async UniTask PerformLabelBattleFinish()
        {
            await performAppearCenterLabel(labelBattleFinish.transform);
        }
        private static async UniTask performAppearCenterLabel(Transform label)
        {
            await HudUtil.AnimBigZeroToOne(label, 1f);
            await UniTask.Delay(2500);
            await HudUtil.AnimSmallOneToZero(label);
        }
    }
}