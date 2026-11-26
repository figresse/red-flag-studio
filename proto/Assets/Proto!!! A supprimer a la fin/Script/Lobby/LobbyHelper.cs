using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace Runtime.Networking.Lobby
{
    public static class LobbyHelper
    {
        public static string UID;
        public static string PlayerName;
        private static CancellationTokenSource _lobbyPollCts;

        public static async Task InitializeClient()
        {
            if (UnityServices.State == ServicesInitializationState.Initialized) return;

            await UnityServices.InitializeAsync();

            AuthenticationService.Instance.SignedIn += () =>
            {
                UID = AuthenticationService.Instance.PlayerId;
            };

            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        public static async Task SetPseudo(string pseudo)
        {
            if (LobbyManager.Instance.CurrentCache?.Lobby == null)
            {
                Debug.LogWarning("[LobbyHelper] No active lobby.");
                return;
            }

            PlayerName = string.IsNullOrEmpty(pseudo)
                ? "Player " + (LobbyManager.Instance.CurrentCache.Lobby.Players.FindIndex(p => p.Id == GetPlayerId()) + 1)
                : pseudo;

            try
            {
                await LobbyManager.Instance.CurrentCache.SetPlayerData(
                    GetPlayerId(),
                    "PlayerName",
                    PlayerName
                );

                // Manually patch local player data so it's visible before next poll
                var lobby = LobbyManager.Instance.CurrentCache.Lobby;
                foreach (var player in lobby.Players)
                {
                    if (player.Id != GetPlayerId()) continue;
                    if (player.Data == null)
                        player.Data = new Dictionary<string, PlayerDataObject>();

                    player.Data["PlayerName"] = new PlayerDataObject(
                        PlayerDataObject.VisibilityOptions.Member, PlayerName);
                    break;
                }

                Debug.Log($"[LobbyHelper] Pseudo set to '{PlayerName}'");
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"[LobbyHelper] Failed to set pseudo: {e.Message}");
            }
        }

        public static string GetPseudo(string playerId)
        {
            if (LobbyManager.Instance.CurrentCache == null)
            {
                Debug.LogWarning("[LobbyHelper] No active lobby.");
                return null;
            }

            LobbyManager.Instance.CurrentCache.TryGetPlayerData(playerId, "PlayerName", out string pseudo);
            return pseudo;
        }

        public static string GetMyPseudo() => GetPseudo(GetPlayerId());

        public static Player GetPlayer()
        {
            return new Player()
            {
                Data = new Dictionary<string, PlayerDataObject>()
                {
                    {
                        "PlayerName",
                        new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, PlayerName)
                    }
                }
            };
        }

        public static void StartLobbyPolling(Unity.Services.Lobbies.Models.Lobby lobby)
        {
            StopLobbyPolling();

            _lobbyPollCts = new CancellationTokenSource();
            _ = LobbyPollAsync(lobby, _lobbyPollCts.Token);
        }

        public static void StopLobbyPolling()
        {
            if (_lobbyPollCts != null)
            {
                _lobbyPollCts.Cancel();
                _lobbyPollCts.Dispose();
                _lobbyPollCts = null;
            }
        }

        private static async Task LobbyPollAsync(Unity.Services.Lobbies.Models.Lobby currentLobby,
            CancellationToken cancellationToken)
        {
            while (currentLobby != null)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    currentLobby = await LobbyService.Instance.GetLobbyAsync(currentLobby.Id);
                    if (LobbyManager.Instance.CurrentCache != null)
                    {
                        LobbyManager.Instance.CurrentCache.UpdateLobby(currentLobby);
                    }

                    LobbyManager.OnLobbyUpdated?.Invoke(currentLobby);

                    if (LobbyManager.Instance.CurrentCache != null &&
                        LobbyManager.Instance.CurrentCache.TryGet("RelayCode", out string code))
                    {
                        StopLobbyPolling();
                        await JoinGameFlow(code);
                        return;
                    }
                }
                catch (OperationCanceledException)
                {
                    Debug.Log("Lobby polling cancelled.");
                    return;
                }
                catch (LobbyServiceException e)
                {
                    if (e.Reason == LobbyExceptionReason.LobbyNotFound)
                    {
                        Debug.Log("Lobby was closed by host.");
                        return;
                    }
                    else
                    {
                        Debug.Log(e);
                        throw;
                    }
                }

                await Task.Delay(1000, cancellationToken);
            }
        }

        private static async Task JoinGameFlow(string code)
        {
            while (NetworkManager.Singleton == null)
                await Task.Yield();

            await Task.Delay(200);

            if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsHost)
                return;

            try
            {
                await JoinRelay(code);
            }
            catch (Exception e)
            {
                Debug.LogError("Relay join failed: " + e.Message);
                return;
            }

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            Debug.Log("Relay mode check: " + transport.ConnectionData.Address);

            // Wait one frame after set data
            await Task.Yield();

            NetworkManager.Singleton.StartClient();
        }

        private static void OnDestroy()
        {
            StopLobbyPolling();
        }

        public static string GetPlayerId()
        {
            return AuthenticationService.Instance.PlayerId;
        }

        public static async Task<string> CreateRelay(int maxPlayers = 4)
        {
            try
            {
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);
                var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

                // ? SetHostRelayData instead of RelayServerData constructor
                transport.SetRelayServerData(
                    allocation.RelayServer.IpV4,
                    (ushort)allocation.RelayServer.Port,
                    allocation.AllocationIdBytes,
                    allocation.Key,
                    allocation.ConnectionData,
                    allocation.ConnectionData
                );

                string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
                Debug.Log("Relay Host created. JoinCode = " + joinCode);

                return joinCode;
            }
            catch
            {
                Debug.Log("Can't Create Allocation");
                throw;
            }
        }

        public static async Task JoinRelay(string joinCode)
        {
            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);


            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

            // ? SetHostRelayData instead of RelayServerData constructor
            transport.SetRelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData,
                allocation.HostConnectionData
            );
        }

        public static async Task CreateRelayCodeData(string joinCode, Unity.Services.Lobbies.Models.Lobby hostLobby)
        {
            await LobbyService.Instance.UpdateLobbyAsync(hostLobby.Id,
                new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        {
                            "RelayCode",
                            new DataObject(DataObject.VisibilityOptions.Member, joinCode)
                        }
                    }
                });
        }

        public static List<UserData> GetPlayerData()
        {
            var players = LobbyManager.Instance.CurrentCache.GetAllPlayer();

            List<UserData> UserDatas = new List<UserData>();

            for (int i = 0; i < players.Count; i++)
            {
                UserDatas.Add(new UserData()
                {
                    PlayerLobbyID = players[i].Id,
                    Username = players[i].Data["PlayerName"].Value
                });
            }

            return UserDatas;
        }
    }
}