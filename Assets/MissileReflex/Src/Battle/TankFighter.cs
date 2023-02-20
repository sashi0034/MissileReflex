#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Fusion;
using MissileReflex.Src.Battle.Effects;
using MissileReflex.Src.Params;
using MissileReflex.Src.Utils;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace MissileReflex.Src.Battle
{
    // TankFighterはAgentから動かす
    public class TankFighter : NetworkBehaviour
    {
        [SerializeField] private Rigidbody tankRigidbody;
        
        [SerializeField] private float accelSize = 10;
        [SerializeField] private float maxSpeed = 5;
        [SerializeField] private float velocityAttenuation = 0.5f;

        [SerializeField] private TankFighterCannon tankFighterCannon;
        [SerializeField] private TankFighterLeg tankFighterLeg;

        [SerializeField] private Collider selfCollider;
        [SerializeField] private GameObject selfView;

        [SerializeField] private TankExplosion effectTankExplosion;
        
        [Networked]
        private PlayerRef _ownerPlayer { get; set; } = PlayerRef.None;

        private BattleRoot battleRoot => BattleRoot.Instance;

        [Networked]
        private ref TankFighterInput _input => ref MakeRef<TankFighterInput>();
        public ref TankFighterInput Input => ref _input;

        private readonly TankFighterPrediction _prediction = new TankFighterPrediction();
        public TankFighterPrediction Prediction => _prediction;

        [Networked] 
        private ref TankFighterHp _hp => ref MakeRef<TankFighterHp>();
        
        public ref TankFighterHp Hp => ref _hp;

        [SerializeField] private float maxShotCoolingTime;
        private float _shotCoolingTime = 0;

        [Networked] 
        private ETankFighterState _state { get; set; } = ETankFighterState.Alive;

        [Networked]
        private Vector3 _initialPos { get; set; }

        private TankFighterId _id;
        public TankFighterId Id => _id;
        
        [Networked(OnChanged = nameof(onChangedTeam))]
        private TankFighterTeam _team { get; set; }
        public TankFighterTeam Team => _team;
        
        private UniTask _interruptedTask = UniTask.CompletedTask;

        public override void Spawned()
        {
            transform.parent = battleRoot.TankManager.transform;
            ChangeMaterial(battleRoot.TankManager.GetTankMatOf(_team));
            _id = battleRoot.TankManager.RegisterTank(this);
            _prediction.Init();
        }

        public void Init(
            TankFighterTeam team,
            Vector3? initialPos,
            PlayerRef? ownerPlayer)
        {
            _input.Init();
            _hp = new TankFighterHp(1);
            _state = ETankFighterState.Alive;

            if (ownerPlayer != null) _ownerPlayer = ownerPlayer.Value;
            if (initialPos != null) transform.position = initialPos.Value;
            _initialPos = transform.position;

            _team = team;
        }
        private void resetRespawn()
        {
            _input.Init();
            _prediction.Init();
            _hp.RecoverFully();
            _shotCoolingTime = 0;
            _state = ETankFighterState.Alive;

            // transform.position = _initialPos;
            trickViewRotation();
            selfCollider.gameObject.SetActive(true);
        }

        public bool IsOwnerLocalPlayer()
        {
            return Runner.LocalPlayer == _ownerPlayer;
        }
        
        [EventFunction]
        public override void FixedUpdateNetwork()
        {
            if (_interruptedTask.Status == UniTaskStatus.Pending) return;
            
            if (_state == ETankFighterState.Dead) return;

            if (_hp.Value <= 0)
            {
                // 死んだ
                rpcallStartDie();
                return;
            }
            
            checkInputMove(Runner.DeltaTime);
            
            updateInputShoot(Runner.DeltaTime);
        }

        [Rpc]
        private void rpcallStartDie()
        {
            if (_state == ETankFighterState.Dead) return;
            _interruptedTask = performDeadAndRespawn(battleRoot.CancelBattle);
        }

        private async UniTask performDeadAndRespawn(CancellationToken cancel)
        {
            await performDead(cancel);

            // 復活
            resetRespawn();
            
            // 復活する演出
            performRespawnAfterDead().Forget();
        }

        private async UniTask performRespawnAfterDead()
        {
            _state = ETankFighterState.Immortal;

            selfView.transform.localScale = Vector3.zero;
            // await selfView.transform.DOScale(1f, 2.0f).SetEase(Ease.OutBack).SetLink(gameObject);
            // await DOTween.Sequence(selfView)
            //     .Append(selfView.transform.DOScale(1.5f, 0.5f).SetEase(Ease.OutBack))
            //     .Append(selfView.transform.DOScale(1.0f, 0.5f).SetEase(Ease.InSine))
            //     .SetLink(gameObject);
            // TODO: オーブ系のエフェクトで無敵を表現?
            await DOTween.Sequence(selfView)
                .Append(selfView.transform.DOScale(1.0f, 0.1f).SetEase(Ease.OutBack))
                .Append(selfView.transform.DOScale(0f, 0.1f).SetEase(Ease.InSine))
                .SetLoops(5)
                .SetLink(gameObject);
            await DOTween.Sequence(selfView)
                .Append(selfView.transform.DOScale(1.0f, 0.3f).SetEase(Ease.OutBack));
            _state = ETankFighterState.Alive;
        }

        private async UniTask performDead(CancellationToken cancel)
        {
            // Debug.Log("change state to dead");
            
            _state = ETankFighterState.Dead;
            
            await selfView.transform.DORotate(new Vector3(0, 0, 180), 0.3f)
                .SetEase(Ease.OutBack).SetLink(gameObject);
            await UniTask.Delay(0.3f.ToIntMilli(), cancellationToken: cancel);
            
            // コライダー無効にして
            tankRigidbody.velocity = Vector3.zero;
            selfCollider.gameObject.SetActive(false);
            
            // 爆発
            var effect = Instantiate(effectTankExplosion, transform.parent);
            effect.transform.position = transform.position;
            
            var lastAttacker = _hp.FindLastAttacker(Runner);
            effect.Effect.cameraShake.enabled = 
                // 自身がやられたときか
                _ownerPlayer == Runner.LocalPlayer ||
                // 自身が攻撃したときにカメラシェイク
                (lastAttacker!= null && lastAttacker._ownerPlayer == Runner.LocalPlayer);
            Util.DelayDestroyEffect(effect.ParticleSystem, cancel).Forget();
            
            Debug.Assert(effect != null);

            // ちょっと大きくなって小さくなる
            await DOTween.Sequence(transform)
                .Append(selfView.transform.DOScale(1.3f, 0.3f).SetEase(Ease.OutBack))
                .Append(selfView.transform.DOScale(0f, 0.3f).SetEase(Ease.InSine))
                .SetLink(gameObject);

            // リスポーン地点に移動
            transform.position = _initialPos;

            // 死亡ペナルティ時間
            await UniTask.Delay(ConstParam.Instance.TankDeathPenaltyTime.ToIntMilli(), cancellationToken: cancel);
        }



        public bool IsAlive()
        {
            return _state != ETankFighterState.Dead;
        }

        public bool IsImmortalNow()
        {
            return _state == ETankFighterState.Immortal;
        }

        [EventFunction]
        private static void onChangedTeam(Changed<TankFighter> changed)
        {
            var self = changed.Behaviour;
            self.ChangeMaterial(self.battleRoot.TankManager.GetTankMatOf(self._team));
        }

        [Button]
        public void ChangeMaterial(Material material)
        {
            tankFighterLeg.ChangeMaterial(material);
            tankFighterCannon.ChangeMaterial(material);
        }
        
        private void checkInputMove(float deltaTime)
        {
            var inputVec = _input.MoveVec;

            bool hasInput = inputVec != Vector3.zero;
            
            // 移動アニメ制御
            tankFighterLeg.AnimRun(hasInput);
            
            if (hasInput)
            {
                tankFighterLeg.LerpLegRotation(deltaTime, inputVec);
                trickViewRotation();
            }
            
            // 加速
            tankRigidbody.velocity += inputVec * accelSize * deltaTime;
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
            if (_shotCoolingTime > 0 || _input.PeekShotRequest() == false) return;
            
            // 発射
            _shotCoolingTime = maxShotCoolingTime;
            shootMissile(shotDirection);
        }
        
        private void shootMissile(Vector3 initialVel)
        {
            var initialPos = transform.position;

            const float missileSpeed = 10f;
            
            tankFighterCannon.AnimShot();
            
            battleRoot.MissileManager.ShootMissile(new MissileInitArg(
                new MissileSourceData(missileSpeed),
                initialPos,
                initialVel,
                this), 
                Runner);
        }
    }
}