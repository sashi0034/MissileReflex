using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MissileReflex.Src.Utils
{
    public class EditorSnapUtil : MonoBehaviour
    {
#if UNITY_EDITOR
        private const string tagTarget = "Target";
        private const string tagSelected = "Selected";
        private const string tagChildren = "Children";

        [FoldoutGroup(tagTarget)] [SerializeField] private Transform[] snappingTargets;
        
        private const float defaultSnappingXZ = 0.5f;
        private const float defaultSnappingRotY = 15f;
        
        [FoldoutGroup(tagTarget)] [Button]
        public void SnapPosXZ(float snapping = defaultSnappingXZ)
        {
            snapPosXZInternal(snappingTargets, snapping);
        }
        
        [FoldoutGroup(tagTarget)] [Button]
        
        public void RandomRotY(float snapping = defaultSnappingRotY)
        {
            randomRotYInternal(snappingTargets, snapping);
        }
        
        [FoldoutGroup(tagSelected)] [Button]
        public void SnapPosXZSelectedObj(float snapping = defaultSnappingXZ)
        {
            snapPosXZInternal(new []{Selection.activeGameObject.transform}, snapping);
        }

        [FoldoutGroup(tagSelected)] [Button]
        public void RandomRotYSelectedObj(float snapping = defaultSnappingRotY)
        {
            randomRotYInternal(new []{Selection.activeGameObject.transform}, snapping);
        }
        
        [FoldoutGroup(tagChildren)] [Button]
        public void SnapPosXZEachChildren(float snapping = defaultSnappingXZ)
        {
            snapPosXZInternal(transform.GetChildren(), snapping);
        }
        
        private void snapPosXZInternal(Transform[] list, float snapping)
        {
            foreach (var child in list)
            {
                if (child == null) continue;
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
        [FoldoutGroup(tagChildren)] public void RandomRotYEachChildren(float snapping = defaultSnappingRotY)
        {
            randomRotYInternal(transform.GetChildren(), snapping);
        }

        private static void randomRotYInternal(Transform[] list, float snapping)
        {
            foreach (var child in list)
            {
                var rotY = snapping * Random.Range(0, (int)(360 / snapping));
                child.transform.rotation = Quaternion.Euler(new Vector3(0, rotY, 0));
            }
        }
#endif
    }
}