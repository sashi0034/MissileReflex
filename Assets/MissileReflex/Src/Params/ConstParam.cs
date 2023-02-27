﻿using MissileReflex.Src.Battle;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace MissileReflex.Src.Params
{
    [CreateAssetMenu(fileName = nameof(ConstParam), menuName = "ScriptableObjects/Create" + nameof(ConstParam))]
    public class ConstParam : SingletonScriptableObject<ConstParam>
    {
        [SerializeField] private float missilePredictInterval = 0.1f;
        public float MissilePredictInterval => missilePredictInterval;
        
        [SerializeField] private float missilePredictRange = 8f;
        public float MissilePredictRange => missilePredictRange;

        [SerializeField] private Vector3 missileColBoxHalfExt = Vector3.one / 4;
        public Vector3 MissileColBoxHalfExt => missileColBoxHalfExt;

        [SerializeField] private float tankAdjMatUpdateInterval = 0.1f;
        public float TankAdjMatUpdateInterval => tankAdjMatUpdateInterval;

        [SerializeField] private float tankDeathPenaltyTime = 1.5f;
        public float TankDeathPenaltyTime => tankDeathPenaltyTime;
        
        
        [SerializeField] private TankAiAgentParam tankAiAgentParam;
        public TankAiAgentParam TankAiAgentParam => tankAiAgentParam;

        [SerializeField] private float playerDefaultY = 0.5f;
        public float PlayerDefaultY => playerDefaultY;

        [SerializeField] private Material[] matTeamColor;
        public Material[] MatTeamColor => matTeamColor;

        [SerializeField] private Material[] matTeamColorMetal;
        public Material[] MatTeamColorMetal => matTeamColorMetal;

        [SerializeField] private int battleTimeLimit = 180;
        public int BattleTimeLimit => battleTimeLimit;

        [SerializeField] private int battleTimeLastSpurt = 30;
        public int BattleTimeLastSpurt => battleTimeLastSpurt;

        [SerializeField] private int matchingTimeLimit = 60;
        public int MatchingTimeLimit => matchingTimeLimit;
        

        public const string LiteralMainScene = "MainScene";
        public static string GetLiteralArena(int index)
        {
            return "Arena_" + index;
        }

        public const float DeltaMilliF = 1e-3f;

        public const int NumTankTeam = 4;
        public const int MaxTankAgent = NumTankTeam * 4;

        public static readonly Color Transparent = new Color(0, 0, 0, 0);
    }
}