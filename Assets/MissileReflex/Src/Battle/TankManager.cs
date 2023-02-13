using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MissileReflex.Src.Battle
{
    public class TankManager : MonoBehaviour
    {
        private readonly List<TankFighter> _tankFighterList = new List<TankFighter>();
        public IReadOnlyList<TankFighter> List => _tankFighterList;

        public void Init()
        {
            _tankFighterList.Clear();
        }

        public void RegisterTank(TankFighter fighter)
        {
            _tankFighterList.Add(fighter);
        }
    }
}