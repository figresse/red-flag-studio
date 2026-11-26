using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameMode : NetworkBehaviour
{
    private static GameMode instance;
    public static GameMode Instance => instance;

    public List<ulong> playerIdList = new();
    public List<GameObject> listOfFloor = new();

    private void Awake()
    {
        if (!NetworkManager.IsHost)
        {
            Destroy(gameObject);
        }
        if (Instance)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    public void InitGame(List<ulong> playerId)
    {
        playerIdList = playerId;
    }
}
