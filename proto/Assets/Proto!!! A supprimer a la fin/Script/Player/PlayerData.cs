using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public int health;

    public void TakeDamage(int damage)
    {
        health -= damage;
    }
}
