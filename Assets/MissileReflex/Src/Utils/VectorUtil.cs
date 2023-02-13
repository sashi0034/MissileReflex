using System;
using UnityEngine;

namespace MissileReflex.Src.Utils
{
    public static class VectorUtil
    {
        public static Vector3 FixX(this Vector3 before, float x)
        {
            return new Vector3(x, before.y, before.z);
        }
        public static Vector3 FixY(this Vector3 before, float y)
        {
            return new Vector3(before.x, y, before.z);
        }
        public static Vector3 FixZ(this Vector3 before, float z)
        {
            return new Vector3(before.x, before.y, z);
        }

        public static float CalcCos(Vector3 vec1, Vector3 vec2)
        {
            return Vector3.Dot(vec1, vec2) / (vec1.magnitude * vec2.magnitude);
        }
        
        public static void AssertNormalized(Vector2 move)
        {
            const float delta = 1e-4f;
            Debug.Assert(Mathf.Abs(move.SqrMagnitude() - 1) < delta);
        }
    }
}