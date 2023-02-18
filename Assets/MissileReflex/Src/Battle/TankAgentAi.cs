#nullable enable

using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MissileReflex.Src.Params;
using MissileReflex.Src.Utils;
using Sirenix.OdinInspector;
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
    
    
    public class TankAgentAi : MonoBehaviour, ITankAgent
    {
        [SerializeField] private TankFighter selfTank;
        private Vector3 selfTankPos => selfTank.transform.position;
        private TankFighterInput tankIn => selfTank.Input;
        private TankFighterPrediction tankPredict => selfTank.Prediction;

        private BattleRoot battleRoot = BattleRoot.Instance;
        private TankManager tankManager => battleRoot.TankManager;

        private static TankAiAgentParam param => ConstParam.Instance.TankAiAgentParam;
        

        public void Init(int selfTeam)
        {
            selfTank.Init(new TankFighterTeam(selfTeam), null, null);
            processAiRoutine().Forget();
        }

        private async UniTask processAiRoutine()
        {
            while (gameObject != null)
            {
                await UniTask.Delay(param.UpdateInterval.ToIntMilli());

                await processAiRoutineFrame();
            }
        }

        private async UniTask processAiRoutineFrame()
        {
            if (selfTank.IsAlive() == false) return;

            var selfPos = selfTank.transform.position;

            var approachingMissile = tankPredict.FindPredictedMissile();
            tankPredict.ClearPredictedMissiles();
            if (approachingMissile != null)
            {
                // ミサイルと当たりそうなので避ける
                await avoidApproachingMissile(approachingMissile, param.UpdateInterval);
                return;
            }

            // ターゲットを探す
            var targetTank = findTargetTankNearSelf();
            if (targetTank == null) return;

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

        private TankFighter? findTargetTankNearSelf()
        {
            TankFighter? target = null;
            float targetSqrMag = 0;
            for (int i = 0; i < tankManager.List.Count; i++)
            {
                var checking = tankManager.List[i];
                if (checking.IsAlive() == false) continue;
                if (selfTank.Team.IsSame(checking.Team)) continue;

                float checkingSqrMag = tankManager.GetTankSqrMagAdjMatAt(selfTank.Id, i);
                if (target != null && checkingSqrMag > targetSqrMag) continue;
                
                // 近いターゲットを更新
                target = checking;
                targetSqrMag = checkingSqrMag;
            }
            
            // Debug.Log( selfTank.Id + " target = " + (target==null ? "null" : target.Id));
            return target;
        }

        private async UniTask avoidApproachingMissile(Missile approachingMissile, float evasionTime)
        {
            var missilePos = approachingMissile.Pos;
            var evasionVec = findSpaciousOrthogonalVec(selfTankPos, missilePos - selfTankPos);
            
            // 回避
            tankIn.SetMoveVec(evasionVec.normalized);
            
            // ミサイルの迎撃を試みる
            tankIn.SetShotRadFromVec3(missilePos - selfTankPos);
            tankIn.ShotRequest.UpFlag();
            
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
            var path = new NavMeshPath();
            NavMesh.CalculatePath(selfTankPos, target.transform.position, NavMesh.AllAreas, path);
            // Util.ExecutePerFrame(100, () =>
            // {
            //     for (var index = 0; index < path.corners.Length - 1; index++) Debug.DrawLine(path.corners[index], path.corners[index + 1], Color.blue);
            // }).Forget();

            const float delta = 0.1f;
            if (path.corners.Length == 0) return Vector3.zero;
            var destPos = path.corners.Length == 1 || (selfTankPos - path.corners[0]).sqrMagnitude < delta 
                ? path.corners[0] 
                : path.corners[1];

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