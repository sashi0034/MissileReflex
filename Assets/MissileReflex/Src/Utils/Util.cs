using System;
using DG.Tweening;
using UnityEngine;
using Object = UnityEngine.Object;

namespace System.Runtime.CompilerServices
{
    // recordを使用可能にする
    internal sealed class IsExternalInit{ }
}

namespace MissileReflex.Src.Utils
{
    public static class Util
    {
        public static void DestroyGameObject(GameObject gameObject)
        {
            Object.Destroy(gameObject);
        }
        
        // https://nekojara.city/unity-enum-children
        public static Transform[] GetChildren(this Transform parent)
        {
            // 子オブジェクトを格納する配列作成
            var children = new Transform[parent.childCount];
            var childIndex = 0;

            // 子オブジェクトを順番に配列に格納
            foreach (Transform child in parent)
            {
                children[childIndex++] = child;
            }

            // 子オブジェクトが格納された配列
            return children;
        }
        public static void DestroyGameObjectPossibleInEditor(GameObject gameObject)
        {
#if UNITY_EDITOR
            Object.DestroyImmediate(gameObject);
#else
            Util.DestroyGameObject(gameObject);
#endif
        }
        public static void DestroyComponent(MonoBehaviour component)
        {
            Object.Destroy(component);
        }
        
        public static Tween GetCompletedTween()
        {
            return DOVirtual.DelayedCall(0, () => { }, false);
        }

        public static int ToIntMilli(this float second)
        {
            return (int)((1000) * second);
        }

        public static Color FixAlpha(this Color before, float a)
        {
            var newColor = before;
            newColor.a = a;
            return newColor;
        }

        public static void ResetScaleAndActivate(MonoBehaviour actor)
        {
            actor.transform.localScale = Vector3.one;
            actor.gameObject.SetActive(true);
        }

        public static T DebugTrace<T>(this T target) where T : IFormattable
        {
            Debug.Log(target.ToString());
            return target;
        }
    }
    
    [AttributeUsage(AttributeTargets.Method)]
    public class EventFunctionAttribute : System.Attribute { }
}