#nullable enable

using MissileReflex.Src.Params;
using MissileReflex.Src.Utils;
using UnityEngine;

namespace MissileReflex.Src.Battle
{
    public record MissileSourceData(
        float Speed)
    {
        public static readonly MissileSourceData Empty = 
            new MissileSourceData(0);
    };

    public record MissileInitArg(
        MissileSourceData SourceData,
        Vector3 InitialPos,
        Vector3 InitialVel);
    
    [DisallowMultipleComponent]
    public class Missile : MonoBehaviour
    {
        [SerializeField] private MissileDamage missileDamage;
        
        [SerializeField] private Rigidbody rigidBody;
        public Rigidbody Rigidbody => rigidBody;
        
        [SerializeField] private GameObject view;
        [SerializeField] private int lifeTimeReflectedCount = 3;

        private Vector3 viewInitialRotation;
        private float viewRotationAnimX = 0;

        private MissileSourceData _data = MissileSourceData.Empty;
        private MissilePhysic _physic;

        public Vector3 Pos => transform.position;

        public Missile()
        {
            _physic = new MissilePhysic(this);
        }

        public void Init(MissileInitArg arg)
        {
            _data = arg.SourceData;
            transform.position = arg.InitialPos;
            rigidBody.velocity = arg.InitialVel;

            viewInitialRotation = view.transform.localRotation.eulerAngles;
        }

        [EventFunction]
        private void Update()
        {
            // 衝突してダメージ与えた
            if (missileDamage.HitTankCount > 0)
            {
                Util.DestroyGameObject(gameObject);
                return;
            }
            
            _physic.Update();
            if (_physic.ReflectedCount >= lifeTimeReflectedCount)
            {
                Util.DestroyGameObject(this.gameObject);
                return;
            }

            updateViewAnim(Time.deltaTime);
        }

        private void updateViewAnim(float deltaTime)
        {
            viewRotationAnimX += deltaTime * 360;
            view.transform.localRotation = Quaternion.Euler(viewInitialRotation + Vector3.right * viewRotationAnimX);
        }

        [EventFunction]
        private void FixedUpdate()
        {
            rigidBody.velocity = rigidBody.velocity.normalized * _data.Speed;
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