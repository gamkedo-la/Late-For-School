using UnityEngine;

public class StationaryDangerousObstacle : MonoBehaviour
{
    public int damage = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<Player>().RemoveHealth(damage);
            Destroy(gameObject);
        }
    }
}
