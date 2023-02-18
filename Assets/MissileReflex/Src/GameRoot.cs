using System;
using MissileReflex.Src.Utils;
using UnityEngine;

namespace MissileReflex.Src
{
    public class GameRoot : MonoBehaviour
    {
        [EventFunction]
        private void Start()
        {
#if !UNITY_EDITOR
            Application.targetFrameRate = 60;
#endif
        }
    }
}