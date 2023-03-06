#nullable enable

using System;
using Michsky.MUIP;
using MissileReflex.Src.Utils;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace MissileReflex.Src.Lobby
{
    public class SimpleSlider : MonoBehaviour
    {
#nullable disable
        [SerializeField] private SliderManager sliderManager;
        public Slider Slider => sliderManager.mainSlider;

        [SerializeField] private TextMeshProUGUI text;
        public TextMeshProUGUI Text => text;

        [SerializeField] private string textFormat = "F1";
#nullable enable
        private string _textSuffix = "";
        private readonly Subject<float> _onChangeValue = new();
        public IObservable<float> OnChangeValue => _onChangeValue;
        

        [EventFunction]
        public void OnValueChanged()
        {
            float value = Slider.value;
            updateView(value);
            _onChangeValue.OnNext(value);
        }

        private void updateView(float value)
        {
            text.text = value.ToString(textFormat) + _textSuffix;
        }

        public void Init(
            RangeF valueRange,
            float initialValue,
            string suffix)
        {
            _textSuffix = suffix;

            Slider.minValue = valueRange.Min;
            Slider.maxValue = valueRange.Max;
            Slider.value = initialValue;
        }
    }
}