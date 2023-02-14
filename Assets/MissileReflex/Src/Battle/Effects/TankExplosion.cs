using CartoonFX;
using UnityEngine;

namespace MissileReflex.Src.Battle.Effects
{
    public class TankExplosion : MonoBehaviour
    {
        [SerializeField] private ParticleSystem particleSystem;
        public ParticleSystem ParticleSystem => particleSystem;

        [SerializeField] private CFXR_Effect effect;
        public CFXR_Effect Effect => effect;
    }
}