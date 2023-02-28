using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MissileReflex.Src.Utils
{
    public static class UniTaskUtil
    {
        public static async UniTask RunTaskHandlingErrorAsync(this UniTask task, Action<Exception> onError) {
            try {
                await task;
            } catch(Exception e)
            {
                LogTaskHandlingError(e);
                onError(e);
            }
        }
    
        public static async UniTask RunTaskHandlingErrorAsync(this UniTask task) {
            try {
                await task;
            } catch(Exception e)
            {
                LogTaskHandlingError(e);
            }
        }

        public static void LogTaskHandlingError(Exception e)
        {
            if (e is OperationCanceledException)
                Debug.LogWarning($"{e.Message}\n{e.StackTrace.colorizeStackTrance()}");
            else
                Debug.LogError($"{e.Message}\n{e.StackTrace.colorizeStackTrance()}");
        }

        private static string colorizeStackTrance(this string message)
        {
            return
#if UNITY_EDITOR
                // atとinの間に色を付ける
                message
                    .Replace(" at ", "</color> at ")
                    .replaceFirst("</color> at ", " at ")
                    .Replace(" in ", " in<color=#76b900> ") + "</color>";
#else
                message;
#endif
        }

        private static string replaceFirst(
            this string self,
            string oldValue,
            string newValue)
        {
            var startIndex = self.IndexOf(oldValue, StringComparison.Ordinal);

            if (startIndex == -1) return self;

            return self
                .Remove(startIndex, oldValue.Length)
                .Insert(startIndex, newValue);
        }

        public static void RunTaskHandlingError(this UniTask task) {
            RunTaskHandlingErrorAsync(task).Forget();
        }
    
        public static void RunTaskHandlingError(this UniTask task, Action<Exception> onError) {
            RunTaskHandlingErrorAsync(task, onError).Forget();
        }
    }
}