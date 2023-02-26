#nullable enable

using System;
using MissileReflex.Src.Utils;
using UnityEngine;

namespace MissileReflex.Src.Battle.Hud
{
    public class LabelScoreAdditionOnKillManager : MonoBehaviour
    {
        [SerializeField] private LabelScoreAdditionOnKill labelScoreAdditionOnKillPrefab;

        public void Init()
        {
            Util.DestroyAllChildren(transform);
        }

        public void BirthLabel(LabelScoreAdditionOnKillArg arg)
        {
            var label = Instantiate(labelScoreAdditionOnKillPrefab, transform);
            label.SetupView(arg);
        }
    }
}