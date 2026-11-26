using System;
using Unity.Netcode;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace Runtime.Networking.Lobby
{
    public class LobbyManager : MonoBehaviour
    {
        private Unity.Services.Lobbies.Models.Lobby hostLobby;
        private Unity.Services.Lobbies.Models.Lobby joinLobby;
        private Unity.Services.Lobbies.Models.Lobby currentLobby;
        public static LobbyManager Instance;

        public LobbyCache CurrentCache { get; private set; }
        public static event Action OnCloseLobby;
        public static event Action<Unity.Services.Lobbies.Models.Lobby> OnLobbyCreated;
        public static event Action<Unity.Services.Lobbies.Models.Lobby> OnLobbyJoined;

        private bool isQuitting = false;
        public static Action<Unity.Services.Lobbies.Models.Lobby> OnLobbyUpdated;

        private async void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // Attendre que l'init soit complète avant tout
            await LobbyHelper.InitializeClient();

            LobbyUi.OnRequestJoinLobby -= JoinLobby;
            LobbyUi.OnRequestJoinLobby += JoinLobby;

            DontDestroyOnLoad(Instance);
        }

        private void OnDisable()
        {
            LobbyUi.OnRequestJoinLobby -= JoinLobby;
        }

        public async void CreateLobby()
        {
            try
            {
                Debug.Log("Create Lobby");
                CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions()
                {
                    Player = LobbyHelper.GetPlayer()
                };

                var lobby = await LobbyService.Instance.CreateLobbyAsync("lobby", 4, createLobbyOptions);

                hostLobby = lobby;
                currentLobby = lobby;
                CurrentCache = new LobbyCache(currentLobby);
                await LobbyHelper.SetPseudo(String.Empty);
                OnLobbyCreated?.Invoke(hostLobby);
                LobbyHelper.StartLobbyPolling(lobby);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
                Debug.Log(e.Reason);
                throw;
            }
        }

        public async void JoinLobby(string Code)
        {

            if (string.IsNullOrWhiteSpace(Code))
            {
                Debug.LogWarning("Code vide reçu dans JoinLobby");
                return;
            }
            try
            {
                JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions()
                {
                    Player = LobbyHelper.GetPlayer()
                };

                var lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(Code, joinLobbyByCodeOptions);

                joinLobby = lobby;
                currentLobby = lobby;
                CurrentCache = new LobbyCache(currentLobby);
                Debug.Log($"Number of player : " + lobby.Players.Count);
                await LobbyHelper.SetPseudo(String.Empty);
                LobbyHelper.StartLobbyPolling(lobby);
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError(e);
                Debug.LogError(e.Reason);
            }
        }

        private void OnApplicationQuit()
        {
            isQuitting = true;
        }
        public async void CloseLobby()
        {
            if (isQuitting)
                return;

            try
            {
                await NetworkScene.Instance.LoadMenuSceneAsync();

                foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
                {
                    if (clientId != NetworkManager.Singleton.LocalClientId)
                        NetworkManager.Singleton.DisconnectClient(clientId);
                }

                NetworkManager.Singleton.Shutdown();

                await LobbyService.Instance.DeleteLobbyAsync(CurrentCache.LobbyId);

                CurrentCache = null;
                hostLobby = null;
                OnCloseLobby?.Invoke();
            }
            catch (LobbyServiceException e)
            {
                Console.WriteLine(e);
            }
        }

        public async void QuitLobby()
        {
            if (isQuitting)
                return;
            try
            {
                if (CurrentCache.HostId == LobbyHelper.GetPlayerId())
                {
                    CloseLobby();
                    return;
                }

                await NetworkScene.Instance.LoadMenuSceneAsync();

                if (NetworkManager.Singleton.IsClient)
                    NetworkManager.Singleton.Shutdown();

                await LobbyService.Instance.RemovePlayerAsync(
                    CurrentCache.LobbyId,
                    LobbyHelper.GetPlayerId()
                );

                CurrentCache = null;
                LobbyHelper.StopLobbyPolling();
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
                Debug.Log(e.Reason);
            }
        }

        public async void StartGame()
        {
            if (CurrentCache == null) return;
            if (CurrentCache.Lobby.HostId != LobbyHelper.GetPlayerId()) return;

            string joinCode = await LobbyHelper.CreateRelay(4);

            await LobbyHelper.CreateRelayCodeData(joinCode, hostLobby);

            NetworkManager.Singleton.StartHost();
            await NetworkScene.Instance.LoadGameSceneAsync();
        }

        [ContextMenu("DebugAllCache")]
        public void DebugAllLobbyCache()
        {
            if (CurrentCache?.Lobby == null)
            {
                Debug.Log("[LobbyCache] No active lobby.");
                return;
            }

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("=== Lobby Cache ===");

            // Lobby level data
            sb.AppendLine("-- Lobby Data --");
            if (CurrentCache.Lobby.Data != null && CurrentCache.Lobby.Data.Count > 0)
            {
                foreach (var kvp in CurrentCache.Lobby.Data)
                    sb.AppendLine($"  [{kvp.Key}] = {kvp.Value.Value} (Visibility: {kvp.Value.Visibility})");
            }
            else
            {
                sb.AppendLine("  (empty)");
            }

            // Player level data
            sb.AppendLine("-- Players Data --");
            if (CurrentCache.Lobby.Players != null)
            {
                foreach (var player in CurrentCache.Lobby.Players)
                {
                    sb.AppendLine($"  Player: {player.Id}");

                    if (player.Data != null && player.Data.Count > 0)
                    {
                        foreach (var kvp in player.Data)
                            sb.AppendLine($"    [{kvp.Key}] = {kvp.Value.Value} (Visibility: {kvp.Value.Visibility})");
                    }
                    else
                    {
                        sb.AppendLine("    (no data)");
                    }
                }
            }

            sb.AppendLine("===================");
            Debug.Log(sb.ToString());
        }


        void OnDestroy()
        {
            if (CurrentCache != null)
                Instance.QuitLobby();
        }
    }
}