using UnityEngine;

public class DangerousObstacle : MonoBehaviour
{
    public int damage = 1;
    public float speed = 5;

    private void Update()
    {
        transform.Translate(Vector2.left * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<Player>().health -= damage;
            Destroy(gameObject);
        }
    }
}
