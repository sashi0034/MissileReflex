#nullable enable

using Cysharp.Threading.Tasks;
using Fusion;
using MissileReflex.Src.Params;
using MissileReflex.Src.Utils;
using UnityEngine;

namespace MissileReflex.Src.Battle
{
    public struct MissileSourceData : INetworkStruct
    {
        private float speed;
        public float Speed => speed;

        public MissileSourceData(float speed)
        {
            this.speed = speed;
        }

        public static readonly MissileSourceData Empty = 
            new MissileSourceData(0);
    };

    public record MissileInitArg(
        MissileSourceData SourceData,
        Vector3 InitialPos,
        Vector3 InitialVel,
        TankFighter Attacker);
    
    [DisallowMultipleComponent]
    public class Missile : NetworkBehaviour
    {
        [SerializeField] private MissileDamage missileDamage;
        public MissileDamage Damage => missileDamage;
        
        [SerializeField] private Rigidbody rigidBody;
        public Rigidbody Rigidbody => rigidBody;
        
        [SerializeField] private GameObject view;
        [SerializeField] private int lifeTimeReflectedCount = 3;

        [SerializeField] private ParticleSystem missileExplosion;

        private NetworkObject _selfNetwork;
        
        private BattleRoot _battleRoot => BattleRoot.Instance;
        public MissileManager Manager => _battleRoot.MissileManager;

        private bool _hasDespawned = false;
        public bool HasDespawned => _hasDespawned;

        private Vector3 _viewInitialRotation;
        private float _viewRotationAnimX = 0;
        private MissilePhysic _physic;

        [Networked]
        private MissileSourceData _data { get; set; } = MissileSourceData.Empty;

        [Networked] 
        private TankFighter _ownerFighter { get; set; }
        public TankFighter OwnerFighter => _ownerFighter;

        [Networked] 
        private int _reflectedCount { get; set; } = 0;

        public Vector3 Pos => transform.position;

        public Missile()
        {
            _physic = new MissilePhysic(this);
        }

        public override void Spawned()
        {
            _selfNetwork = GetComponent<NetworkObject>();
            
            _viewInitialRotation = view.transform.localRotation.eulerAngles;

            transform.parent = Manager.transform;
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            _hasDespawned = true;
            
            // 最大回数まで反射したときもエフェクトを出す
            if (isReflectedUpTo()) BirthEffectExplosion(transform.position);
        }

        public void Init(MissileInitArg arg)
        {
            _data = arg.SourceData;
            _ownerFighter = arg.Attacker;
            transform.position = arg.InitialPos;
            rigidBody.velocity = arg.InitialVel;
        }

        [EventFunction]
        public override void FixedUpdateNetwork()
        {
            // 衝突した
            if (missileDamage.HitTankCount > 0 || missileDamage.HitMissileCount > 0)
            {
                Runner.Despawn(_selfNetwork);
                return;
            }
            
            _physic.Update();
            
            // たくさん反射したのでおしまい
            if (isReflectedUpTo())
            {
                Runner.Despawn(_selfNetwork);
                return;
            }

            updateViewAnim(Runner.DeltaTime);

            // 速度調整
            rigidBody.velocity = rigidBody.velocity.normalized * _data.Speed;
        }

        private bool isReflectedUpTo()
        {
            return _reflectedCount >= lifeTimeReflectedCount;
        }

        public void IncReflectedCount()
        {
            _reflectedCount++;
        }
        
        public void BirthEffectExplosion(Vector3 pos, RpcInfo info = default)
        {
            var effect = Instantiate(missileExplosion, Manager.transform);
            effect.transform.position = pos;
            Util.DelayDestroyEffect(effect, _battleRoot.CancelBattle).Forget();
        }
        
        private void updateViewAnim(float deltaTime)
        {
            _viewRotationAnimX += deltaTime * 360;
            view.transform.localRotation = Quaternion.Euler(_viewInitialRotation + Vector3.right * _viewRotationAnimX);
        }

        [EventFunction]
        private void OnCollisionEnter(Collision collision)
        {
            _physic.OnCollisionEnter(collision);
        }

        public void PredictHitTank()
        {
            // 進路方向にrayを飛ばす
            if (Physics.BoxCast(transform.position, ConstParam.Instance.MissileColBoxHalfExt, rigidBody.velocity, 
                    out var rayHit, Quaternion.Euler(rigidBody.velocity), ConstParam.Instance.MissilePredictRange) == false) return;
            // if (Physics.Raycast(transform.position, rigidBody.velocity, out var rayHit,
            //         ConstParam.Instance.MissilePredictRange) == false) return;
                
            var other = rayHit.collider;
            if (IsColliderTankFighter(other, out var tank) == false) return;
            
            // 当たりそうなので通知
            tank.Prediction.PredictMissileHit(this);
        }

        public static bool IsColliderTankFighter(Collider other, out TankFighter tank)
        {
            return other.gameObject.transform.parent.TryGetComponent<TankFighter>(out tank);
        }
    }
}