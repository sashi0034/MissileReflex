#nullable enable
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MissileReflex.Src.Params;
using MissileReflex.Src.Utils;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace MissileReflex.Src.Battle
{
    // TankFighterはAgentから動かす
    public class TankFighter : MonoBehaviour
    {
        [SerializeField] private Rigidbody tankRigidbody;
        
        [SerializeField] private float accelSize = 10;
        [SerializeField] private float maxSpeed = 5;
        [SerializeField] private float velocityAttenuation = 0.5f;

        [SerializeField] private TankFighterCannon tankFighterCannon;
        [SerializeField] private TankFighterLeg tankFighterLeg;

        [SerializeField] private Collider selfCollider;
        [SerializeField] private GameObject selfView;

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

        private Vector3 _initialPos;

        private BattleRoot battleRoot => _parentAgent.BattleRoot;
        
        
        public void Init(
            ITankAgent agent,
            Material? material,
            Vector3? initialPos)
        {
            _parentAgent = agent;
            _input.Init();
            _prediction.Init();
            _hp.Init(1);
            _shotCoolingTime = 0;
            _state = ETankFighterState.Alive;

            if (material != null) ChangeMaterial(material);
            
            battleRoot.TankManager.RegisterTank(this);

            if (initialPos != null) transform.position = initialPos.Value;
            _initialPos = transform.position;
        }
        private void resetRespawn()
        {
            _input.Init();
            _prediction.Init();
            _hp.RecoverFully();
            _shotCoolingTime = 0;
            _state = ETankFighterState.Alive;

            transform.position = _initialPos;
            gameObject.SetActive(true);
        }
        
        [EventFunction]
        private void Update()
        {
            if (_state == ETankFighterState.Dead) return;

            if (_hp.Value <= 0)
            {
                // 死んだ
                performDeadAndRespawn().RunTaskHandlingError();
                return;
            }
            
            checkInputMove();
            
            updateInputShoot(Time.deltaTime);
        }

        private async UniTask performDeadAndRespawn()
        {
            await performDead();

            // 復活
            resetRespawn();
            
            // 復活する演出
            selfView.transform.localScale = Vector3.zero;
            await selfView.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack);
        }

        private async UniTask performDead()
        {
            // Debug.Log("change state to dead");
            
            _state = ETankFighterState.Dead;
            
            await selfView.transform.DORotate(new Vector3(0, 0, 180), 0.3f).SetEase(Ease.OutBack);
            await UniTask.Delay(0.3f.ToIntMilli());
            
            // コライダー無効にして
            selfCollider.gameObject.SetActive(false);
            
            // 爆発
            var effect = Instantiate(effectExplosion, transform);
            Debug.Assert(effect != null);

            // ちょっと大きくなって小さくなる
            DOTween.Sequence(transform)
                .Append(selfView.transform.DOScale(1.3f, 0.3f).SetEase(Ease.OutBack))
                .Append(selfView.transform.DOScale(0f, 0.3f).SetEase(Ease.InSine));
            
            await UniTask.WaitUntil(() => effect == null || effect.isStopped);
            Util.DestroyGameObject(effect.gameObject);
            
            // Debug.Log("finished explosion effect");
            
            gameObject.SetActive(false);
            selfCollider.gameObject.SetActive(true);
            selfView.transform.rotation = quaternion.Euler(Vector3.zero);
            selfView.transform.localScale = Vector3.one;
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
            
            selfView.transform.localRotation =
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