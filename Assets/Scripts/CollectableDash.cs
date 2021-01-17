using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableDash : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<Player>().AddDash();
            Destroy(gameObject);
        }
    }
}
