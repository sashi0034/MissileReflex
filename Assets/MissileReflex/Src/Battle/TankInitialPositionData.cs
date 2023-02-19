using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MissileReflex.Src.Battle
{
    [Serializable]
    public class TankInitialPositionDataElem
    {
        [SerializeField] private Transform[] posObj;
        public Transform[] PosObj => posObj;
    }
    
    public class TankInitialPositionData : MonoBehaviour
    {
        [SerializeField] private TankInitialPositionDataElem[] teamElements;
        public TankInitialPositionDataElem[] TeamElements => teamElements;
    }

}