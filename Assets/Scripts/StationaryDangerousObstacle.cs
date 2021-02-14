using UnityEngine;

public class StationaryDangerousObstacle : MonoBehaviour
{
    public int damage = 1;
    public float movementSlowdownTime = 0.0f; // seconds

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Player player = collision.GetComponent<Player>();
            player.RemoveHealth(damage);
            player.SlowPlayerMovement(movementSlowdownTime);
            player.SetInvincible(player.timeInvincibleAfterHurt);
        }
    }
}
