using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider2D))]
public class MenuBlock : MonoBehaviour
{
    [FMODUnity.EventRef]
    public string blockHitSound;

    public UnityEvent onHit;

    private static float hitTimeOutSeconds = 0.5f;
    private static float hitTimeOutLeft = 0;
    public static bool hitTimeOutUpdatedThisFrame = false;

    private BoxCollider2D boxCollider2D;
    public delegate void PlayerCollisionAction();

    public void Start()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();
    }

    public void Update()
    {
        hitTimeOutUpdatedThisFrame = false;
    }

    public void LateUpdate()
    {
        // Prevent more than one class from updating the timer each frame
        if (!hitTimeOutUpdatedThisFrame)
        {
            hitTimeOutLeft -= Time.deltaTime;
            hitTimeOutUpdatedThisFrame = true;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.bounds.max.y <= boxCollider2D.bounds.min.y &&
            collision.collider.bounds.min.x < boxCollider2D.bounds.max.x &&
            collision.collider.bounds.max.x > boxCollider2D.bounds.min.x &&
            collision.collider.CompareTag("Player") &&
            hitTimeOutLeft <= 0)
        {
            onHit.Invoke();
            PlayBlockHitSound();
        }
    }

    private void OnEnable()
    {
        hitTimeOutLeft = hitTimeOutSeconds;
    }

    public void PlayBlockHitSound()
    {
        FMODUnity.RuntimeManager.PlayOneShot(blockHitSound);
    }
}
