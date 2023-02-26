#nullable enable

using System;
using Cysharp.Threading.Tasks;
using MissileReflex.Src.Battle;
using MissileReflex.Src.Connection;
using MissileReflex.Src.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace MissileReflex.Src
{
    public class GameRoot : MonoBehaviour
    {
#nullable disable
        private static GameRoot _instance;
        
        [SerializeField] private BattleRoot battleRoot;
        public BattleRoot BattleRoot => battleRoot;

        [SerializeField] private NetworkManager networkManager;
        public NetworkManager Network => networkManager;
#nullable enable

        private void Awake()
        {
            if (Util.EnsureSingleton(this, ref _instance) == false) return;
        }
        
        [EventFunction]
        private void Start()
        {
#if !UNITY_EDITOR
            Application.targetFrameRate = 60;
#endif
        }

        public async UniTask LoadScene(string sceneName)
        {
            Debug.Log("start load scene: " + sceneName);
            DontDestroyOnLoad(gameObject);
            SceneManager.LoadScene(sceneName);
            
            await UniTask.DelayFrame(1);
            
            SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetActiveScene());
            Debug.Log("finish load scene: " + sceneName);
        }
    }
}