using UnityEngine;
using Unity.VisualScripting;

public class Move : MonoBehaviour
{
    public float speed = 0.1f;
    private bool isGrounded = false;
    private bool isJumping = false;
    private float jumptime = 10;
    private float jumpforce = 500;
    public Rigidbody rigidbodya;
    public Collider collider;
    void Update()
    {
        Vector3 ef = transform.position;
        Vector3 fe = transform.position;
        if (Input.GetKey(KeyCode.W))
        {
            Vector3 NewPos = transform.position + transform.forward * speed;
            transform.position = NewPos;
        }
        if (Input.GetKey(KeyCode.S))
        {
            Vector3 NewPos = transform.position + transform.forward * -speed;
            transform.position = NewPos;
        }
        if (Input.GetKey(KeyCode.D))
        {
            Vector3 NewPos = transform.position + transform.right * speed;
            transform.position = NewPos;
        }
        if (Input.GetKey(KeyCode.A))
        {
            Vector3 NewPos = transform.position + transform.right * -speed;
            transform.position = NewPos;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            transform.Rotate(new Vector3(0, -0.2f, 0));
        }
        if (Input.GetKey(KeyCode.E))
        {
            transform.Rotate(new Vector3(0, 0.2f, 0));
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded)
            {
                isJumping = true;
                isGrounded = false;
            }
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            isJumping = false;
        }
        if (isJumping)
        {
            if (jumptime > 0)
            {
                rigidbodya.AddForce(transform.up * jumpforce);
                jumptime = -1;
                print("jump");
            }
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("sol"))
        {
            isGrounded = true;
            jumptime = 10;
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }

}
