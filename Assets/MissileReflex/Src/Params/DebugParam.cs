using Sirenix.OdinInspector;
using UnityEngine;

namespace MissileReflex.Src.Params
{
    [CreateAssetMenu(fileName = nameof(DebugParam), menuName = "ScriptableObjects/Create" + nameof(DebugParam))]
    public class DebugParam : SingletonScriptableObject<DebugParam>
    {
        private const string tagBuildIn = "BuildIn";
        
        [FoldoutGroup(tagBuildIn)][SerializeField] private bool isClearDebug = false;
        public bool IsClearDebug => isClearDebug;

#if DEBUG
        [SerializeField] private bool isForceBattleOffline;
        public bool IsForceBattleOffline => isForceBattleOffline;

        [SerializeField] private int owBattleTimeLimit = -1;
        public int OwBattleTimeLimit => owBattleTimeLimit;

        [SerializeField] private int owBattleTimeLastSpurt = -1;
        public int OwBattleTimeLastSpurt => owBattleTimeLastSpurt;

        [SerializeField] private int owMatchingTimeLimit = -1;
        public int OwMatchingTimeLimit => owMatchingTimeLimit;
        
                
        
#endif
    }
}