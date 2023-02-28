#nullable enable

using Cysharp.Threading.Tasks;
using MissileReflex.Src.Utils;
using Sirenix.Utilities;
using Unity.VisualScripting;
using UnityEngine;

namespace MissileReflex.Src.Battle.Hud
{
    public class SectionTeamResult : MonoBehaviour
    {
#nullable disable
        [SerializeField] private LabelTeamResultInfo[] labelTeamResultInfos;
        [SerializeField] private ButtonConfirm buttonConfirm;
        [SerializeField] private BattleHud battleHud;
#nullable enable

        public async UniTask PerformResult(BattleFinalResult finalResult)
        {
            setupInfo(finalResult);
            
            await HudUtil.AnimBigZeroToOne(transform);
        }

        public void setupInfo(BattleFinalResult finalResult)
        {
            var teamScores = finalResult.TeamScores;
            var icons = battleHud.PanelCurrTeamInfoManager.GetIcons();
            
            // チーム情報
            labelTeamResultInfos.ForEach((info, i) =>
            {
                info.TextOrder.text = Util.StringifyOrder(teamScores[i].Order);
                info.ImageTeam.sprite = icons[teamScores[i].TeamId];
                info.TextScore.text = teamScores[i].Score.ToString();
            });
        }
        
    }
}