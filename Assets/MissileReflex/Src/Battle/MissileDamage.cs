#nullable enable

using Fusion;
using MissileReflex.Src.Utils;
using UnityEngine;

namespace MissileReflex.Src.Battle
{
    public class MissileDamage : NetworkBehaviour
    {
#nullable disable
        [SerializeField] private Missile owner;
        [SerializeField] private sbyte damageAmount = 1;
#nullable enable
        
        private int _hitTankCount = 0;
        public int HitTankCount => _hitTankCount;

        private int _hitMissileCount = 0;
        public int HitMissileCount => _hitMissileCount;
        

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

            // ミサイル発射した人自体に当たり判定が発生するのは、一度跳ね返ってから            
            if (owner.ReflectedCount == 0 && tank == owner.OwnerFighter) return false;

            // 無敵中ならキャンセル
            if (tank.IsImmortalNow()) return false;

            if (hasAuthorityDamageTank(tank)) rpcallDamageTank(tank);
            return true;
        }

        private bool hasAuthorityDamageTank(TankFighter tank)
        {
            return
                // AIはホストでダメージ判定
                (tank.OwnerNetworkPlayer == PlayerRef.None && Runner.IsServer) ||
                // プレイヤーは自分の捜査対象のみ判定
                tank.IsOwnerLocalPlayer();
        }

        [Rpc]
        private void rpcallDamageTank(TankFighter target)
        {
            target.Hp.CauseDamage(damageAmount, owner.OwnerFighter);
            _hitTankCount++;
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