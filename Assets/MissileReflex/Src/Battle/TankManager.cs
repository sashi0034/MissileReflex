using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using MissileReflex.Src.Params;
using MissileReflex.Src.Utils;
using UnityEngine;

namespace MissileReflex.Src.Battle
{
    public class TankManager : MonoBehaviour
    {
        [SerializeField] private BattleRoot battleRoot;
        [SerializeField] private Material[] tankMaterialList;

        [SerializeField] private NetworkPrefabRef playerPrefab;
        [SerializeField] private NetworkPrefabRef aiPrefab;
        
        private readonly List<TankFighter> _tankFighterList = new List<TankFighter>();
        public IReadOnlyList<TankFighter> List => _tankFighterList;

        private float[,] _tankSqrMagAdjMat = new float[,]{};

        private IntervalProcess _processCalcTankSqrMagAdjMat = new();

        public void Init()
        {
            _tankFighterList.Clear();
            _tankSqrMagAdjMat = new float[,]{};
            _processCalcTankSqrMagAdjMat =
                new IntervalProcess(calcTankSqrMagAdjMat, ConstParam.Instance.TankAdjMatUpdateInterval);
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

        public void SpawnPlayer(NetworkRunner runner, PlayerRef player)
        {
            var pos = new Vector3(
                player.RawEncoded % runner.Config.Simulation.DefaultPlayers * 3, 
                ConstParam.Instance.PlayerDefaultY, 
                0);
            runner.Spawn(playerPrefab, pos, Quaternion.identity, player, onBeforeSpawned: (networkRunner, obj) =>
            {
                obj.GetComponent<TankAgentPlayer>().Init(pos, player);
            });
        }
        
        public void SpawnAi(NetworkRunner runner)
        {
            // TODO: 位置とチームをちゃんと設定
            var pos = new Vector3(
                0, 
                ConstParam.Instance.PlayerDefaultY, 
                -5);
            var team = 1;
            runner.Spawn(aiPrefab, pos, Quaternion.identity, onBeforeSpawned: (networkRunner, obj) =>
            {
                obj.GetComponent<TankAgentAi>().Init(team);
            });
        }

        public float GetTankSqrMagAdjMatAt(TankFighterId id, int column)
        {
            return _tankSqrMagAdjMat[id.Value, column];
        }
    }
}