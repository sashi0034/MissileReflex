using System;
using MissileReflex.Src.Utils;
using UnityEngine;

namespace MissileReflex.Src.Battle
{
    [Serializable]
    public struct TankFighterCannon
    {
        [SerializeField] private GameObject cannonView;
        [SerializeField] private Animator cannonAnimator;
        [SerializeField] private SkinnedMeshRenderer cannonMesh;

        private static readonly AnimHash hashShot = new AnimHash("shot");
        
        public void ChangeMaterial(Material mat)
        {
            cannonMesh.material = mat;
        }
        
        public void LerpCannonRotation(float deltaTime, Vector3 direction)
        {
            cannonView.transform.localRotation = Quaternion.Euler(
                0,
                Mathf.LerpAngle(
                    cannonView.transform.localRotation.eulerAngles.y,
                    Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg,
                    deltaTime * 20),
                0);
        }

        public void AnimShot()
        {
            cannonAnimator.SetTrigger(hashShot.Code);
        }
    }
}