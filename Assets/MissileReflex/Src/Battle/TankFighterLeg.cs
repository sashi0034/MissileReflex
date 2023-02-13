using System;
using MissileReflex.Src.Utils;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;

namespace MissileReflex.Src.Battle
{
    [Serializable]
    public struct TankFighterLeg
    {
        [SerializeField] private GameObject legView;
        [SerializeField] private Animator legAnimator;
        [SerializeField] private SkinnedMeshRenderer legMesh;
        
        private static readonly AnimHash hashRun = new AnimHash("run");

        public void ChangeMaterial(Material mat)
        {
            legMesh.material = mat;
        }

        public void AnimRun(bool hasInput)
        {
            legAnimator.SetBool(hashRun.Code, hasInput);
        }

        public float GetLegRotRadY()
        {
            return (90 - legView.transform.rotation.eulerAngles.y) * Mathf.Deg2Rad;
        }
        
        public void LerpLegRotation(float deltaTime, Vector3 inputVec)
        {
            legView.transform.localRotation = Quaternion.Euler(
                0,
                Mathf.LerpAngle(
                    legView.transform.localRotation.eulerAngles.y,
                    Mathf.Atan2(inputVec.x, inputVec.z) * Mathf.Rad2Deg,
                    deltaTime * 10),
                0);
        }
    }
}