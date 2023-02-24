#nullable enable

using Fusion;
using MissileReflex.Src.Utils;
using UnityEngine;

namespace MissileReflex.Src.Battle
{
    public class MissileDamage : NetworkBehaviour
    {
        [SerializeField] private Missile owner;
        [SerializeField] private sbyte damageAmount = 1;
        
        private int _hitTankCount = 0;
        public int HitTankCount => _hitTankCount;

        private int _hitMissileCount = 0;
        public int HitMissileCount => _hitMissileCount;

        private bool _hasEnteredMissileOwner = false;

        [EventFunction]
        public void OnTriggerEnter(Collider other)
        {
            if (owner.HasDespawned) return;
            if (checkHitWithTank(other)) return;
            if (checkHitWithMissile(other)) return;
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

            // 無敵中ならキャンセル
            if (tank.IsImmortalNow()) return false;
                
            tank.Hp.CauseDamage(damageAmount, owner.OwnerFighter);
            _hitTankCount++;
            return true;
        }
        
        private bool checkHitWithMissile(Collider other)
        {
            if (other.gameObject.transform.parent.TryGetComponent<Missile>(out var otherMissile) == false) return false;

            _hitMissileCount++;
            if (otherMissile.Damage._hitMissileCount == 0)
            {
                // 爆発エフェクト発生
                owner.RequestEffectExplosion((owner.transform.position + otherMissile.transform.position) / 2);
            }
            
            return true;
        }
    }
}