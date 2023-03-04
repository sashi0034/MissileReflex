using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace MissileReflex.Src.Params
{
    [CreateAssetMenu(fileName = nameof(DebugParam), menuName = "ScriptableObjects/Create" + nameof(DebugParam))]
    public class DebugParam : SingletonScriptableObject<DebugParam>
    {
        private const string tagBuildIn = "BuildIn";
        
        [FoldoutGroup(tagBuildIn)][SerializeField] private bool isClearDebug = false;
        public bool IsClearDebug => isClearDebug;

        [SerializeField] private int owBattleTimeLimit = -1;
        public int OwBattleTimeLimit => owBattleTimeLimit;

        [SerializeField] private int owBattleTimeLastSpurt = -1;
        public int OwBattleTimeLastSpurt => owBattleTimeLastSpurt;

        [SerializeField] private int owMatchingTimeLimit = -1;
        public int OwMatchingTimeLimit => owMatchingTimeLimit;
#if DEBUG
        [SerializeField] private bool isForceOfflineBattle;
        public bool IsForceOfflineBattle => isForceOfflineBattle;

        [SerializeField] private bool isGuiDebugStartBattle = false;
        public bool IsGuiDebugStartBattle => isGuiDebugStartBattle;
#endif
    }
}