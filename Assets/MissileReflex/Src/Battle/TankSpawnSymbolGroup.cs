#nullable enable

using System;
using System.Collections.Generic;
using MissileReflex.Src.Utils;
using UnityEngine;

namespace MissileReflex.Src.Battle
{

    [Serializable]
    public class TankSpawnSymbolTeamList
    {
#nullable disable
        [SerializeField] private List<TankSpawnSymbol> list;
        public IReadOnlyList<TankSpawnSymbol> List => list;
#nullable enable
    }

    public class TankSpawnSymbolGroup : MonoBehaviour
    {
        private static TankSpawnSymbolGroup? _instance;
        public static TankSpawnSymbolGroup? Instance => _instance;
#nullable disable
        [SerializeField] private List<TankSpawnSymbolTeamList> groups;
        public List<TankSpawnSymbolTeamList> Groups => groups;
#nullable enable

        [EventFunction]
        private void Awake()
        {
            Util.EnsureSingleton(this, ref _instance);
        }

        public List<TankSpawnSymbol> FlatTankSpawnSymbols()
        {
            var result = new List<TankSpawnSymbol>{};
            foreach (var symbols in Groups) result.AddRange(symbols.List);
            return result;
        }
    }}