using UnityEngine;
using System.Linq;
using Unity.Netcode;
using System.Threading.Tasks;
using System.Collections.Generic;
using Runtime.Networking.Lobby;

public class GameInitializer : NetworkBehaviour
{

    [SerializeField]
    NetworkObject playerPrefab;
    [SerializeField]
    private float diameter;

    public List<ulong> playerList = new();
    private int _playerLoadedOnScene = 0;


    /*private void Start()
    {
        if (NetworkManager.Singleton.IsHost)
            IncrementPlayerLoaded();
        else
        {
            NotifyWhenServerInit();
        }
    }*/

    private async void NotifyWhenServerInit()
    {
        while (true)
        {
            if (NetworkManager.Singleton == null)
            {
                await Task.Yield();
                continue;
            }

            try
            {
                if (NetworkManager.Singleton.IsClient)
                {
                    NotifyLoadCompleteRpc();
                    break;
                }

                await Task.Yield();
            }
            catch (System.Exception)
            {
                // server has not been instancied yet
                await Task.Yield();
            }
        }

        enabled = false;
    }

    private void IncrementPlayerLoaded()
    {
        _playerLoadedOnScene++;
        var playersToWaitFor = LobbyManager.Instance.CurrentCache.Lobby.Players.Count;
        Debug.Log($"Player loaded {playersToWaitFor}");
        if (_playerLoadedOnScene >= playersToWaitFor)
            InitGame();
    }

    [ContextMenu("TestInit")]
    public void InitGame()
    {
        if (!NetworkManager.IsHost) return;
        playerList = new(NetworkManager.ConnectedClientsIds);

        float angle = 360 / NetworkManager.Singleton.ConnectedClientsIds.Count;
        for (int i = 0; i < playerList.Count; i++)/* SpawnPlayer Network object*/
        {
            NetworkObject player = Instantiate(playerPrefab, transform.position + (transform.forward * diameter), Quaternion.AngleAxis((i + 1) * angle, transform.up));

            player.transform.LookAt(transform.position);
            transform.SetPositionAndRotation(new Vector3(0, 2f, 0), Quaternion.AngleAxis((i + 1) * angle, transform.up));
            player.SpawnAsPlayerObject(NetworkManager.Singleton.ConnectedClientsIds[i]);
        }
       //GameMode.Instance.InitGame(playerList);
    }

    [Rpc(SendTo.Server)]
    private void NotifyLoadCompleteRpc()
    {
        Debug.Log("Received a notification");
        IncrementPlayerLoaded();
    }
}
