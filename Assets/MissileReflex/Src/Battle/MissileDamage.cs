using System;
using MissileReflex.Src.Utils;
using UnityEngine;

namespace MissileReflex.Src.Battle
{
    public class MissileDamage : MonoBehaviour
    {
        [SerializeField] private float damageAmount = 1f;
        
        private int _hitTankCount = 0;
        public int HitTankCount => _hitTankCount;

        private int _hitMissileCount = 0;
        public int HitMissileCount => _hitMissileCount;

        private bool _hasEnteredMissileOwner = false;

        [EventFunction]
        public void OnTriggerEnter(Collider other)
        {
            if (checkHitWithTank(other)) return;
            if (checkHitWithMIissile(other)) return;
        }

        private bool checkHitWithTank(Collider other)
        {
            if (Missile.IsColliderTankFighter(other, out var tank) == false) return false;

            if (_hasEnteredMissileOwner == false)
            {
                // ミサイル発射した人自体ならキャンセル
                _hasEnteredMissileOwner = true;
                return false;
            }

            tank.Hp.CauseDamage(damageAmount);
            _hitTankCount++;
            return true;
        }
        
        private bool checkHitWithMIissile(Collider other)
        {
            if (other.gameObject.transform.parent.TryGetComponent<Missile>(out var otherMissile) == false) return false;

            _hitMissileCount++;
            otherMissile.Damage._hitMissileCount++;
            return true;
        }
    }
}