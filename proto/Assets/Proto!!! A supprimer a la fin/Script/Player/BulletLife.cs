using UnityEngine;

public class BulletLife : MonoBehaviour
{
    public int lifeTime;
    private float second = 1;
    private void Update()
    {
        second -= Time.deltaTime;
        if(second <= 0)
        {
            second = 1;
            lifeTime -= 1;
            if (lifeTime <= 0)
            {
                LifeSpanReach();
            }
        }
    }

    private void LifeSpanReach()
    {
        Destroy(this.gameObject);
    }
}
