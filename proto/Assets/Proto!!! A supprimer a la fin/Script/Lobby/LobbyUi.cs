using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Runtime.Networking.Lobby
{
    public class LobbyUi : MonoBehaviour
    {
        [SerializeField] private List<TextMeshProUGUI> playerPrefLobby;
        [SerializeField] private Button codeButton;
        [SerializeField] private Button validPseudoButton;
        [SerializeField] private TextMeshProUGUI playerNumberInLobby;
        [SerializeField] private TextMeshProUGUI codeButtonText;
        [SerializeField] private GameObject lobbyPanel;
        [SerializeField] private GameObject previewPanel;

        [SerializeField] private TMP_InputField inputCode;
        [SerializeField] private TMP_InputField pseudoInput;

        public static event Action<string> OnRequestJoinLobby;

        private void Awake()
        {
            LobbyManager.OnLobbyCreated += OnLobbyCreated;

            LobbyManager.OnCloseLobby += LobbyClose;

            LobbyManager.OnLobbyUpdated += OnLobbyCreated;
            LobbyManager.OnLobbyUpdated += UpdatePlayerCount;

            validPseudoButton.onClick.AddListener(UpdatePseudo);
        }

        private void UpdatePseudo()
        {
            LobbyHelper.SetPseudo(pseudoInput.text);
        }

        private void LobbyClose()
        {
            if (previewPanel != null && lobbyPanel != null)
            {
                previewPanel.SetActive(false);
                lobbyPanel.SetActive(true);
            }
        }

        private void UpdatePlayerCount(Unity.Services.Lobbies.Models.Lobby _Obj)
        {
            playerNumberInLobby.text = _Obj.Players.Count + "/4 Players";
            DisplayCode(_Obj);
        }

        public void CopyCode()
        {
            TextEditor _editor = new TextEditor();
            _editor.text = codeButtonText.text;
            _editor.SelectAll();
            _editor.Copy();
        }

        private void DisplayCode(Unity.Services.Lobbies.Models.Lobby _Obj)
        {
            if (codeButton != null)
                codeButtonText.text = _Obj.LobbyCode;
        }

        private void OnLobbyCreated(Unity.Services.Lobbies.Models.Lobby lobby)
        {
            if (previewPanel != null && lobbyPanel != null)
            {
                previewPanel.SetActive(false);
                lobbyPanel.SetActive(true);
            }


            UpdatePlayerCount(lobby);
        }

        public void JoinLobby()
        {
            if (string.IsNullOrWhiteSpace(inputCode.text))
            {
                Debug.LogWarning("Code de lobby vide !");
                return;
            }

            OnRequestJoinLobby?.Invoke(inputCode.text);
        }
        public void StartGame()
        {
            LobbyManager.Instance.StartGame();
        }

        public void QuitLobby()
        {
            LobbyManager.Instance.QuitLobby();
        }
    }
}