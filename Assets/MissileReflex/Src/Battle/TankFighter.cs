#nullable enable
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MissileReflex.Src.Params;
using MissileReflex.Src.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MissileReflex.Src.Battle
{
    public class TankFighterInput
    {
        private Vector3 _moveVec = Vector3.zero;
        public Vector3 MoveVec => _moveVec;

        private float _shotRad = 0;
        public float ShotRad => _shotRad;
        
        private readonly BoolFlag _shotRequest = new BoolFlag();
        public BoolFlag ShotRequest => _shotRequest;

        public void Init()
        {
            _moveVec = Vector3.zero;
            _shotRad = 0;
            _shotRequest.Clear();
        }

        public void SetMoveVec(Vector3 move)
        {
            Debug.Assert(Vector3.SqrMagnitude(move) <= 1 + ConstParam.DeltaMilliF);
            _moveVec = move;
        }

        public void SetShotRad(float rad)
        {
            _shotRad = rad;
        }
        
        public void SetShotRadFromVec3(Vector3 vec)
        {
            _shotRad = Mathf.Atan2(vec.z, vec.x);
        }
    }

    public class TankFighterPrediction
    {
        private readonly List<Missile> _missileHits = new List<Missile>();
        // public IReadOnlyList<Missile> MissileHits => _missileHits;

        public void Init()
        {
            _missileHits.Clear();
        }
        
        public void ClearPredictedMissiles()
        {
            _missileHits.Clear();
        }

        // TODO: AIじゃなくて使わないときのためのフラグを作る
        public void PredictMissileHit(Missile missile)
        {
            _missileHits.Add(missile);
        }

        public Missile? FindPredictedMissile()
        {
            foreach (var missile in _missileHits)
            {
                if (missile != null) return missile;
            }

            return null;
        } 
        
        // public bool FindPredictedMissile(out Missile? hit)
        // {
        //     foreach (var missile in _missileHits)
        //     {
        //         if (missile == null) continue;
        //         hit = missile;
        //         return true;
        //     }
        //
        //     hit = null;
        //     return false;
        // } 
    }

    public interface ITankAgent
    {
        public BattleRoot BattleRoot { get; }
    }

    public class TankFighterHp
    {
        private float _value = 0;
        public float Value => _value;

        public void Init(float value)
        {
            this._value = value;
        }

        public void CauseDamage(float value)
        {
            _value = Mathf.Max(0, _value - value);
        }
    }

    public enum ETankFighterState
    {
        Alive,
        Dead
    }
    
    // TankFighterはAgentから動かす
    public class TankFighter : MonoBehaviour
    {
        [SerializeField] private Rigidbody tankRigidbody;
        
        [SerializeField] private float accelSize = 10;
        [SerializeField] private float maxSpeed = 5;
        [SerializeField] private float velocityAttenuation = 0.5f;

        [SerializeField] private TankFighterCannon tankFighterCannon;
        [SerializeField] private TankFighterLeg tankFighterLeg;

        [SerializeField] private GameObject viewObject;

        [SerializeField] private ParticleSystem effectExplosion;

        private ITankAgent? _parentAgent;

        private readonly TankFighterInput _input = new TankFighterInput();
        public TankFighterInput Input => _input;

        private readonly TankFighterPrediction _prediction = new TankFighterPrediction();
        public TankFighterPrediction Prediction => _prediction;

        private readonly TankFighterHp _hp = new TankFighterHp();
        public TankFighterHp Hp => _hp;

        [SerializeField] private float maxShotCoolingTime;
        private float _shotCoolingTime = 0;

        private ETankFighterState _state = ETankFighterState.Alive;

        private BattleRoot battleRoot => _parentAgent.BattleRoot;
        
        [EventFunction]
        private void Update()
        {
            if (_state == ETankFighterState.Dead) return;

            if (_hp.Value <= 0)
            {
                // 死んだ
                performDead();
                return;
            }
            
            checkInputMove();
            
            updateInputShoot(Time.deltaTime);
        }

        private async UniTask performDead()
        {
            // Debug.Log("change state to dead");
            
            _state = ETankFighterState.Dead;
            
            await transform.DORotate(new Vector3(0, 0, 180), 0.3f).SetEase(Ease.OutBack);
            await UniTask.Delay(0.3f.ToIntMilli());
            
            // 爆発
            var effect = Instantiate(effectExplosion, transform);
            Debug.Assert(effect != null);

            // ちょっと大きくなって小さくなる
            DOTween.Sequence(transform)
                .Append(viewObject.transform.DOScale(1.3f, 0.3f).SetEase(Ease.OutBack))
                .Append(viewObject.transform.DOScale(0f, 0.3f).SetEase(Ease.InSine));
            
            await UniTask.WaitUntil(() => effect == null || effect.isStopped);
            Util.DestroyGameObject(effect.gameObject);
            
            // Debug.Log("finished explosion effect");
            
            gameObject.SetActive(false);
        }

        public void Init(
            ITankAgent agent,
            Material? material)
        {
            _parentAgent = agent;
            _input.Init();
            _prediction.Init();
            _hp.Init(1);
            _shotCoolingTime = 0;
            _state = ETankFighterState.Alive;

            if (material != null) ChangeMaterial(material);
            
            battleRoot.TankManager.RegisterTank(this);
        }

        public bool IsAlive()
        {
            return _state != ETankFighterState.Dead;
        }

        [Button]
        public void ChangeMaterial(Material material)
        {
            tankFighterLeg.ChangeMaterial(material);
            tankFighterCannon.ChangeMaterial(material);
        }
        
        private void checkInputMove()
        {
            var inputVec = _input.MoveVec;

            bool hasInput = inputVec != Vector3.zero;
            
            // 移動アニメ制御
            tankFighterLeg.AnimRun(hasInput);
            
            if (hasInput)
            {
                tankFighterLeg.LerpLegRotation(Time.deltaTime, inputVec);
                trickViewRotation();
            }
            
            // 加速
            tankRigidbody.velocity += inputVec * accelSize * Time.deltaTime;
            if (tankRigidbody.velocity.sqrMagnitude > maxSpeed * maxSpeed)
                tankRigidbody.velocity = tankRigidbody.velocity.normalized * maxSpeed;

            // 減衰
            if (inputVec == Vector3.zero) tankRigidbody.velocity *= velocityAttenuation;
        }
        
        private void trickViewRotation()
        {
            const float trickDefaultAngle = 30;
            const float trickDeltaAngle = 15;
            
            viewObject.transform.localRotation =
                Quaternion.Euler(Vector3.right * (trickDefaultAngle - Mathf.Max(trickDeltaAngle * Mathf.Sin(tankFighterLeg.GetLegRotRadY()), 0)));
        }
        
        private void updateInputShoot(float deltaTime)
        {
            var shotDirection = new Vector3(Mathf.Cos(_input.ShotRad), 0, Mathf.Sin(_input.ShotRad));

            tankFighterCannon.LerpCannonRotation(Time.deltaTime, shotDirection);

            _shotCoolingTime = Math.Max(_shotCoolingTime - deltaTime, 0);

            // ミサイルを打つか確認
            if (_shotCoolingTime > 0 || _input.ShotRequest.PeekFlag() == false) return;
            
            // 発射
            _shotCoolingTime = maxShotCoolingTime;
            shootMissile(shotDirection);
        }
        
        private void shootMissile(Vector3 initialVel)
        {
            var initialPos = transform.position;

            const float missileSpeed = 10f;
            
            Debug.Assert(_parentAgent != null);
            _parentAgent.BattleRoot.MissileManager.ShootMissile(new MissileInitArg(
                new MissileSourceData(missileSpeed),
                initialPos,
                initialVel));
        }
    }
}