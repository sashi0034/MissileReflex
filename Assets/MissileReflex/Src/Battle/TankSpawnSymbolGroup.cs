using System;
using System.Collections.Generic;
using UnityEngine;

namespace MissileReflex.Src.Battle
{

    [Serializable]
    public class TankSpawnSymbolTeamList
    {
        [SerializeField] private List<TankSpawnSymbol> list;
        public IReadOnlyList<TankSpawnSymbol> List => list;
    }

    public class TankSpawnSymbolGroup : MonoBehaviour
    {
        [SerializeField] private List<TankSpawnSymbolTeamList> groups;
        public List<TankSpawnSymbolTeamList> Groups => groups;
        
        public List<TankSpawnSymbol> FlatTankSpawnSymbols()
        {
            var result = new List<TankSpawnSymbol>{};
            foreach (var symbols in Groups) result.AddRange(symbols.List);
            return result;
        }
    }}