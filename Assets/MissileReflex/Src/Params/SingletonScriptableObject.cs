using UnityEditor;
using UnityEngine;

namespace MissileReflex.Src.Params
{
    public abstract class SingletonScriptableObject<T> : ScriptableObject where T : SingletonScriptableObject<T>
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load(typeof(T).Name) as T;
                }

                return _instance;
            }
        }

        // protected void BackupMirrorFile(string assetName)
        // {
        //     AssetDatabase.CopyAsset(
        //         $"Assets/Resources/{assetName}.asset", 
        //         $"Assets/{nameof(MissileReflex)}/ScriptableObjectMirror/{assetName}");
        // }
    }
}