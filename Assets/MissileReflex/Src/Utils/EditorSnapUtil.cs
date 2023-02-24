using Sirenix.OdinInspector;
using UnityEngine;

namespace MissileReflex.Src.Utils
{
    public class EditorSnapUtil : MonoBehaviour
    {
        [Button]
        public void SnapPosXZEachChildren(float snapping = 0.5f)
        {
            foreach (var child in transform.GetChildren())
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
        public void RandomRotYEachChildren(float snapping = 15)
        {
            foreach (var child in transform.GetChildren())
            {
                var rotY = snapping * Random.Range(0, (int)(360 / snapping));
                child.transform.rotation = Quaternion.Euler(new Vector3(0, rotY, 0));
            }
        }
    }
}