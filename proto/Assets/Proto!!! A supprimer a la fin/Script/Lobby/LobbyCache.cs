using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

namespace Runtime.Networking.Lobby
{
    [Serializable]
    public class LobbyCache
    {
        private Unity.Services.Lobbies.Models.Lobby _lobby;

        public Unity.Services.Lobbies.Models.Lobby Lobby => _lobby;
        public string LobbyId => _lobby?.Id;
        public string HostId => _lobby?.HostId;
        public bool IsValid => _lobby != null;

        public LobbyCache(Unity.Services.Lobbies.Models.Lobby lobby)
        {
            _lobby = lobby;
        }

        public void UpdateLobby(Unity.Services.Lobbies.Models.Lobby lobby)
        {
            _lobby = lobby;
        }

        public bool TryGet<T>(string key, out T value)
        {
            if (_lobby?.Data != null && _lobby.Data.TryGetValue(key, out var dataObject))
            {
                if (dataObject.Value is T typed)
                {
                    value = typed;
                    return true;
                }

                try
                {
                    value = (T)Convert.ChangeType(dataObject.Value, typeof(T));
                    return true;
                }
                catch
                {
                    value = default;
                    return false;
                }
            }

            value = default;
            return false;
        }

        public async Task Set<T>(string key, T value,
            DataObject.VisibilityOptions visibility = DataObject.VisibilityOptions.Member)
        {
            if (_lobby == null) return;

            var updated = await LobbyService.Instance.UpdateLobbyAsync(_lobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { key, new DataObject(visibility, value.ToString()) }
                }
            });

            _lobby = updated;
        }

        public T Get<T>(string key)
        {
            if (TryGet<T>(key, out var value))
                return value;

            throw new System.Collections.Generic.KeyNotFoundException(
                $"Key '{key}' not found in lobby data.");
        }

        public void Clear() => _lobby = null;

        public async Task SetPlayerData(string playerId, string key, string value,
            PlayerDataObject.VisibilityOptions visibility = PlayerDataObject.VisibilityOptions.Member)
        {
            if (_lobby == null) return;

            await LobbyService.Instance.UpdatePlayerAsync(_lobby.Id, playerId, new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>
                {
                    { key, new PlayerDataObject(visibility, value) }
                }
            });
        }

        public bool TryGetPlayerData(string playerId, string key, out string value)
        {
            if (_lobby?.Players == null)
            {
                value = null;
                return false;
            }

            foreach (var player in _lobby.Players)
            {
                if (player.Id != playerId) continue;

                if (player.Data != null && player.Data.TryGetValue(key, out var data))
                {
                    value = data.Value;
                    return true;
                }
            }

            value = null;
            return false;
        }

        public List<Player> GetAllPlayer()
        {
            if (_lobby == null)
                return new List<Player>();

            return _lobby.Players;
        }
    }
}