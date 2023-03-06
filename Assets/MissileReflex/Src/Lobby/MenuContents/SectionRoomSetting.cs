#nullable enable

using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MissileReflex.Src.Connection;
using MissileReflex.Src.Params;
using MissileReflex.Src.Utils;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace MissileReflex.Src.Lobby.MenuContents
{
    public class SectionRoomSetting : MonoBehaviour
    {
#nullable disable
        [SerializeField] private LobbyHud lobbyHud;

        [SerializeField] private SimpleSlider sliderMatchingSpeed;
        [SerializeField] private SimpleSlider sliderBattleTimeLimit;

        [SerializeField] private GameObject viewNotAvailable;
        public GameObject ViewNotAvailable => viewNotAvailable;
        
#nullable enable
        private LobbySharedState? sharedState => lobbyHud.SharedState;

        [EventFunction]
        private void Awake()
        {
            sliderMatchingSpeed.OnChangeValue.Subscribe(value => { modifyRoomSetting(
                setting => new LobbyRoomSetting(setting) { MatchingSpeed = value }); });
            sliderBattleTimeLimit.OnChangeValue.Subscribe(value => { modifyRoomSetting(
                setting => new LobbyRoomSetting(setting) { BattleTimeLimit = (int)value }); });
        }

        private void modifyRoomSetting(Func<LobbyRoomSetting, LobbyRoomSetting> func)
        {
            if (sharedState == null) return;
            sharedState.ModifyRoomSetting(func);
        }

        public void Init()
        {
            sliderMatchingSpeed.Init(
                ConstParam.MatchingSpeedRange, ConstParam.MatchingSpeedDefault, " 倍速");
            sliderBattleTimeLimit.Init(
                ConstParam.BattleTimeLimitRange.ToRangeF(), ConstParam.BattleTimeLimitDefault, " 秒");
        }

        [EventFunction]
        private void Update()
        {
            ViewNotAvailable.SetActive(sharedState == null || sharedState.Object.HasStateAuthority == false);
            if (sharedState == null) return;

            sliderMatchingSpeed.Slider.value = sharedState.RoomSetting.MatchingSpeed;
            sliderBattleTimeLimit.Slider.value = sharedState.RoomSetting.BattleTimeLimit;
        }
    }
}