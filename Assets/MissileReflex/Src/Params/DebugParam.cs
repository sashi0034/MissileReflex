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

#if UNITY_EDITOR
        [SerializeField] private bool isForceBattleOffline;
        public bool IsForceBattleOffline => isForceBattleOffline;
        
#endif
    }
}