using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{
    public Camera playerCamera;
    public GameObject playerBody;
    public float testValueX;
    public float testValueY;
    public Vector2 mouseTest;
    public int mouseSensitivityUp;
    public int mouseSensitivityRight;

    public void DebugMouseDeltaTest(InputAction.CallbackContext mouseDelta)
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            mouseTest = mouseDelta.ReadValue<Vector2>();
            testValueX = (mouseTest.x * mouseSensitivityRight) * Time.deltaTime;
            testValueY = (-mouseTest.y * mouseSensitivityUp) * Time.deltaTime;
            playerCamera.transform.eulerAngles += new Vector3(testValueY, 0, 0);
            playerBody.transform.eulerAngles += new Vector3(0, testValueX, 0);
        }
    }

    [ContextMenu("Test")]
    public void TestMouseLock()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    [ContextMenu("Unlock")]
    public void TestMouseUnlock()
    {
        Cursor.lockState &= CursorLockMode.None;
    }
}
