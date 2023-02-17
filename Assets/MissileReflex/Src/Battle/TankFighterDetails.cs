#nullable enable

using System.Collections.Generic;
using MissileReflex.Src.Params;
using MissileReflex.Src.Utils;
using UnityEngine;

namespace MissileReflex.Src.Battle
{
    public class TankFighterInput
    {
        private Vector3 _moveVec = Vector3.zero;
        public Vector3 MoveVec => _moveVec;

        private float _shotRad = 0;
        public float ShotRad => _shotRad;
        
        private readonly BoolFlag _shotRequest = new BoolFlag();
        public BoolFlag ShotRequest => _shotRequest;

        public void Init()
        {
            _moveVec = Vector3.zero;
            _shotRad = 0;
            _shotRequest.Clear();
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

    public class TankFighterHp
    {
        private float _maxValue = 0;
        private float _value = 0;
        public float Value => _value;
        private TankFighter? _lastAttacker;
        public TankFighter? LastAttacker => _lastAttacker;

        public void Init(float maxValue)
        {
            _maxValue = maxValue;
            _value = maxValue;
            _lastAttacker = null;
        }

        public void RecoverFully()
        {
            _value = _maxValue;
            _lastAttacker = null;
        }

        public void CauseDamage(float value, TankFighter attacker)
        {
            if (value <= 0) return;
            _value = Mathf.Max(0, _value - value);
            _lastAttacker = attacker;
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