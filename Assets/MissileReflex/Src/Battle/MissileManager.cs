using System;
using System.Collections.Generic;
using Fusion;
using MissileReflex.Src.Battle;
using MissileReflex.Src.Params;
using MissileReflex.Src.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace MissileReflex.Src.Battle
{
    [DisallowMultipleComponent]
    public class MissileManager : MonoBehaviour
    {
        [SerializeField] private BattleRoot battleRoot;
        
        [SerializeField] private NetworkPrefabRef[] missilePrefab;

        [SerializeField] private float missileOffsetY = 0.5f;

        private readonly List<Missile> _missileList = new List<Missile>();
        private IntervalProcess _intervalProcess = new();

        public void Init()
        {
            foreach (var missile in _missileList)
            {
                Util.DestroyGameObject(missile.gameObject);
            }
            _missileList.Clear();
            _intervalProcess = new IntervalProcess(predictMissileHit, ConstParam.Instance.MissilePredictInterval);
        }

        public void ShootMissile(MissileInitArg arg, NetworkRunner runner)
        {
            var team = arg.Attacker.Team;
            var attackerPlayer = arg.Attacker.OwnerNetworkPlayer;
            runner.Spawn(
                missilePrefab[team.TeamId], arg.Attacker.transform.position, Quaternion.identity, attackerPlayer,
                onBeforeSpawned: (networkRunner, obj) =>
            {
                var missile = obj.GetComponent<Missile>();
                _missileList.Add(missile);

                var fixedArg = arg with { InitialPos = arg.InitialPos.FixY(missileOffsetY) };
                missile.Init(fixedArg);
            });
        }

        [EventFunction]
        private void Update()
        {
            _intervalProcess.Update(Time.deltaTime);
        }

        private void predictMissileHit()
        {
            // 前回の各通知をクリア
            // foreach (var tank in battleRoot.TankManager.List)
            // {
            //     tank.Prediction.ClearMissileHits();
            // }
            
            // ミサイルがタンクとヒットしそうかチェックする
            for (var i = _missileList.Count - 1; i >= 0; i--)
            {
                var missile = _missileList[i];
                if (missile == null)
                {
                    _missileList.RemoveAt(i);
                    continue;
                }
                missile.PredictHitTank();
            }
        }
    }
}