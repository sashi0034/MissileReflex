#nullable enable

using System.Collections.Generic;
using System.Linq;
using MissileReflex.Src.Battle;
using MissileReflex.Src.Utils;
using UnityEngine;

namespace MissileReflex.Src.Battle
{
    public class MissilePhysic
    {
        private readonly Missile owner;
        private Transform transform => owner.transform;
        private Rigidbody rigidBody => owner.Rigidbody;
        
        private Vector3 _velocityOld;
        private Vector3 _positionOld;
        
        private int _reflectedCount = 0;
        public int ReflectedCount => _reflectedCount;

        private List<Vector3> _reflectedVelListInFrame = new List<Vector3>();

        public MissilePhysic(Missile owner)
        {
            this.owner = owner;
        }

        public void Update()
        {
            _positionOld = transform.position;
            _velocityOld = rigidBody.velocity;
            
            transform.rotation = Quaternion.Euler(
                0,
                -Mathf.Atan2(_velocityOld.z, _velocityOld.x) * Mathf.Rad2Deg,
                0);
            
            if (_reflectedVelListInFrame.Count == 0) return;
            
            _reflectedVelListInFrame.Clear();
            _reflectedCount++;
        }
        
        public void OnCollisionEnter(Collision collision)
        {
            var vecIn = _velocityOld.FixY(0);
            var vecNorm = collision.contacts[0].normal.FixY(0);
            var vecReflect = Vector3.Reflect(vecIn, vecNorm);
            
            _reflectedVelListInFrame.Add(vecReflect);
            rigidBody.velocity = 
                _reflectedVelListInFrame.Aggregate((sum, elem) => sum += elem) / _reflectedVelListInFrame.Count;
            transform.position = _positionOld;
        }
    }
}