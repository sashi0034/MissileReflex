#nullable enable

using System;
using Fusion;
using MissileReflex.Src.Battle;
using MissileReflex.Src.Connection;
using MissileReflex.Src.Utils;
using UnityEngine;

namespace MissileReflex.Src.Battle
{
    [DisallowMultipleComponent]
    public class TankAgentPlayer : NetworkBehaviour, ITankAgent
    {
        private BattleContext BattleContext => BattleContext.Instance;

        [SerializeField] private TankFighter _selfTank;
        public TankFighter Tank => _selfTank;
        
        private Camera mainCamera => Camera.main;

        
        public void Init(TankSpawnInfo spawnInfo, PlayerRef networkPlayer)
        {
            _selfTank.Init(spawnInfo, networkPlayer);
        }

        public override void FixedUpdateNetwork()
        {
            GetInput(out PlayerInputData input);
            
            updateInputMove(input);
            
            updateInputShoot(input);

            // カメラ位置調整
            if (Object.HasInputAuthority && _selfTank.IsAlive()) 
                mainCamera.transform.localPosition = _selfTank.transform.position.FixY(mainCamera.transform.localPosition.y);
        }

        private void updateInputMove(PlayerInputData input)
        {
            var inputVec = new Vector3(input.Direction.x, 0, input.Direction.y).normalized;

            _selfTank.Input.SetMoveVec(inputVec);
        }

        private void updateInputShoot(PlayerInputData input)
        {
            var playerPos = _selfTank.transform.position;
            var mouseWorldPos = input.MouseWorldPos;

            var shotDirection = mouseWorldPos - playerPos;
            
            _selfTank.Input.SetShotRadFromVec3(shotDirection);

            if (input.Button.IsPushMouseLeft) _selfTank.Input.MakeShotRequest();
        }


        [EventFunction]
        private void FixedUpdate()
        { }

        [EventFunction]
        private void OnCollisionEnter(Collision collision)
        {}
    }
}