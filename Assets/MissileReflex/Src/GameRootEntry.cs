#nullable enable

using System;
using MissileReflex.Src.Utils;
using UnityEngine;

namespace MissileReflex.Src
{
    public class GameRootEntry : MonoBehaviour
    {
#nullable disable
        [SerializeField] private GameRoot gameRoot;
        public GameRoot GameRoot => gameRoot;
#nullable enable
        
        [EventFunction]
        private void Start()
        {
            if (GameRoot.Instance == null) Instantiate(gameRoot);
        }
    }
}