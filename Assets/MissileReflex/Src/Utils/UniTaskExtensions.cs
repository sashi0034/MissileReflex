using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MissileReflex.Src.Utils
{
    public static class UniTaskExtensions
    {
        public static async UniTask RunTaskHandlingErrorAsync(this UniTask task, Action<Exception> onError) {
            try {
                await task;
            } catch(Exception e)
            {
                onError(e);
                Debug.LogError(e.Message);
            }
        }
    
        public static async UniTask RunTaskHandlingErrorAsync(this UniTask task) {
            try {
                await task;
            } catch(Exception e) {
                Debug.LogError(e.Message);
            }
        }
        public static void RunTaskHandlingError(this UniTask task) {
            RunTaskHandlingErrorAsync(task).Forget();
        }
    
        public static void RunTaskHandlingError(this UniTask task, Action<Exception> onError) {
            RunTaskHandlingErrorAsync(task, onError).Forget();
        }
    }
}