#nullable enable

using System;
using Cysharp.Threading.Tasks;
using MissileReflex.Src.Battle;
using MissileReflex.Src.Connection;
using MissileReflex.Src.Front;
using MissileReflex.Src.Lobby;
using MissileReflex.Src.Params;
using MissileReflex.Src.Storage;
using MissileReflex.Src.Utils;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace MissileReflex.Src
{
    public class GameRoot : MonoBehaviour
    {
#nullable disable
        private static GameRoot _instance;
        public static GameRoot Instance => _instance;
        
        [SerializeField] private BattleRoot battleRoot;
        public BattleRoot BattleRoot => battleRoot;
        public BattleHud BattleHud => battleRoot.Hud;

        [SerializeField] private NetworkManager networkManager;
        public NetworkManager Network => networkManager;

        [SerializeField] private LobbyHud lobbyHud;
        public LobbyHud LobbyHud => lobbyHud;

        [SerializeField] private FrontHud frontHud;
        public FrontHud FrontHud => frontHud;

        [SerializeField] private SaveData saveData = new();
        public SaveData SaveData => saveData;
        
#nullable enable

        private void Awake()
        {
            if (Util.EnsureSingleton(this, ref _instance) == false) return;
            Util.AssertNotNullSerializeFieldsRecursive(this, nameof(MissileReflex),new ());
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
        
        [Button]
        public void ResetSaveData()
        {
            saveData = new SaveData();
        }

        public void ReadSaveData()
        {
            string jsonData = ES3.Load<string>(ConstParam.SaveDataMainKey, defaultValue: "");
            if (jsonData.IsNullOrWhitespace()) return;
            
            Debug.Log("read save data:\n" + jsonData);

            var temp = JsonUtility.FromJson<SaveData>(jsonData);
            Debug.Assert(temp != null);
            if (temp == null) return;
            saveData = temp;
            
            Debug.Log("succeeded read save data");
        }

        public void WriteSaveData()
        {
            string jsonData = JsonUtility.ToJson(saveData);
            ES3.Save(ConstParam.SaveDataMainKey, jsonData);
            
            Logger.Print("write save data:\n" + jsonData);
        }
        
    }
}