using System;

namespace MissileReflex.Src.Utils
{
    public readonly struct FloatRange
    {
        public readonly float Min;
        public readonly float Max;

        public FloatRange(float inclusiveMin, float inclusiveMax)
        {
            Min = inclusiveMin;
            Max = inclusiveMax;
        }

        public bool IsInRange(float value)
        {
            return Min <= value && value <= Max;
        }

        public float FixInRange(float value)
        {
            return Math.Min(Math.Max(value, Min), Max);
        }
    }
}