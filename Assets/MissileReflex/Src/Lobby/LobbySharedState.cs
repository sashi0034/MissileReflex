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
        private bool _hasLoadedArena;
        public bool HasLoadedArena => _hasLoadedArena;

        public LobbyPlayerStatus RaiseLoadedArena()
        {
            _hasLoadedArena = true;
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

        public override void Spawned()
        {
            transform.parent = lobbyHud.transform;
            lobbyHud.RegisterSharedState(this);
        }
        
        public void Init()
        {
            _matchingRemainingCount = ConstParam.Instance.MatchingTimeLimit;
        }

        public void DecRemainingCount()
        {
            _matchingRemainingCount--;
        }

        public void NotifyLocalLoadedArena()
        {
            rpcallNotifyLoadedArena(Runner.LocalPlayer);
        }

        [Rpc(RpcSources.All, RpcTargets.All)]
        private void rpcallNotifyLoadedArena(PlayerRef player)
        {
            modifyPlayerStatus(player, flag => flag.RaiseLoadedArena());
        }

        public bool IsAllPlayersLoadedArena()
        {
            foreach (var player in Runner.ActivePlayers)
            {
                if (getPlayerStatusOrDefault(player).HasLoadedArena == false) return false;
            }
            return true;
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
    }
}