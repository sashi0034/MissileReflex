#nullable enable

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MissileReflex.Src.Params;
using MissileReflex.Src.Utils;
using UnityEngine;

namespace MissileReflex.Src.Battle
{
    public class TankSpawnSymbol : MonoBehaviour
    {
#nullable disable
        [SerializeField] private Material matStoppingColor;
        [SerializeField] private GameObject rotatingCube;
        [SerializeField] private MeshRenderer rotatingCubeMesh;
#nullable enable
        
        private TankFighter? _targetFighter;
        private float _currRotating = 0;
        private float _rotatingSpeed = defaultRotatingSpeed;
        private const float defaultRotatingSpeed = 180;
        
        public void Init(TankFighter fighter)
        {
            rotatingCube.gameObject.SetActive(true);
            _targetFighter = fighter;

            changeRotCubeMatToTeamColor(fighter);
        }

        private void changeRotCubeMatToTeamColor(TankFighter fighter)
        {
            rotatingCubeMesh.sharedMaterial = ConstParam.Instance.MatTeamColorMetal[fighter.Team.TeamId];
        }

        [EventFunction]
        private void Update()
        {
            if (_targetFighter == null)
            {
                rotatingCube.gameObject.SetActive(false);
                return;
            }

            updateRotatingCube(Time.deltaTime);
        }

        private void updateRotatingCube(float deltaTime)
        {
            _currRotating += _rotatingSpeed * deltaTime;
            while (_currRotating > 360) _currRotating -= 360;
            rotatingCube.transform.rotation = Quaternion.Euler(0, _currRotating, 0);
        }

        public void AnimWither()
        {
            _rotatingSpeed = 0;
            rotatingCubeMesh.sharedMaterial = matStoppingColor;
        }

        public async UniTask AnimRespawn(CancellationToken token)
        {
            Debug.Assert(_targetFighter != null);
            changeRotCubeMatToTeamColor(_targetFighter);
            
            // 高速回転して
            await DOTween.To(
                () => _rotatingSpeed,
                value => _rotatingSpeed = value,
                defaultRotatingSpeed * 10,
                2.0f).SetEase(Ease.OutBack);
            await UniTask.Delay(3.0f.ToIntMilli(), cancellationToken: token);
            
            // 落ち着く
            await DOTween.To(
                () => _rotatingSpeed,
                value => _rotatingSpeed = value,
                defaultRotatingSpeed,
                0.5f).SetEase(Ease.OutBack);
        }
    }
}