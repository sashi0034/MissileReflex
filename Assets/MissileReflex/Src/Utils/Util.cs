﻿#nullable enable

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
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
                await UniTask.DelayFrame(1);
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

    }
    
    [AttributeUsage(AttributeTargets.Method)]
    public class EventFunctionAttribute : System.Attribute { }
}