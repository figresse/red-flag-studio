using Unity.Netcode;
using UnityEngine;

public class SpawnManager : NetworkBehaviour
{
    public NetworkObject playerBody;

    private Vector3 spawnPosition = new Vector3(0,0,0);


    [ContextMenu("Init")]
    private void Init()
    {
        VerifState();
        NetworkManager.OnClientDisconnectCallback += DestroyPlayer;
        SpawnPlayer();
        print("Init finished");
    }
    void VerifState()
    {
        if (NetworkManager.IsServer == false)
        {
            Destroy(this);
        }
    }   

    private void SpawnPlayer()
    {
        print(NetworkManager.ConnectedClientsList.Count);
        for (int i = 0; NetworkManager.ConnectedClientsList.Count - 1 >= i; i++)
        {
            print(NetworkManager.ConnectedClientsList[i].ClientId);
            Vector3 newSpawnPos = spawnPosition;
            spawnPosition.x += (i+1) * 5;

            NetworkManager.SpawnManager.InstantiateAndSpawn(playerBody, NetworkManager.ConnectedClientsList[i].ClientId,false , true , false , newSpawnPos);
            
        }
        print("SpawnFinish");
    }
    private void DestroyPlayer(ulong playerDisconnected)
    {

    }
    private void ClickScreen()
    {

    }
}
