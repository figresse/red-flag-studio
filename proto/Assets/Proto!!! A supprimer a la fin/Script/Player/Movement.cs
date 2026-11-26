using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    public int speed = 0;
    public Vector2 moveVector = new();
    public Vector3 finalMoveVector = new();
    float xInput;
    float zInput;


    private void Update()
    {
        finalMoveVector = xInput * transform.right + zInput * transform.forward;
        transform.position += (finalMoveVector * speed) * Time.deltaTime;
    }

    public void MoveAction(InputAction.CallbackContext moveValue)
    {
        moveVector = moveValue.ReadValue<Vector2>();
        xInput = moveVector.x;
        zInput = moveVector.y;
    }
}
