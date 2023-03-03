#nullable enable

using System;
using Cysharp.Threading.Tasks;
using MissileReflex.Src.Storage;
using MissileReflex.Src.Utils;
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
        private int ratingDeltaByLastBattle = 0;
        public int RatingDeltaByLastBattle => ratingDeltaByLastBattle;

        [EventFunction]
        private void Start()
        {
            inputPlayerName.onEndEdit.AddListener(onSubmitPlayerName);
        }

        public void SetRatingDeltaByLastBattle(int delta)
        {
            ratingDeltaByLastBattle = delta;
        }

        public void CleanRestart()
        {
            inputPlayerName.text = gameRoot.SaveData.PlayerName;
            textPlayerRating.text = gameRoot.SaveData.PlayerRating.ToString(); 
            
            bool isShowRatingDelta = ratingDeltaByLastBattle != 0;
            textRatingDelta.gameObject.SetActive(isShowRatingDelta);
            if (isShowRatingDelta) performRatingDelta().RunTaskHandlingError();
        }

        private void onSubmitPlayerName(string newName)
        {
            // ダメ
            if (newName.IsNullOrWhitespace())
            {
                inputPlayerName.text = gameRoot.SaveData.PlayerName;
                return;
            }

            const int maxPlayerNameLength = 16;
            var newNameCorrected = newName[..Math.Min(newName.Length, maxPlayerNameLength)]
                .Replace("<", "")
                .Replace(">", "");

            string oldName = gameRoot.SaveData.PlayerName;

            inputPlayerName.text = newNameCorrected;
            gameRoot.SaveData.SetPlayerName(newNameCorrected);
            gameRoot.WriteSaveData();
            
            lobbyHud.SectionMultiChatRef.PostInfoMessageAuto(
                $"{oldName} が名前を {newNameCorrected} に変更しました");
        }

        private async UniTask performRatingDelta()
        {
            // TODO
        }
    }
}