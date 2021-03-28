using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider2D))]
public class MenuBlock : MonoBehaviour
{
    [FMODUnity.EventRef]
    public string blockHitSound;

    public UnityEvent onHit;

    private BoxCollider2D boxCollider2D;
    public delegate void PlayerCollisionAction();

    public void Start()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.bounds.max.y <= boxCollider2D.bounds.min.y &&
            collision.collider.bounds.min.x < boxCollider2D.bounds.max.x &&
            collision.collider.bounds.max.x > boxCollider2D.bounds.min.x &&
            collision.collider.CompareTag("Player"))
        {
            onHit.Invoke();
            FMODUnity.RuntimeManager.PlayOneShot(blockHitSound);
        }
    }
}
