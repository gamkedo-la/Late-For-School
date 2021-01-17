using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableLife : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<Player>().AddHealth(1);
            Destroy(gameObject);
        }
    }
}
