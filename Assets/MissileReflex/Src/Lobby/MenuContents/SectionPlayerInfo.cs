#nullable enable

using Cysharp.Threading.Tasks;
using MissileReflex.Src.Utils;
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

        private async UniTask performRatingDelta()
        {
            // TODO
        }
    }
}