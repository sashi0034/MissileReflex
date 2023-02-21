using MissileReflex.Src.Battle.Hud;
using UnityEngine;

namespace MissileReflex.Src.Battle
{
    public class BattleHud : MonoBehaviour
    {
        [SerializeField] private LabelTankNameManager labelTankNameManager;
        public LabelTankNameManager LabelTankNameManager => labelTankNameManager;
        
    }
}