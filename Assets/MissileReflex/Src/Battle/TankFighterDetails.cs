#nullable enable

using System.Collections.Generic;
using Fusion;
using MissileReflex.Src.Params;
using MissileReflex.Src.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MissileReflex.Src.Battle
{
    public struct TankFighterInput : INetworkStruct
    {
        // Recognized float-based Networked Properties will implement Fusion's compression.
        // Such as Float, Double, Vector2, Vector3, Color, Matrix etc.
        [Networked] private Vector3 _moveVec { get; set; }
        [Networked] private float _shotRad { get; set; }
        private NetworkBool _shotRequest;

        public Vector3 MoveVec => _moveVec;
        public float ShotRad => _shotRad;

        public void Init()
        {
            _moveVec = Vector3.zero;
            _shotRad = 0;
            _shotRequest = false;
        }

        public void SetMoveVec(Vector3 move)
        {
            Debug.Assert(Vector3.SqrMagnitude(move) <= 1 + ConstParam.DeltaMilliF);
            _moveVec = move;
        }

        public void SetShotRad(float rad)
        {
            _shotRad = rad;
        }
        
        public void SetShotRadFromVec3(Vector3 vec)
        {
            _shotRad = Mathf.Atan2(vec.z, vec.x);
        }

        public void MakeShotRequest()
        {
            _shotRequest = true;
        }

        public bool PeekShotRequest()
        {
            var result = _shotRequest;
            _shotRequest = false;
            return result;
        }
    }

    public class TankFighterPrediction
    {
        private readonly List<Missile> _missileHits = new List<Missile>();
        // public IReadOnlyList<Missile> MissileHits => _missileHits;

        public void Init()
        {
            _missileHits.Clear();
        }
        
        public void ClearPredictedMissiles()
        {
            _missileHits.Clear();
        }

        // TODO: AIじゃなくて使わないときのためのフラグを作る
        public void PredictMissileHit(Missile missile)
        {
            _missileHits.Add(missile);
        }

        public Missile? FindPredictedMissile()
        {
            foreach (var missile in _missileHits)
            {
                if (missile != null) return missile;
            }

            return null;
        } 
        
        // public bool FindPredictedMissile(out Missile? hit)
        // {
        //     foreach (var missile in _missileHits)
        //     {
        //         if (missile == null) continue;
        //         hit = missile;
        //         return true;
        //     }
        //
        //     hit = null;
        //     return false;
        // } 
    }

    public interface ITankAgent {}

    public struct TankFighterHp : INetworkStruct
    {
        private sbyte _maxValue;
        private sbyte _value;
        private NetworkBehaviourId _lastAttacker;

        public sbyte Value => _value;
        public NetworkBehaviourId LastAttacker => _lastAttacker;

        public TankFighterHp(sbyte max)
        {
            _maxValue = max;
            _value = max;
            _lastAttacker = default;
        }
        
        public void RecoverFully()
        {
            _value = _maxValue;
            _lastAttacker = default;
        }

        public void CauseDamage(sbyte value, TankFighter attacker)
        {
            if (value <= 0) return;
            _value = (sbyte)(Mathf.Max(0, _value - value));
            _lastAttacker = attacker;
        }

        public TankFighter? FindLastAttacker(NetworkRunner runner)
        {
            if (_lastAttacker == default) return null;
            if (runner.TryFindBehaviour(_lastAttacker, out var networkBehaviour) == false) return null;
            return networkBehaviour.TryGetComponent<TankFighter>(out var tank) == false ? null : tank;
        }
    }

    public readonly struct TankFighterTeam
    {
        private readonly int _teamId;
        public int TeamId => _teamId;

        public TankFighterTeam(int teamId)
        {
            _teamId = teamId;
        }

        public bool IsSame(TankFighterTeam other)
        {
            return _teamId == other._teamId;
        }
    }

    public readonly struct TankFighterId
    {
        private readonly int _value;
        public int Value => _value;

        public TankFighterId(int value)
        {
            _value = value;
        }

        public bool IsSame(TankFighterId other)
        {
            return _value == other._value;
        }

        public override string ToString()
        {
            return "id: " + _value;
        }
    }

    public enum ETankFighterState
    {
        Immortal,
        Alive,
        Dead
    }
    
}