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

        private bool _hasEnteredMissileOwner = false;

        [EventFunction]
        public void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.transform.parent.TryGetComponent<TankFighter>(out var tank) == false) return;

            if (_hasEnteredMissileOwner == false)
            {
                // ミサイル発射した人自体ならキャンセル
                _hasEnteredMissileOwner = true;
                return;
            } 
            
            tank.Hp.CauseDamage(damageAmount);
            _hitTankCount++;
        }
    }
}