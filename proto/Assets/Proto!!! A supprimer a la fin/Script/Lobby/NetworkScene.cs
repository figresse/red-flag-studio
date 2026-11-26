using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Runtime.Networking.Lobby
{
    public class NetworkScene : MonoBehaviour
    {
        [SerializeField] private string menuSceneName = "";
        [SerializeField] private string gameSceneName = "";

#if UNITY_EDITOR
        [SerializeField] private SceneAsset gameSceneAsset;

        void OnValidate()
        {
            if (gameSceneAsset == null) return;
            gameSceneName = gameSceneAsset.name;
        }

#endif
        private string currentScene = "";

        public static NetworkScene Instance;

        protected void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this);
            }

            DontDestroyOnLoad(this);
        }

        public async Task LoadGameSceneAsync()
        {
            await LoadSceneAsync(gameSceneName);
        }

        public async Task LoadMenuSceneAsync()
        {
            await LoadSceneAsync(menuSceneName);
        }

        private async Task UnloadMenuScene()
        {
            await SceneManager.UnloadSceneAsync(menuSceneName);
        }

        private async Task LoadSceneAsync(string sceneName)
        {
            if (currentScene == sceneName)
                return;

            if (sceneName == menuSceneName)
            {
                SceneManager.LoadScene(sceneName);
                currentScene = sceneName;
                return;
            }

            if (NetworkManager.Singleton != null &&
                NetworkManager.Singleton.IsListening)
            {
                Debug.Log("Load Scene");
                if (NetworkManager.Singleton.IsServer)
                {
                    NetworkManager.Singleton.SceneManager.LoadScene(
                        sceneName,
                        LoadSceneMode.Single);
                    Debug.Log("Scene : " + sceneName);
                }
            }
            else
            {
                SceneManager.LoadScene(sceneName);
            }

            currentScene = sceneName;
        }
    }
}