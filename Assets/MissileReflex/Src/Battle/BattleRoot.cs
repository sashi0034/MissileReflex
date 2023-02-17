﻿using System;
using System.Collections.Generic;
using System.Threading;
using Fusion;
using MissileReflex.Src.Params;
using UnityEngine;

namespace MissileReflex.Src.Battle
{
    [DisallowMultipleComponent]
    public class BattleRoot : MonoBehaviour
    {
        private static BattleRoot _instance;
        public static BattleRoot Instance => _instance;
        
        [SerializeField] private MissileManager missileManager;
        public MissileManager MissileManager => missileManager;

        [SerializeField] private TankManager tankManager;
        public TankManager TankManager => tankManager;

        private CancellationTokenSource _cancelBattle = new CancellationTokenSource();
        public CancellationToken CancelBattle => _cancelBattle.Token;

        public BattleRoot()
        {
            Debug.Assert(_instance == null);
            _instance = this;
        }

        public void Awake()
        {
            Init();
        }

        public void Init()
        {
            missileManager.Init();
            tankManager.Init();
        }
    }
}