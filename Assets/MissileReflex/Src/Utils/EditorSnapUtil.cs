using Sirenix.OdinInspector;
using UnityEngine;

namespace MissileReflex.Src.Utils
{
    public class EditorSnapUtil : MonoBehaviour
    {
        private const string tagTarget = "Target";
        private const string tagChildren = "Children";

        [FoldoutGroup(tagTarget)] [SerializeField] private Transform[] snappingTargets;
        
        [FoldoutGroup(tagTarget)] [Button]
        
        public void SnapPosXZ(float snapping = 0.5f)
        {
            snapPosXZInternal(snappingTargets, snapping);
        }
        
        [FoldoutGroup(tagTarget)] [Button]
        
        public void RandomRotY(float snapping = 15)
        {
            randamRotYInternal(snappingTargets, snapping);
        }
        
        
        [FoldoutGroup(tagChildren)] [Button]
        public void SnapPosXZEachChildren(float snapping = 0.5f)
        {
            snapPosXZInternal(transform.GetChildren(), snapping);
        }
        
        private void snapPosXZInternal(Transform[] list, float snapping)
        {
            foreach (var child in list)
            {
                var currPos = child.transform.position;
                var newPos = new Vector3(
                    snapValue(currPos.x, snapping),
                    currPos.y,
                    snapValue(currPos.z, snapping));
                child.transform.position = newPos;
            }
        }

        private static float snapValue(float value, float snapping)
        {
            return Mathf.Floor((value + snapping / 2) / snapping) * snapping;
        }

        [Button]
        [FoldoutGroup(tagChildren)] public void RandomRotYEachChildren(float snapping = 15)
        {
            randamRotYInternal(transform.GetChildren(), snapping);
        }

        private void randamRotYInternal(Transform[] list, float snapping)
        {
            foreach (var child in list)
            {
                var rotY = snapping * Random.Range(0, (int)(360 / snapping));
                child.transform.rotation = Quaternion.Euler(new Vector3(0, rotY, 0));
            }
        }
    }
}