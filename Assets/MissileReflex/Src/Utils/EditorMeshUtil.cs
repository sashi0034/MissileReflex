using Sirenix.OdinInspector;
using UnityEngine;

namespace MissileReflex.Src.Utils
{
    public class MeshEditorUtil : MonoBehaviour
    {
#if UNITY_EDITOR
        [Button]
        public void SaveMesh()
        {
            var mesh = GetComponent<MeshFilter>();
            UnityEditor.AssetDatabase.CreateAsset(mesh.sharedMesh, $"Assets/{nameof(MissileReflex)}/StaticResources/{gameObject.name}.asset");
            UnityEditor.AssetDatabase.SaveAssets();
        }
#endif
    }
}