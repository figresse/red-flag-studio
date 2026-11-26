using UnityEngine;

public class Shoot : MonoBehaviour
{
    public GameObject bullet;
    public GameObject canon;
    public void ShootAction()
    {
        GameObject spawnBullet =Instantiate(bullet,canon.transform.position,bullet.transform.rotation);
        spawnBullet.GetComponent<Rigidbody>().AddForce(transform.forward * 100, ForceMode.Impulse);
    }
}
