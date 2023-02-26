#nullable enable

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MissileReflex.Src.Params;
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

        private readonly Vector3[] panelLocalPosList = new Vector3[ConstParam.NumTankTeam];
        public IReadOnlyList<Vector3> PanelLocalPosList => panelLocalPosList;  

        public void Start()
        {
            Debug.Assert(panelLocalPosList.Length == panelCurrTeamInfoList.Length);
            for (int i = 0; i < panelCurrTeamInfoList.Length; ++i)
            {
                panelLocalPosList[i] = panelCurrTeamInfoList[i].transform.localPosition;
            }
        }

        public void Init()
        {
            foreach (var panel in panelCurrTeamInfoList)
            {
                panel.Init();
            }
        }

        public void UpdateInfo(BattleTeamStateWithId[] stateList)
        {
            Debug.Assert(stateList.Count() == panelCurrTeamInfoList.Length);
            stateList.Sort((a, b) => b.State.Score - a.State.Score);

            int checkingScore = -1;
            int checkingOrder = 0;
            for (var index = 0; index < stateList.Length; index++)
            {
                var state = stateList[index];
                var (teamId, teamState) = state;

                if (checkingScore != (checkingScore = teamState.Score)) checkingOrder++;

                panelCurrTeamInfoList[teamId].UpdateInfo(this, teamState, checkingOrder, index);
            }
        }
    }
}