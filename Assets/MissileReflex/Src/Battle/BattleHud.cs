using MissileReflex.Src.Battle.Hud;
using UnityEngine;

namespace MissileReflex.Src.Battle
{
    public class BattleHud : MonoBehaviour
    {
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
        

        public void Init()
        {
            labelTankNameManager.Init();
            panelRemainingTime.Init();
            panelCurrTeamInfoManager.Init();
            labelKillOpponentManager.Init();
            labelScoreAdditionOnKillManager.Init();
        }
    }
}