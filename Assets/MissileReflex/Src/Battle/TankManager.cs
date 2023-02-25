#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using MissileReflex.Src.Params;
using MissileReflex.Src.Utils;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace MissileReflex.Src.Battle
{
    public record TankSpawnInfo(
        TankFighterTeam Team, 
        int TeamMemberIndex,
        Vector3 InitialPos,
        string TankName)
    {};

    public class TankManager : MonoBehaviour
    {
#nullable disable
        [SerializeField] private BattleRoot battleRoot;
        [SerializeField] private Material[] tankMaterialList;

        [SerializeField] private NetworkPrefabRef playerPrefab;
        [SerializeField] private NetworkPrefabRef aiPrefab;
#nullable enable

        // チームごとのspawn位置
        private TankSpawnSymbolGroup? tankSpawnSymbolGroup;
        
        private readonly List<TankFighter> _tankFighterList = new List<TankFighter>();
        public IReadOnlyList<TankFighter> List => _tankFighterList;

        private TankFighter? _localPlayerTank;

        private float[,] _tankSqrMagAdjMat = new float[,]{};

        private IntervalProcess _processCalcTankSqrMagAdjMat = new();

        public void Init()
        {
            _tankFighterList.Clear();
            _tankSqrMagAdjMat = new float[,]{};
            _processCalcTankSqrMagAdjMat =
                new IntervalProcess(calcTankSqrMagAdjMat, ConstParam.Instance.TankAdjMatUpdateInterval);
            _localPlayerTank = null;

            tankSpawnSymbolGroup = FindObjectOfType<TankSpawnSymbolGroup>();
            Debug.Assert(tankSpawnSymbolGroup != null);
        }

        [EventFunction]
        private void Update()
        {
            _processCalcTankSqrMagAdjMat.Update(Time.deltaTime);
        }

        public TankFighterId RegisterTank(TankFighter fighter)
        {
            _tankFighterList.Add(fighter);
            
            int numTank = _tankFighterList.Count;
            _tankSqrMagAdjMat = new float[numTank, numTank];
            
            return new TankFighterId(numTank - 1);
        }

        public Material GetTankMatOf(TankFighterTeam team)
        {
            Debug.Assert(team.TeamId < tankMaterialList.Length);
            return tankMaterialList[team.TeamId];
        }

        private void calcTankSqrMagAdjMat()
        {
            int numTank = _tankFighterList.Count;
            
            // タンク間平方距離の隣接行列を更新
            for (int row = 0; row < numTank; ++row)
            {
                for (int column = row + 1; column < numTank; ++column)
                {
                    if (row == column) _tankSqrMagAdjMat[row, row] = 0;
                    
                    var tank1 = _tankFighterList[row];
                    var tank2 = _tankFighterList[column];
                    var sqrMag = (tank1.transform.position - tank2.transform.position).sqrMagnitude;

                    _tankSqrMagAdjMat[row, column] = sqrMag;
                    _tankSqrMagAdjMat[column, row] = sqrMag;
                }
            }
        }

        public void SpawnPlayer(NetworkRunner runner, PlayerRef player, TankSpawnInfo spawnInfo)
        {
            runner.Spawn(playerPrefab, spawnInfo.InitialPos, Quaternion.identity, player, onBeforeSpawned: (networkRunner, obj) =>
            {
                obj.GetComponent<TankAgentPlayer>().Init(spawnInfo, player);
            });
        }
        
        public void SpawnAi(NetworkRunner runner, TankSpawnInfo spawnInfo)
        {
            runner.Spawn(aiPrefab, spawnInfo.InitialPos, Quaternion.identity, onBeforeSpawned: (networkRunner, obj) =>
            {
                obj.GetComponent<TankAgentAi>().Init(spawnInfo);
            });
        }

        private TankSpawnSymbol getTankSpawnSymbol(TankFighterTeam team, int teamMemberIndex)
        {
            Debug.Assert(tankSpawnSymbolGroup != null);
            return tankSpawnSymbolGroup.Groups[team.TeamId].List[teamMemberIndex];
        }

        public TankSpawnInfo GetNextSpawnInfo(string name)
        {
            int numTank = _tankFighterList.Count;

            var team = new TankFighterTeam(numTank % ConstParam.NumTankTeam);
            int teamMemberIndex = numTank / ConstParam.NumTankTeam;

            return new TankSpawnInfo(
                team, 
                teamMemberIndex,
                getTankSpawnSymbol(team, teamMemberIndex).transform.position.FixY(ConstParam.Instance.PlayerDefaultY),
                name);
        }

        public TankSpawnSymbol GetSpawnSymbol(TankFighter tank)
        {
            var team = tank.Team;
            var teamMemberIndex = tank.TeamMemberIndex;
            var symbol = getTankSpawnSymbol(team, teamMemberIndex);
            return symbol;
        }

        public float GetTankSqrMagAdjMatAt(TankFighterId id, int column)
        {
            return _tankSqrMagAdjMat[id.Value, column];
        }

        public TankFighter? FindLocalPlayerTank()
        {
            if (_localPlayerTank != null) return _localPlayerTank;
            
            foreach (var tank in _tankFighterList)
            {
                if (tank.IsOwnerLocalPlayer() == false) continue;
                _localPlayerTank = tank;
                return tank;
            }

            return null;
        }
    }
}