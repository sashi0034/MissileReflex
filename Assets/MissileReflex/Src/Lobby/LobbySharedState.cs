#nullable enable

using System;
using System.Collections.Generic;
using Fusion;
using MissileReflex.Src.Connection;
using MissileReflex.Src.Params;
using UnityEngine;

namespace MissileReflex.Src.Lobby
{
    public struct LobbyPlayerStatus : INetworkStruct
    {
        private PlayerGeneralInfo _info;
        public PlayerGeneralInfo Info => _info;
        
        private bool _hasLoadedArena;
        public bool HasLoadedArena => _hasLoadedArena;

        public LobbyPlayerStatus RaiseLoadedArena()
        {
            _hasLoadedArena = true;
            return this;
        }

        public LobbyPlayerStatus SetInfo(PlayerGeneralInfo info)
        {
            _info = info;
            return this;
        }

        public LobbyPlayerStatus CleanParams()
        {
            _hasLoadedArena = false;
            return this;
        }
    }

    public class LobbySharedState : NetworkBehaviour
    {
        private static GameRoot gameRoot => GameRoot.Instance;
        private static LobbyHud lobbyHud => gameRoot.LobbyHud;

        [Networked]
        private NetworkDictionary<PlayerRef, LobbyPlayerStatus> playerStatus { get; } =
            MakeInitializer(new Dictionary<PlayerRef, LobbyPlayerStatus>());

        [Networked] private int _matchingRemainingCount { get; set; } = Int32.MaxValue;
        public int MatchingRemainingCount => _matchingRemainingCount;

        [Networked] private NetworkBool _hasEnteredBattle { get; set; } = false;
        public bool HasEnteredBattle => _hasEnteredBattle;

        public override void Spawned()
        {
            transform.parent = lobbyHud.transform;
            lobbyHud.RegisterSharedState(this);
        }
        
        public void Init()
        {
            CleanRestart();
        }

        public void DecRemainingCount()
        {
            _matchingRemainingCount--;
        }

        public void NotifyPlayerInfo(PlayerGeneralInfo info)
        {
            rpcallNotifyPlayerInfo(Runner.LocalPlayer, info);
        }
        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void rpcallNotifyPlayerInfo(PlayerRef player, PlayerGeneralInfo info)
        {
            modifyPlayerStatus(player, status => status.SetInfo(info));
        }
        
        public void NotifyLocalLoadedArena()
        {
            rpcallNotifyLoadedArena(Runner.LocalPlayer);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private void rpcallNotifyLoadedArena(PlayerRef player)
        {
            modifyPlayerStatus(player, flag => flag.RaiseLoadedArena());
        }

        // バトルが終わった後に、同じ部屋で対戦できるように
        public void CleanRestart()
        {
            _hasEnteredBattle = false;
            _matchingRemainingCount = ConstParam.Instance.MatchingTimeLimit;
            
            foreach (var player in Runner.ActivePlayers)
            {
                modifyPlayerStatus(player, status => status.CleanParams());
            }
        }

        public bool IsAllPlayersLoadedArena()
        {
            foreach (var player in Runner.ActivePlayers)
            {
                if (getPlayerStatusOrDefault(player).HasLoadedArena == false) return false;
            }
            return true;
        }

        public void NotifyEnteredBattle()
        {
            _hasEnteredBattle = true;
        }

        public LobbyPlayerStatus GetPlayerStatus(PlayerRef player)
        {
            if (playerStatus.TryGet(player, out var status)) return status;
            Debug.Assert(false);
            return new LobbyPlayerStatus();
        }

        private void modifyPlayerStatus(PlayerRef? nullablePlayer, Func<LobbyPlayerStatus, LobbyPlayerStatus> modifier)
        {
            Debug.Assert(nullablePlayer != null);
            var player = nullablePlayer ?? PlayerRef.None;
            playerStatus.Set(player, modifier(getPlayerStatusOrDefault(player)));
        }

        private LobbyPlayerStatus getPlayerStatusOrDefault(PlayerRef player)
        {
            return playerStatus.ContainsKey(player) ? playerStatus[player] : new LobbyPlayerStatus();
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RpcallPostChatMessage(string playerCaption, string content)
        {
            lobbyHud.SectionMultiChatRef.PostChatMessageLocal(playerCaption, content);
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        public void RpcallPostInfoMessage(string content)
        {
            lobbyHud.SectionMultiChatRef.PostInfoMessageLocal(content);
        }
        
    }
}