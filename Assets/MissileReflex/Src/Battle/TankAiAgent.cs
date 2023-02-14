﻿using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using MissileReflex.Src.Params;
using MissileReflex.Src.Utils;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace MissileReflex.Src.Battle
{
    [Serializable]
    public class TankAiAgentParam
    {
        [SerializeField] private float updateInterval = 0.1f;
        public float UpdateInterval => updateInterval;
        
        [SerializeField] private float shotRange = 10;
        public float ShotRange => shotRange;
        public float ShotRangeSqrMag => shotRange * shotRange;

        // [SerializeField] private float retrieveDuration = 0.3f;
        // public float RetrieveDuration => retrieveDuration;
    }
    
    
    public class TankAiAgent : MonoBehaviour, ITankAgent
    {
        [SerializeField] private TankFighter selfTank;
        private Vector3 selfTankPos => selfTank.transform.position;
        private TankFighterInput tankIn => selfTank.Input;
        private TankFighterPrediction tankPredict => selfTank.Prediction;

        [SerializeField] private BattleRoot battleRoot;
        public BattleRoot BattleRoot => battleRoot;

        [SerializeField] private Material enemyMaterial;

        [SerializeField] private NavMeshAgent navAi;

        private static TankAiAgentParam param => ConstParam.Instance.TankAiAgentParam;

        [EventFunction]
        private void Start()
        {
            Init();
        }

        public void Init()
        {
            selfTank.Init(this, enemyMaterial);
            processAiRoutine().Forget();

            navAi.speed = 0;
            navAi.angularSpeed = 0;
            navAi.acceleration = 0;
            navAi.updatePosition = false;
            navAi.updateRotation = false;
        }

        [EventFunction]
        private void Update()
        {
        }

        private async UniTask processAiRoutine()
        {
            while (gameObject != null)
            {
                await UniTask.Delay(param.UpdateInterval.ToIntMilli());

                var targetTank = battleRoot.Player.Tank;
                var selfPos = selfTank.transform.position;

                var approachingMissile = tankPredict.FindPredictedMissile();
                tankPredict.ClearPredictedMissiles();
                if (approachingMissile != null)
                {
                    // ミサイルと当たりそうなので避ける
                    await avoidApproachingMissile(approachingMissile, param.UpdateInterval);
                    continue;
                }

                if (isNoWallBetweenTargetTank(selfPos, targetTank))
                {
                    // 射程内に入ってるので退き撃ち
                    shotWithRetreat(targetTank);
                }
                else
                {
                    // 射程内に入ってないので目標に近づく
                    approachrTargetTank(targetTank);
                }
                
            }
        }

        private async UniTask avoidApproachingMissile(Missile approachingMissile, float evasionTime)
        {
            var missilePos = approachingMissile.Pos;
            var evasionVec = findSpaciousOrthogonalVec(selfTankPos, missilePos - selfTankPos);
            tankIn.SetMoveVec(evasionVec.normalized);
            await UniTask.Delay(evasionTime.ToIntMilli());
        }

        private static bool isNoWallBetweenTargetTank(Vector3 selfPos, TankFighter targetTank)
        {
            var targetPos = targetTank.transform.position;
            var targetVed = targetPos - selfPos;
            
            if (Physics.BoxCast(selfPos, ConstParam.Instance.MissileColBoxHalfExt, targetVed, 
                    out var rayHit, Quaternion.Euler(targetVed), param.ShotRange) == false) 
                return false;
            
            return rayHit.transform == targetTank.transform;
        }

        private void shotWithRetreat(TankFighter targetTank)
        {
            var selfPos = selfTank.transform.position;
            var targetPos = targetTank.transform.position;
            // var destVec = calcDestVecToTarget(targetTank);
            var destVec = targetPos - selfPos;
            
            var rotatedDestVec = findSpaciousOrthogonalVec(selfPos, destVec);

            tankIn.SetMoveVec(rotatedDestVec.normalized);
            tankIn.SetShotRadFromVec3(destVec);
            tankIn.ShotRequest.UpFlag();
            // await UniTask.Delay(param.UpdateInterval.ToIntMilli());
        }

        // 自分の位置から直行ベクトルのうちスペースにゆとりのあるほうを探す
        private static Vector3 findSpaciousOrthogonalVec(Vector3 selfPos, Vector3 destVec)
        {
            var rotatedDestVec1 = Quaternion.Euler(0, 90, 0) * destVec;
            var rotatedDestVec2 = Quaternion.Euler(0, -90, 0) * destVec;

            var spaceVec1 = Physics.Raycast(selfPos, rotatedDestVec1, out var rayHit1, param.ShotRange)
                ? rayHit1.distance
                : param.ShotRange;
            var spaceVec2 = Physics.Raycast(selfPos, rotatedDestVec2, out var rayHit2, param.ShotRange)
                ? rayHit2.distance
                : param.ShotRange;

            // 90度回転させたベクトルのうち、壁までの距離がより遠い方を方向として選択
            var rotatedDestVec = spaceVec1 < spaceVec2 ? rotatedDestVec2 : rotatedDestVec1;
            return rotatedDestVec;
        }

        private float calcSqrMagSelfWithTargetTank(TankFighter target)
        {
            return (target.transform.position - selfTank.transform.position).sqrMagnitude;
        }

        private void approachrTargetTank(TankFighter target)
        {
            var destVec = calcDestVecToTarget(target);

            tankIn.SetMoveVec(destVec.normalized);
        }

        private Vector3 calcDestVecToTarget(TankFighter target)
        {
            navAi.nextPosition = selfTank.transform.position;
            navAi.SetDestination(target.transform.position);

            var destPos = navAi.steeringTarget;
            var currPos = selfTank.transform.position;
            var destVec = destPos - currPos;
            return destVec;
        }

        private static Vector3 randVecCross()
        {
            return Random.Range(0, 4) switch
            {
                0 => Vector3.forward,
                1 => Vector3.back,
                2 => Vector3.left,
                3 => Vector3.right,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}