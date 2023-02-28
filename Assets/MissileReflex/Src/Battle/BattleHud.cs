using MissileReflex.Src.Battle.Hud;
using MissileReflex.Src.Utils;
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
                sectionTeamResult);
            
            labelTankNameManager.Init();
            panelRemainingTime.Init();
            panelCurrTeamInfoManager.Init();
            labelKillOpponentManager.Init();
            labelScoreAdditionOnKillManager.Init();
        }
    }
}