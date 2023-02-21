using UnityEngine;

namespace MissileReflex.Src.Params
{
    public class ParamRef : MonoBehaviour
    {
        [SerializeField] private DebugParam debugParam;
        public DebugParam DebugParam => debugParam;

        [SerializeField] private ConstParam constParam;
        public ConstParam ConstParam => constParam;
        
    }
}