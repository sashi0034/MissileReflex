using System;
using System.Collections.Generic;
using MissileReflex.Src.Battle;
using MissileReflex.Src.Params;
using MissileReflex.Src.Utils;
using UnityEngine;

namespace MissileReflex.Src.Battle
{
    [DisallowMultipleComponent]
    public class MissileManager : MonoBehaviour
    {
        [SerializeField] private BattleRoot battleRoot;
        
        [SerializeField] private Missile missilePrefab;
        public Missile MissilePrefab => missilePrefab;

        [SerializeField] private float missileOffsetY = 0.5f;

        private readonly List<Missile> _missileList = new List<Missile>();
        private readonly IntervalProcess _intervalProcess;

        public MissileManager()
        {
            // TODO: 時間調整
            _intervalProcess = new IntervalProcess(predictMissileHit, 0.1f);
        }

        public void Init()
        {
            foreach (var missile in _missileList)
            {
                Util.DestroyGameObject(missile.gameObject);
            }
            _missileList.Clear();
            _intervalProcess.Clear();
        }

        public void ShootMissile(MissileInitArg arg)
        {
            var missile = Instantiate(missilePrefab, this.transform);
            _missileList.Add(missile);

            var fixedArg = arg with { InitialPos = arg.InitialPos.FixY(missileOffsetY) };
            missile.Init(fixedArg);
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