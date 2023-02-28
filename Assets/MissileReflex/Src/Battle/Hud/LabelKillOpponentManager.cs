#nullable enable

using MissileReflex.Src.Params;
using MissileReflex.Src.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace MissileReflex.Src.Battle.Hud
{
    public class LabelKillOpponentManager : MonoBehaviour
    {
#nullable disable
        [SerializeField] private BattleRoot battleRoot;
        [SerializeField] private LabelKillOpponent labelKillOpponentPrefab;
        [SerializeField] private VerticalLayoutGroup verticalLayoutGroup;
#nullable enable

        public void Start()
        { }

        public void Init()
        {
            Util.DestroyAllChildren(verticalLayoutGroup.transform);
        }

        public void AppendLabel(TankFighter attackedPlayer, TankFighter killed)
        {
            string message = 
                attackedPlayer == killed
                    ? "<color=#ccd>自分の攻撃でやられた</color>"
                    : killed.IsOwnerLocalPlayer() 
                        ? $"<color=#{getTeamColor(attackedPlayer)}>{attackedPlayer.TankName}</color><color=#ccd>にやられた</color>"
                        :attackedPlayer.Team.IsSame(killed.Team)
                            ? $"<color=#{getTeamColor(killed)}>{killed.TankName}</color>を倒してしまった..."
                            : $"<color=#{getTeamColor(killed)}>{killed.TankName}</color>を倒した";

            var newLabel = Instantiate(labelKillOpponentPrefab, verticalLayoutGroup.transform);
            newLabel.PerformMessage(message, battleRoot.CancelBattle).RunTaskHandlingError();
        }

        private static string getTeamColor(TankFighter killed)
        {
            const float brightness = 1.3f;
            return ColorUtility.ToHtmlStringRGB(ConstParam.Instance.MatTeamColor[killed.Team.TeamId].color * brightness);
        }
    }
}