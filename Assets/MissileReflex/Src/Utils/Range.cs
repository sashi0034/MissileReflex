#nullable enable

using System;

namespace MissileReflex.Src.Utils
{
    public class RangeF
    {
        public readonly float Min;
        public readonly float Max;

        public RangeF(float inclusiveMin, float inclusiveMax)
        {
            Min = inclusiveMin;
            Max = inclusiveMax;
        }

        public bool IsInRange(float value)
        {
            return Min <= value && value <= Max;
        }

        public float Clamp(float value)
        {
            return Math.Min(Math.Max(value, Min), Max);
        }
    }
    
    public class RangeInt
    {
        public readonly int Min;
        public readonly int Max;

        public RangeInt(int inclusiveMin, int inclusiveMax)
        {
            Min = inclusiveMin;
            Max = inclusiveMax;
        }

        public bool IsInRange(int value)
        {
            return Min <= value && value <= Max;
        }

        public int Clamp(int value)
        {
            return Math.Min(Math.Max(value, Min), Max);
        }

        public RangeF ToRangeF()
        {
            return new RangeF(Min, Max);
        }
    }
}