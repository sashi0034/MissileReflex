#nullable enable

using System.Collections.Generic;
using System.Linq;
using MissileReflex.Src.Utils;
using Sirenix.Utilities;
using UnityEngine;

namespace MissileReflex.Src.Battle.Hud
{
    public class PanelCurrTeamInfoManager : MonoBehaviour
    {
#nullable disable
        [SerializeField] private PanelCurrTeamInfo[] panelCurrTeamInfoList;
#nullable enable

        public void Init()
        {
            
        }

        public void UpdateInfo(BattleTeamStateWithId[] stateList)
        {
            Debug.Assert(stateList.Count() == panelCurrTeamInfoList.Length);
            stateList.Sort((a, b) => b.State.Score - a.State.Score);

            int checkingScore = -1;
            int checkingOrder = 0;
            for (var index = 0; index < stateList.Length; index++)
            {
                var (teamId, teamState) = stateList[index];

                if (checkingScore != (checkingScore = teamState.Score)) checkingOrder++;

                panelCurrTeamInfoList[teamId].TextScore.text = teamState.Score.ToString();
                panelCurrTeamInfoList[teamId].TextOrder.text = Util.StringifyOrder(checkingOrder);
            }
        }
    }
}