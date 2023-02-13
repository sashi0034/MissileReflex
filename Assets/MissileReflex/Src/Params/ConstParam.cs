using MissileReflex.Src.Battle;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace MissileReflex.Src.Params
{
    [CreateAssetMenu(fileName = nameof(ConstParam), menuName = "ScriptableObjects/Create" + nameof(ConstParam))]
    public class ConstParam : SingletonScriptableObject<ConstParam>
    {
        [SerializeField] private TankAiAgentParam tankAiAgentParam;
        public TankAiAgentParam TankAiAgentParam => tankAiAgentParam;
        
        
        public const float DeltaMilliF = 1e-3f;
        
#if UNITY_EDITOR
        // git管理できるようにするため作成
        // [Button]
        // public void BackupMirrorFile()
        // {
        //     BackupMirrorFile(nameof(ConstParam));
        // }
#endif
    }
}