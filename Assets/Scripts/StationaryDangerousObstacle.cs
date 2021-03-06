﻿using UnityEngine;

public class StationaryDangerousObstacle : MonoBehaviour
{
    public int damage = 1;
    public float movementSlowdownTime = 1.0f; // seconds
    public bool isOnWall = false;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Player player = collision.GetComponent<Player>();
            if (!player.IsInvincible())
            {
                player.RemoveHealth(damage);
                player.SlowPlayerMovement(movementSlowdownTime);
            }
        }
    }
}
