#nullable enable

using System;
using MissileReflex.Src.Battle;
using MissileReflex.Src.Utils;
using UnityEngine;

namespace MissileReflex.Src.Battle
{
    [DisallowMultipleComponent]
    public class Player : MonoBehaviour, ITankAgent
    {
        [SerializeField] private BattleRoot battleRoot;
        public BattleRoot BattleRoot => battleRoot;

        [SerializeField] private TankFighter _selfTank;
        public TankFighter Tank => _selfTank;
        
        private Camera mainCamera => Camera.main;


        public void Init()
        {
            _selfTank.Init(this, null);
        }

        [EventFunction]
        private void Update()
        {
            updateInputMove();
            
            updateInputShoot();

            // カメラ位置調整
            mainCamera.transform.position = _selfTank.transform.position.FixY(mainCamera.transform.position.y);
        }

        private void updateInputMove()
        {
            var inputVec = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;

            _selfTank.Input.SetMoveVec(inputVec);
        }

        private void updateInputShoot()
        {
            var playerPos = _selfTank.transform.position;
            var distancePlayerCamera = Vector3.Distance(playerPos, mainCamera.transform.position); 
            var mousePos = Input.mousePosition.FixZ(distancePlayerCamera);
            var worldMousePos = mainCamera.ScreenToWorldPoint(mousePos);

            var shotDirection = worldMousePos - playerPos;
            
            _selfTank.Input.SetShotRadFromVec3(shotDirection);

            if (Input.GetMouseButtonDown(0)) _selfTank.Input.ShotRequest.UpFlag();
        }


        [EventFunction]
        private void FixedUpdate()
        { }

        [EventFunction]
        private void OnCollisionEnter(Collision collision)
        {}
    }
}