using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbySlotContainer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerName;
    [SerializeField] private TextMeshProUGUI playerCount;
    public Button JoinButton;
    private string lobbyCode;

    public string LobbyCode => lobbyCode;


    public void SetLobbyValue(string _playerName, int _playerCount, int _playerMaxCount, string _lobbyCode)
    {
        playerName.text = _playerName;
        playerCount.text = _playerCount + "/" + _playerMaxCount;
        lobbyCode = _lobbyCode;
    }
}
