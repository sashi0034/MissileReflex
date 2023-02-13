using MissileReflex.Src.Battle;
using MissileReflex.Src.Params;
using MissileReflex.Src.Utils;
using UnityEngine;

namespace MissileReflex.Src.Battle
{
    [DisallowMultipleComponent]
    public class MissileManager : MonoBehaviour
    {
        [SerializeField] private Missile missilePrefab;
        public Missile MissilePrefab => missilePrefab;

        [SerializeField] private float missileOffsetY = 0.5f;

        public void ShootMissile(MissileInitArg arg)
        {
            var missile = Instantiate(missilePrefab, this.transform);

            var fixedArg = arg with { InitialPos = arg.InitialPos.FixY(missileOffsetY) };
            missile.Init(fixedArg);
        }
        
    }
}