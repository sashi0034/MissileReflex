#nullable enable

using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MissileReflex.Src.Params;
using MissileReflex.Src.Storage;
using MissileReflex.Src.Utils;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;

namespace MissileReflex.Src.Lobby.MenuContents
{
    public class SectionPlayerInfo : MonoBehaviour
    {
#nullable disable
        [SerializeField] private LobbyHud lobbyHud;
        private GameRoot gameRoot => lobbyHud.GameRoot;
        
        [SerializeField] private TMP_InputField inputPlayerName;
        public TMP_InputField InputPlayerName => inputPlayerName;

        [SerializeField] private TextMeshProUGUI textPlayerRating;
        public TextMeshProUGUI TextPlayerRating => textPlayerRating;

        [SerializeField] private TextMeshProUGUI textRatingDelta;
        public TextMeshProUGUI TextRatingDelta => textRatingDelta;
        
#nullable enable
        private const int invalidRatingDelta = Int32.MinValue;
        private int _ratingDeltaByLastBattle = invalidRatingDelta;
        public int RatingDeltaByLastBattle => _ratingDeltaByLastBattle;

        [EventFunction]
        private void Start()
        {
            inputPlayerName.onEndEdit.AddListener(onSubmitPlayerName);
        }

        public void SetRatingDeltaByLastBattle(int delta)
        {
            _ratingDeltaByLastBattle = delta;
        }

        public void CleanRestart()
        {
            inputPlayerName.text = gameRoot.SaveData.PlayerName;
            textPlayerRating.text = gameRoot.SaveData.PlayerRating.ToString(); 
            
            checkPerformRatingDelta();
        }

        private void checkPerformRatingDelta()
        {
            bool isShowRatingDelta = _ratingDeltaByLastBattle != invalidRatingDelta;
            textRatingDelta.gameObject.SetActive(isShowRatingDelta);
            
            if (!isShowRatingDelta) return;
            performRatingDelta(gameRoot.SaveData.PlayerRating, _ratingDeltaByLastBattle).RunTaskHandlingError();
            _ratingDeltaByLastBattle = invalidRatingDelta;
        }

#if UNITY_EDITOR
        [Button]
        public void TestPerformRatingDelta(int ratingDelta)
        {
            _ratingDeltaByLastBattle = ratingDelta;
            checkPerformRatingDelta();
        }
#endif


        private void onSubmitPlayerName(string newName)
        {
            // ダメ
            if (newName.IsNullOrWhitespace())
            {
                inputPlayerName.text = gameRoot.SaveData.PlayerName;
                return;
            }
            
            SeManager.Instance.PlaySe(SeManager.Instance.SeSectionTap);

            const int maxPlayerNameLength = 16;
            var newNameCorrected = newName[..Math.Min(newName.Length, maxPlayerNameLength)]
                .Replace("<", "")
                .Replace(">", "");

            string oldName = gameRoot.SaveData.PlayerName;

            inputPlayerName.text = newNameCorrected;
            gameRoot.SaveData.SetPlayerName(newNameCorrected);
            if (lobbyHud.SharedState != null) lobbyHud.SharedState.NotifyPlayerInfoFromSaveData(gameRoot.SaveData);
            gameRoot.WriteSaveData();
            
            lobbyHud.SectionMultiChatRef.PostInfoMessageAuto(
                $"{oldName} が名前を {newNameCorrected} に変更しました");
        }

        private async UniTask performRatingDelta(PlayerRating actualRating, int ratingDelta)
        {
            textPlayerRating.text = (actualRating.Value - ratingDelta).ToString();
            textRatingDelta.text = Util.StringifySignedNumber(ratingDelta);
            textRatingDelta.color = ratingDelta == 0
                ? Util.ColourHex(ConstParam.ColorCodeGamingGreen)
                : ratingDelta < 0
                    ? Util.ColourHex(ConstParam.ColorBluePale)
                    : Util.ColourHex(ConstParam.ColorOrange);

            Util.ActivateAndResetScale(textRatingDelta);

            await UniTask.Delay(0.5f.ToIntMilli());
            await textPlayerRating.transform.DOScale(1.1f, 0.5f).SetEase(Ease.InBack);

            SeManager.Instance.PlaySe(SeManager.Instance.SeRatingChange);
            
            // 変化量をカウントダウン 
            int changingRatingDelta = ratingDelta;
            await DOTween.To(
                () => changingRatingDelta,
                value =>
                {
                    changingRatingDelta = value;
                    textPlayerRating.text = (actualRating.Value - changingRatingDelta).ToString();
                    textRatingDelta.text = Util.StringifySignedNumber(value);
                },
                0,
                1.5f).SetEase(Ease.OutSine);
            await textPlayerRating.transform.DOScale(1.0f, 0.5f).SetEase(Ease.InOutBack,10);
            await UniTask.Delay(0.5f.ToIntMilli());
            await HudUtil.AnimSmallOneToZero(textRatingDelta.transform);

        }
    }
}