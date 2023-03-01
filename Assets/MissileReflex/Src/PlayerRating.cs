#nullable enable

using Fusion;

namespace MissileReflex.Src
{
    public struct PlayerRating : INetworkStruct
    {
        private const int invalidValue = -1;
        private int _value;
        public int Value => _value;
        
        public PlayerRating(int value)
        {
            _value = value;
        }

        public static readonly PlayerRating InvalidRating = new PlayerRating(invalidValue);

        public bool IsValid()
        {
            return _value != invalidValue;
        }
    }
}