#nullable enable

using Fusion;

namespace MissileReflex.Src
{
    public struct PlayerGeneralInfo : INetworkStruct
    {
        private PlayerRating _rating;
        public PlayerRating Rating => _rating;
        
        private NetworkString<_16> _name;
        public string Name => _name.Value;
        
        public PlayerGeneralInfo(PlayerRating rating, string name)
        {
            _rating = rating;
            _name = name;
        }
    }
}