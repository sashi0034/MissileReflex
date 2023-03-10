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
        
        [SerializeField] private ParticleSystem missileExplosion;
        public ParticleSystem MissileExplosion => missileExplosion;

        private readonly List<Missile> _missileList = new List<Missile>();
        private IntervalProcess _intervalProcess = new();

        public void Init()
        {
            ClearBattle();
            _intervalProcess = new IntervalProcess(predictMissileHit, ConstParam.Instance.MissilePredictInterval);
        }

        public void ClearBattle()
        {
            foreach (var missile in _missileList)
            {
                if (missile == null) continue;
                missile.Runner.Despawn(missile.Object);
            }
            _missileList.Clear();
        }

        public void ShootMissile(MissileInitArg arg, NetworkRunner runner)
        {
            var attacker = arg.Attacker;
            var attackerPlayer = attacker.OwnerNetworkPlayer;
            
            // ミサイルは自分自身が発射するように
            if (attacker.OwnerNetworkPlayer != PlayerRef.None && attacker.IsOwnerLocalPlayer() == false) return;

            var team = attacker.Team;

            runner.Spawn(
                missilePrefab[team.TeamId], arg.Attacker.transform.position, Quaternion.identity, attackerPlayer,
                onBeforeSpawned: (_, obj) =>
            {
                var missile = obj.GetComponent<Missile>();
                missile.Init(arg);
            });
        }

        public void RegisterMissile(Missile missile)
        {
            _missileList.Add(missile);
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