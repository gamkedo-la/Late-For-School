using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class MenuBlock : MonoBehaviour
{
    [FMODUnity.EventRef]
    public string blockHitSound;

    private BoxCollider2D boxCollider2D;
    public delegate void PlayerCollisionAction();
    private PlayerCollisionAction playerCollisionAction;

    public void Start()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.bounds.max.y <= boxCollider2D.bounds.min.y &&
            collision.collider.bounds.min.x < boxCollider2D.bounds.max.x &&
            collision.collider.bounds.max.x > boxCollider2D.bounds.min.x &&
            collision.collider.CompareTag("Player") &&
            playerCollisionAction != null)
        {
            playerCollisionAction();
            FMODUnity.RuntimeManager.PlayOneShot(blockHitSound);
        }
    }

    public void SetPlayerCollisionAction(PlayerCollisionAction playerCollisionAction)
    {
        this.playerCollisionAction = playerCollisionAction; // Set to null to have no action
    }
}
