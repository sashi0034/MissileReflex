using Fusion;
using UnityEngine;

namespace MissileReflex.Src.Connection
{
    public struct PlayerInputButton : INetworkStruct
    {
        private byte _buttonFlags;

        public PlayerInputButton(byte buttonFlags)
        {
            _buttonFlags = buttonFlags;
        }

        public const byte BitMouseLeft = 1 << 0;
        public bool IsPushMouseLeft => (_buttonFlags & BitMouseLeft) != 0;
        
        public const byte BitMouseRight = 1 << 1;
        public bool IsPushMouseRight => (_buttonFlags & BitMouseLeft) != 0;
    }
    
    public struct PlayerInputData : INetworkInput
    {
        [Networked] private Vector2 _direction { get; set; }
        public Vector2 Direction => _direction;

        [Networked] private Vector3 _mouseWorldPos { get; set; }
        public Vector3 MouseWorldPos => _mouseWorldPos;
        
        private PlayerInputButton _button { get; set; }
        public PlayerInputButton Button => _button;

        public PlayerInputData(
            Vector2 direction, 
            Vector3 mouseWorldPos, 
            PlayerInputButton button)
        {
            _direction = direction;
            _mouseWorldPos = mouseWorldPos;
            _button = button;
        }
    }
}