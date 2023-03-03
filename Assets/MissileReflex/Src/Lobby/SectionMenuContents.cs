﻿#nullable enable

using System;
using MissileReflex.Src.Lobby.MenuContents;
using MissileReflex.Src.Utils;
using UnityEngine;

namespace MissileReflex.Src.Lobby
{
    public class SectionMenuContents : MonoBehaviour
    {
#nullable disable
        [SerializeField] private SectionPlayerInfo sectionPlayerInfo;
        public SectionPlayerInfo SectionPlayerInfo => sectionPlayerInfo;

        [SerializeField] private SectionMultiChat sectionMultiChat;
        public SectionMultiChat SectionMultiChat => sectionMultiChat;
        
        [SerializeField] private SectionOfflineBattle sectionOfflineBattle;
        public SectionOfflineBattle SectionOfflineBattle => sectionOfflineBattle;
        
        [SerializeField] private SectionHelp sectionHelp;
        public SectionHelp SectionHelp => sectionHelp;
#nullable enable
        
        public MonoBehaviour[] ListSections()
        {
            return new MonoBehaviour[]
            {
                sectionPlayerInfo,
                sectionMultiChat,
                sectionOfflineBattle,
                sectionHelp,
            };
        }

        public void Init()
        {
            foreach (var section in ListSections())
            {
                Util.DeactivateGameObjects(section);
            }
        }

        public void CleanRestart()
        {
            sectionPlayerInfo.CleanRestart();
        }
    }
}