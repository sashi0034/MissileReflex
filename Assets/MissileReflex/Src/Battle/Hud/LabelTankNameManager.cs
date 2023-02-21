using UnityEngine;

namespace MissileReflex.Src.Battle.Hud
{
    public class LabelTankNameManager : MonoBehaviour
    {
        [SerializeField] private LabelTankName labelTankName;

        public LabelTankName Birth()
        {
            return Instantiate(labelTankName, transform);
        }
    }
}