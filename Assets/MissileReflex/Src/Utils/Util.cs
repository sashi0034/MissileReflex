#nullable enable

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MissileReflex.Src.Params;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

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


        public static bool EnsureSingleton<T>(T awakened, ref T instanceRef) where T : MonoBehaviour
        {
            if (instanceRef != null & instanceRef != awakened)
            {
                Util.DestroyGameObject(awakened.gameObject);
                return false;
            }
            instanceRef = awakened;
            return true;
        }

        public static void DestroyAllChildren(Transform parent)
        {
            foreach (var child in parent.GetChildren())
            {
                Util.DestroyGameObject(child.gameObject);
            }
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

        public static T DebugTrace<T>(this T target) where T : IFormattable
        {
            Debug.Log(target.ToString());
            return target;
        }
        
        public static T TraceAssertNotNull<T>(this T target)
        {
            Debug.Assert(target != null);
            return target;
        }
        
        public static async UniTask DelayDestroyEffect(ParticleSystem effect, CancellationToken cancel)
        {
            await UniTask.WaitUntil(() => effect.isStopped, cancellationToken: cancel);
            Util.DestroyGameObject(effect.gameObject);
        }

        public static void CallDelayedAfterFrame(Action action)
        {
            DOVirtual.DelayedCall(0, () => action());
        }
        
        public static async UniTask ExecutePerFrame(int numFrame, Action action)
        {
            for (int i = 0; i < numFrame; ++i)
            {
                action();
                await UniTask.DelayFrame(0);
            }
        }
        
        public static async UniTask ExecutePerFrameWhileSec(float seconds, Action action)
        {
            while (seconds > 0)
            {
                action();
                await UniTask.DelayFrame(0);
                seconds -= Time.deltaTime;
            }
        }

        public static Action EmptyAction => () => { };
        public static void DoEmpty(){ }

        public static string StringifyOrder(int order)
        {
            int first = order % 10;
            return first switch
            {
                1 => order + "st",
                2 => order + "nd",
                3 => order + "rd",
                _ => order + "th"
            };
        }
        
        public static Color ColourRgb(int r, int g, int b)
        {
            return new Color((float)r / 255.0f, (float)g / 255.0f, (float)b / 255.0f);
        }

        public static Color ColourHex(int hexRgb)
        {
            int r = (hexRgb >> 16) & 0xff;
            int g = (hexRgb >> 8) & 0xff;
            int b = hexRgb & 0xff;
            return ColourRgb(r, g, b);
        }
        
        public static void DeactivateGameObjects(params MonoBehaviour[] objects)
        {
            foreach (var behaviour in objects)
            {
                behaviour.gameObject.SetActive(false);
            }
        }
        public static void ActivateGameObjects(params MonoBehaviour[] objects)
        {
            foreach (var behaviour in objects)
            {
                behaviour.gameObject.SetActive(true);
            }
        }
        public static void ActivateAndResetScale(params MonoBehaviour[] objects)
        {
            foreach (var target in objects)
            {
                target.gameObject.SetActive(true);
                target.transform.localScale = Vector3.one;
            }
        }
        
        public static void ShuffleList<T>(this IList<T> array)  
        {  
            for (var i = array.Count - 1; i > 0; --i)
            {
                var j = Random.Range(0, i + 1);
                (array[i], array[j]) = (array[j], array[i]);
            }
        }

        public static void AssertNotNullSerializeFieldsRecursive(
            MonoBehaviour checking, 
            string projectNameSpace, 
            List<MonoBehaviour> checkedList)
        {
#if UNITY_EDITOR
            bool isRoot = checkedList.Count == 0;
            var fields = checking.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            checkedList.Add(checking);

            foreach (var field in fields)
            {
                if (field.GetCustomAttribute(typeof(SerializeField), true) == null) continue;

                var fieldValue = field.GetValue(checking);
                
                if (fieldValue == null ||
                    TryAccessGameObject(fieldValue) is UnassignedReferenceException)
                {
                    Debug.LogError( 
                        $"<color={ConstParam.ColorCodeGamingGreen}>{field.Name}</color> is null in <color={ConstParam.ColorCodeGamingGreen}>{checking.name}</color>");
                }

                var fieldType = field.FieldType;
                if (fieldType.Namespace == null) continue;
                if (fieldType.Namespace.StartsWith(projectNameSpace) == false) continue;
                if (fieldValue is MonoBehaviour fieldMonoBehaviour && checkedList.Contains(fieldMonoBehaviour) == false)
                    AssertNotNullSerializeFieldsRecursive(fieldMonoBehaviour, projectNameSpace, checkedList);
            } 
            if (isRoot) Debug.Log("done null check: " + checkedList.Count);
#endif
        }

        public static Exception? TryAccessGameObject(object? fieldValue)
        {
            if (fieldValue is not GameObject fieldGameObject) return null;
            try
            {
                _ = fieldGameObject.activeSelf;
            }
            catch (Exception e)
            {
                return e;
            }
            return null;
        }
    }
    
    [AttributeUsage(AttributeTargets.Method)]
    public class EventFunctionAttribute : System.Attribute { }
}