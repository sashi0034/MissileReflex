#nullable enable

using System;
using Cysharp.Threading.Tasks;
using Fusion;
using MissileReflex.Src.Params;
using MissileReflex.Src.Utils;
using Sirenix.Utilities;
using TMPro;
using UniRx;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MissileReflex.Src.Lobby.MenuContents
{
    public class SectionMultiChat : MonoBehaviour
    {
#nullable disable
        [SerializeField] private LobbyHud lobbyHud;
        private GameRoot gameRoot => lobbyHud.GameRoot;

        [SerializeField] private LabelPostedContent labelPostedContentPrefab;
        public LabelPostedContent LabelPostedContentPrefab => labelPostedContentPrefab;

        [SerializeField] private VerticalLayoutGroup scrollContent;
        public VerticalLayoutGroup ScrollContent => scrollContent;

        [SerializeField] private ScrollRect scrollView;
        public ScrollRect ScrollView => scrollView;

        [SerializeField] private PanelInputChatContent panelInputChatContent;
        public PanelInputChatContent PanelInputChatContent => panelInputChatContent;
        
#nullable enable
        
        private int _numPosted = 0;
        private IDisposable? _subscribedOnSubmit;
        

        public void Init()
        {
            _numPosted = 0;
            foreach (var child in scrollContent.transform.GetChildren())
            {
                Util.DestroyGameObject(child.gameObject);
            }

            _subscribedOnSubmit?.Dispose();
            _subscribedOnSubmit = panelInputChatContent.OnSubmitInput.Subscribe(input =>
            {
                if (input.IsNullOrWhitespace()) return;
                // ローカルプレイヤーのチャット送信
                PostChatMessageAuto(
                    stringifyLocalPlayerCaption(), 
                    input);
                panelInputChatContent.CleanInputContent();
            });
        }

        [EventFunction]
        private void OnEnable()
        {
#if UNITY_EDITOR
            if (EditorApplication.isPlaying == false) return;
#endif
            // 非表示から表示させるとたまにレイアウトが崩れるので再構築
            rebuildScrollView();
        }

        private void rebuildScrollView()
        {
            Util.CallDelayedAfterFrame(async () =>
            {
                // シーン移動して新しいほうがすぐ破棄される場合があるので
                if (gameObject == null) return;
                
                LayoutRebuilder.MarkLayoutForRebuild(scrollContent.GetComponent<RectTransform>());
                await UniTask.DelayFrame(0);
                // スクロール位置更新
                scrollView.normalizedPosition = new Vector2(0, 0);
            });
        }

        private string stringifyLocalPlayerCaption()
        {
            return $"{gameRoot.SaveData.PlayerName} (<u><i>{gameRoot.SaveData.PlayerRating}</u></i>)";
        }
        
        public void PostChatMessageAuto(string playerCaption, string content)
        {
            if (lobbyHud.SharedState != null)
                lobbyHud.SharedState.RpcallPostChatMessage(playerCaption, content);
            else
                PostChatMessageLocal(playerCaption, content);
        }
        
        public void PostInfoMessageAuto(string content)
        {
            if (lobbyHud.SharedState != null)
                lobbyHud.SharedState.RpcallPostInfoMessage(content);
            else
                PostInfoMessageLocal(content);
        }
        
        public void PostChatMessageLocal(string playerCaption, string content)
        {
            var newContent = appendNewPostedContent();
            newContent.TextCaption.text = playerCaption + $" <color=black>{DateTime.Now:yyyy/MMM/dd hh:mm:ss}";
            newContent.TextContent.text = content;
        }

        public void PostInfoMessageLocal(string content)
        {
            var newContent = appendNewPostedContent();
            newContent.TextCaption.color = Util.ColourHex(ConstParam.ColorOrange);
            newContent.TextCaption.text = "<u>Info</u>" + $" <color=black>{DateTime.Now:yyyy/MMM/dd hh:mm:ss}";
            newContent.TextCaption.fontStyle = FontStyles.Italic;
            newContent.TextContent.text = $"{content}";
            newContent.TextContent.fontStyle = FontStyles.Italic;
        }

        private LabelPostedContent appendNewPostedContent()
        {
            _numPosted++;
            
            var newContent = Instantiate(labelPostedContentPrefab, scrollContent.transform);
            newContent.TextIndex.text = _numPosted + ":";

            const int maxScrollContents = 100;
            if (scrollContent.transform.childCount > maxScrollContents) 
                Util.DestroyGameObject(scrollContent.transform.GetChild(0).gameObject);
            
            rebuildScrollView();

            return newContent;
        }
    }
}