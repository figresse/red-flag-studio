using UnityEngine;

public class BulletDamage : MonoBehaviour
{
    public int damage;
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.GetComponent<PlayerData>() != null)
        {
            collision.gameObject.GetComponent<PlayerData>().TakeDamage(damage);
        }
    }
}
