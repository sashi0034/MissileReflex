#nullable enable

using System;
using MissileReflex.Src.Utils;
using TMPro;
using UnityEngine;

namespace MissileReflex.Src.Battle.Hud
{
    public class LabelTankName : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI textMesh;
        public TextMeshProUGUI TextMesh => textMesh;

        private TankFighter? _followingTank;

        public void RegisterTank(TankFighter tank)
        {
            _followingTank = tank;
        }

        public void Update()
        {
            if (_followingTank == null)
            {
                Util.DestroyGameObject(gameObject);
                return;
            }
            
        }
    }
}