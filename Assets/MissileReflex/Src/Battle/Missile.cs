#nullable enable

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
    }
}