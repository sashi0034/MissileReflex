#nullable enable

using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MissileReflex.Src.Params;
using MissileReflex.Src.Utils;
using Sirenix.Utilities;
using UniRx;
using Unity.VisualScripting;
using UnityEngine;
using Unit = Unity.VisualScripting.Unit;

namespace MissileReflex.Src.Battle.Hud
{
    public class SectionTeamResult : MonoBehaviour
    {
#nullable disable
        [SerializeField] private LabelTeamResultInfo[] labelTeamResultInfos;
        [SerializeField] private ButtonConfirm buttonConfirm;
        [SerializeField] private BattleHud battleHud;
#nullable enable
        public void Init()
        {
            buttonConfirm.Init();
        }

        public async UniTask PerformResult(BattleFinalResult finalResult)
        {
            setupInfo(finalResult);

            foreach (var info in labelTeamResultInfos)
            {
                Util.DeactivateGameObjects(info);
            }

            await HudUtil.AnimBigZeroToOne(transform);
            
            for (int i = labelTeamResultInfos.Length - 1; i >= 0; --i)
            {
                var info = labelTeamResultInfos[i];
                
                await UniTask.Delay((labelTeamResultInfos.Length - i) * 0.3f.ToIntMilli());

                var infoRect = info.GetComponent<RectTransform>();
                const float animDuration = 0.3f;
                HudUtil.AnimBigZeroToOne(info.transform, animDuration).Forget();
                await HudUtil.AnimRectTransformSizeZeroToBeforeY(infoRect, animDuration);
            }

            var anim = DOTween.Sequence()
                .Append(labelTeamResultInfos[0].transform.DOScale(1.1f, 0.5f).SetEase(Ease.OutBack))
                .Append(labelTeamResultInfos[0].transform.DOScale(1.0f, 0.5f).SetEase(Ease.InSine))
                .SetLoops(-1);
            HudUtil.AnimBigZeroToOne(buttonConfirm.transform).Forget();

            // 確認ボタン押すまで待機
            await buttonConfirm.OnConfirmed.Take(1);
            await UniTask.Delay(0.3f.ToIntMilli());
            
            anim.Kill();

            await HudUtil.AnimSmallOneToZero(transform);
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

            var tankScores = finalResult.TankScores;
            tankScores.Sort((a, b) => b.Score.Score - a.Score.Score);
            
            // 上のチームからプレイヤー情報書き換えていく
            labelTeamResultInfos.ForEach(((info, i) =>
            {
                int infoTeam = teamScores[i].TeamId;
                int memberOrder = 0;
                tankScores.ForEach((tankScore, _) =>
                {
                    // 対象チームのプレイヤーのみ見る
                    if (tankScore.Team.TeamId != infoTeam) return;
                    var textScoreAndPlayer = info.TextScoreAndPlayerList[memberOrder];
                    Util.ActivateAndResetScale(textScoreAndPlayer);
                    textScoreAndPlayer.TextFirst.text = tankScore.Score.ToString();
                    textScoreAndPlayer.TextSecond.text = tankScore.TankName;
                    textScoreAndPlayer.TextSecond.color = tankScore.IsLocalPlayer 
                        // ローカルのプレイヤーは色付ける 
                        ? ConstParam.Instance.MatTeamColor[infoTeam].color
                        : Color.white;
                    memberOrder++;
                });
                
                // エラーが起きてないなら、ここで終わるはずである
                if (memberOrder >= info.TextScoreAndPlayerList.Length) return;

                for (int remaining = memberOrder; remaining < info.TextScoreAndPlayerList.Length; ++i)
                {
                    Util.DeactivateGameObjects(info.TextScoreAndPlayerList[memberOrder]);
                }
            }));
        }
        
    }
}