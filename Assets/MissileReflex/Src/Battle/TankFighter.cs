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
#nullable disable
        [SerializeField] private Rigidbody tankRigidbody;
        
        [SerializeField] private float accelSize = 10;
        [SerializeField] private float maxSpeed = 5;
        [SerializeField] private float velocityAttenuation = 0.5f;

        [SerializeField] private TankFighterCannon tankFighterCannon;
        [SerializeField] private TankFighterLeg tankFighterLeg;

        [SerializeField] private Collider selfCollider;
        [SerializeField] private GameObject selfView;

        [SerializeField] private TankExplosion effectTankExplosion;
#nullable enable
        
        [Networked]
        private PlayerRef _ownerNetworkPlayer { get; set; } = PlayerRef.None;

        public PlayerRef OwnerNetworkPlayer => _ownerNetworkPlayer;

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

        private TankFighterId _localId;
        public TankFighterId LocalId => _localId;
        
        [Networked(OnChanged = nameof(onChangedTeam))]
        private TankFighterTeam _team { get; set; }
        public TankFighterTeam Team => _team;

        [Networked] private int _teamMemberIndex { get; set; }
        public int TeamMemberIndex => _teamMemberIndex;

        [Networked] private string _tankName { get; set; } = "";
        public string TankName => _tankName;
        
        [Networked] private PlayerRating _playerRating { get; set; }
        public PlayerRating PlayerRating => _playerRating;
        
        [Networked]         
        private ref TankScore _earnedScore => ref MakeRef<TankScore>();
        public ref TankScore EarnedScore => ref _earnedScore;
        
        private UniTask _taskDeadAndRespawn = UniTask.CompletedTask;

        public override void Spawned()
        {
            transform.parent = battleRoot.TankManager.transform;
            ChangeMaterial(battleRoot.TankManager.GetTankMatOf(_team));
            _localId = battleRoot.TankManager.RegisterTank(this);
            _prediction.Init();
            battleRoot.Hud.LabelTankNameManager.BirthWith(this);
            getSpawnSymbol().Init(this);
        }

        private TankSpawnSymbol getSpawnSymbol()
        {
            return battleRoot.TankManager.GetSpawnSymbol(this);
        }


        public void Init(
            TankSpawnInfo spawnInfo,
            PlayerRef? ownerPlayer)
        {
            _input.Init();
            _hp = new TankFighterHp(1);
            _state = ETankFighterState.Alive;

            if (ownerPlayer != null) _ownerNetworkPlayer = ownerPlayer.Value;
            
            transform.position = spawnInfo.InitialPos;
            _initialPos = spawnInfo.InitialPos;

            _team = spawnInfo.Team;
            _teamMemberIndex = spawnInfo.TeamMemberIndex;

            _tankName = spawnInfo.TankName;
            _playerRating = spawnInfo.Rating;
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
            return Runner.LocalPlayer == _ownerNetworkPlayer;
        }
        
        [EventFunction]
        public override void FixedUpdateNetwork()
        {
            if (battleRoot.CancelBattle.IsCancellationRequested) return;
            if (_taskDeadAndRespawn.Status == UniTaskStatus.Pending) return;
            
            if (_state == ETankFighterState.Dead) return;

            // 死んでるかチェック
            if (checkStartDie()) return;
            
            checkInputMove(Runner.DeltaTime);
            
            updateInputShoot(Runner.DeltaTime);
        }
        
        private bool checkStartDie()
        {
            if (_hp.Value > 0) return false;
            if (_taskDeadAndRespawn.Status == UniTaskStatus.Pending) return false;
            
            // 死んだ
            rpcallStartDie();
            return true;
        }

        [Rpc]
        private void rpcallStartDie()
        {
            if (_taskDeadAndRespawn.Status == UniTaskStatus.Pending) return;
            _taskDeadAndRespawn = performDeadAndRespawn(battleRoot.CancelBattle)
                // 例外が起きた時も一応リスポーンするように
                .RunTaskHandlingErrorAsync(_ => invokeRespawnAfterDead(battleRoot.CancelBattle));
        }

        private async UniTask performDeadAndRespawn(CancellationToken cancel)
        {
            // 位置移動もdeadの中でやっている
            await performDead(cancel);

            invokeRespawnAfterDead(cancel);
        }

        private void invokeRespawnAfterDead(CancellationToken cancel)
        {
            // 復活
            resetRespawn();

            // 復活する演出
            performRespawnAfterDead(cancel).Forget();
        }

        private async UniTask performRespawnAfterDead(CancellationToken cancel)
        {
            _state = ETankFighterState.Immortal;

            selfView.transform.localScale = Vector3.zero;

            getSpawnSymbol().AnimRespawn(cancel).Forget();
            
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
            Debug.Assert(effect != null);
            if (effect != null) effect.transform.position = transform.position;
            
            var lastAttacker = _hp.FindLastAttacker(Runner);
            bool isKilledByLocalPlayer = lastAttacker != null && lastAttacker._ownerNetworkPlayer == Runner.LocalPlayer;
            if (effect != null) effect.Effect.cameraShake.enabled = 
                // ローカルプレイヤー自身がやられたときか
                _ownerNetworkPlayer == Runner.LocalPlayer ||
                // ローカルプレイヤー自身が攻撃したときにカメラシェイク
                isKilledByLocalPlayer;
            Util.DelayDestroyEffect(effect.ParticleSystem, cancel).Forget();
            
            // を倒したの表示
            if (lastAttacker != null && (isKilledByLocalPlayer || IsOwnerLocalPlayer())) 
                battleRoot.Hud.LabelKillOpponentManager.AppendLabel(lastAttacker, this);

            // スコア加算
            if (lastAttacker != null) battleRoot.Progress.MutateScoreOnKill(lastAttacker, this);
            
            // リスポーン地点の演出
            getSpawnSymbol().AnimWither();

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
            const float missileSpeed = 10f;
            
            tankFighterCannon.AnimShot();
            
            battleRoot.MissileManager.ShootMissile(new MissileInitArg(
                new MissileSourceData(missileSpeed),
                initialVel,
                this), 
                Runner);
        }
    }
}