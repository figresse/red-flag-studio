using Runtime.Networking.Lobby;
using Unity.Services.Lobbies;
using UnityEngine;

public class MainMenuUi : MonoBehaviour
{
    public void QuitGame()
    {
        Application.Quit();
    }
    public void CreateLobby()
    {
        LobbyManager.Instance.CreateLobby();
    }
}
