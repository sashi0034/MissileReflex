using MissileReflex.Src.Utils;
using UnityEngine;

namespace MissileReflex.Src.Battle.Hud
{
    public class LabelTankNameManager : MonoBehaviour
    {
        [SerializeField] private LabelTankName labelTankName;

        public LabelTankName BirthWith(TankFighter tank)
        {
            var result = Instantiate(labelTankName, transform);
            result.RegisterTank(tank);
            return result;
        }

        public void Init()
        {
            Util.DestroyAllChildren(transform);
        }
    }
}