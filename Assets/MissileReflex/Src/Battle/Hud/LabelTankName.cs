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
        [SerializeField] private RectTransform selfRect;
        public TextMeshProUGUI TextMesh => textMesh;

        private TankFighter? _followingTank;

        public void RegisterTank(TankFighter tank)
        {
            _followingTank = tank;
            textMesh.text = tank.TankName;
        }

        public void Update()
        {
            if (_followingTank == null)
            {
                Util.DestroyGameObject(gameObject);
                return;
            }

            selfRect.position =
                RectTransformUtility.WorldToScreenPoint(Camera.main, _followingTank.transform.position);
        }
    }
}