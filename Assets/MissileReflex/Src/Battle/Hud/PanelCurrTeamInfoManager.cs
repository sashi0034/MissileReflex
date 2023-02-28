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
        private bool _isLastSpurt = false;

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
            _isLastSpurt = false;
            foreach (var panel in panelCurrTeamInfoList)
            {
                panel.Init();
            }
        }

        public void EnterLastSpurt()
        {
            _isLastSpurt = true;
            foreach (var panel in panelCurrTeamInfoList)
            {
                panel.EnterLastSpurt();
            }
        }

        public void UpdateInfo(BattleTeamScore[] sortedScores)
        {
            if (_isLastSpurt) return;
            
            Debug.Assert(sortedScores.Count() == panelCurrTeamInfoList.Length);
            
            for (var index = 0; index < sortedScores.Length; index++)
            {
                var score = sortedScores[index];
                panelCurrTeamInfoList[score.TeamId].UpdateInfo(this, score, score.Order, index);
            }
        }

        public Sprite[] GetIcons()
        {
            return panelCurrTeamInfoList.Select(panel => panel.ImageIcon.sprite).ToArray();
        }
        
        public Animator[] GetIconAnimators()
        {
            return panelCurrTeamInfoList.Select(panel => panel.Animator).ToArray();
        }
    }
}