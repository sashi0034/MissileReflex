using System;
using MissileReflex.Src.Battle;
using MissileReflex.Src.Utils;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using RangeInt = MissileReflex.Src.Utils.RangeInt;

namespace MissileReflex.Src.Params
{
    [CreateAssetMenu(fileName = nameof(ConstParam), menuName = "ScriptableObjects/Create" + nameof(ConstParam))]
    public partial class ConstParam : SingletonScriptableObject<ConstParam>
    {
        [SerializeField] private float missilePredictInterval = 0.1f;
        public float MissilePredictInterval => missilePredictInterval;
        
        [SerializeField] private float missilePredictRange = 8f;
        public float MissilePredictRange => missilePredictRange;

        [SerializeField] private Vector3 missileColBoxHalfExt = Vector3.one / 4;
        public Vector3 MissileColBoxHalfExt => missileColBoxHalfExt;
        
        [SerializeField] private float missileOffsetY = 0.8f;
        public float MissileOffsetY => missileOffsetY;

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
        
        [SerializeField] private int battleTimeLastSpurt = 30;
        public int BattleTimeLastSpurt => OverwriteIfDebug(battleTimeLastSpurt, DebugParam.Instance.OwBattleTimeLastSpurt);

        [SerializeField] private int matchingTimeLimit = 60;
        public int MatchingTimeLimit => OverwriteIfDebug(matchingTimeLimit, DebugParam.Instance.OwMatchingTimeLimit);

        [SerializeField] private int battleResultSessionTimeLimit = 10;
        public int BattleResultSessionTimeLimit => battleResultSessionTimeLimit;
        
        

        public const string LiteralMainScene = "MainScene";
        public static string GetLiteralArena(int index)
        {
            return "Arena_" + index;
        }

        public const float DeltaMilliF = 1e-3f;

        public const int NumTankTeam = 4;
        public const int MaxTankAgent = NumTankTeam * 4;

        public const int DefaultPlayerRating = 1000;

        public const int RatingDeltaCriterion = 50;

        public const string SaveDataMainKey = "save_data_main";

        public static readonly Color Transparent = new Color(0, 0, 0, 0);

        public const string ColorCodeGamingGreen = "#76b900";
        public const string ColorOrange = "#F86718";
        public const string ColorBluePale = "#0576B9";

        public static readonly RangeF MatchingSpeedRange = new RangeF(0.5f, 5);
        public const float MatchingSpeedDefault = 2f;

        public static readonly RangeInt BattleTimeLimitRange = new RangeInt(45, 300);
        public const int BattleTimeLimitDefault = 90;

        public static int OverwriteIfDebug(int release, int debug)
        {
            return overwriteIfDebugInternal(release, debug, -1);
        }
        public static float OverwriteIfDebug(float release, float debug)
        {
            return overwriteIfDebugInternal(release, debug, -1);
        }
        private static T overwriteIfDebugInternal<T>(T release, T debug, T invalid) where T : IEquatable<T>
        {
            return
#if !DEBUG
                release;
#else
                debug.Equals(invalid) ? release : debug;
#endif

        }
    }
}