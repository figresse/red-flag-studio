using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class VerifPlayerOwner : NetworkBehaviour
{
    public GameObject playerCamera;
    public Movement movementScript;
    public NetworkObject playerObject;
    public Shoot shootScript;
    private void Start()
    {
        print("test");
        print(playerObject.OwnerClientId);
        print(NetworkManager.LocalClientId);

        if (playerObject.OwnerClientId != NetworkManager.LocalClientId)
        {
            playerCamera.SetActive(false);
            Destroy(movementScript);
        }
    }
}
