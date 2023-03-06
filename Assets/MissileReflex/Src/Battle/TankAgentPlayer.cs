#nullable enable

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Fusion;
using MissileReflex.Src.Battle;
using MissileReflex.Src.Connection;
using MissileReflex.Src.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MissileReflex.Src.Battle
{
    [DisallowMultipleComponent]
    public class TankAgentPlayer : NetworkBehaviour, ITankAgent
    {
        private BattleRoot battleRoot => BattleRoot.Instance;

#nullable disable
        [SerializeField] private TankFighter _selfTank;
#nullable enable
        public TankFighter Tank => _selfTank;
        
        private Camera mainCamera => Camera.main;

        
        public void Init(TankSpawnInfo spawnInfo, PlayerRef networkPlayer)
        {
            _selfTank.Init(spawnInfo, networkPlayer);
        }

        public override void FixedUpdateNetwork()
        {
            if (battleRoot.IsSleeping) return;
            if (_selfTank.CancelBattle.IsCancellationRequested) return;
            if (_selfTank.IsOwnerLocalPlayer() && Object.StateAuthority != Runner.LocalPlayer)
            {
                // プレイヤー自身のオブジェクトはStateAuthorityを要求する
                Object.RequestStateAuthority();
            }
            
            GetInput(out PlayerInputData input);
            
            updateInputMove(input);
            
            updateInputShoot(input);

            // カメラ位置調整
            if (Object.HasInputAuthority && _selfTank.IsAlive()) controlCameraPos(input);
        }

#if UNITY_EDITOR
        [Button]
        public void TestShowAuthority()
        {
            Debug.Log($"owner: {_selfTank.OwnerNetworkPlayer}\nstate auth: {Object.StateAuthority}");
        }

        [Button]
        public void TestRequestAuthority()
        {
            Util.RunUniTask(async () =>
            {
                Debug.Log($"state auth before: {Object.StateAuthority}");
                Object.RequestStateAuthority();
                Debug.Log($"state auth after 0s: {Object.StateAuthority}");
                await UniTask.Delay(1000);
                Debug.Log($"state auth after 1s: {Object.StateAuthority}");
            });
        }
#endif
        private void controlCameraPos(PlayerInputData _)
        {
            var cameraTransform = mainCamera.transform;
            var cameraPos = cameraTransform.localPosition;

            const float lerpScale = 20f;

            var targetPos = _selfTank.transform.position.FixY(cameraPos.y);

            cameraTransform.localPosition = Vector3.Lerp(
                cameraPos,
                targetPos,
                Time.deltaTime * lerpScale);
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