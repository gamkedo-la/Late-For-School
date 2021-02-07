using UnityEngine;

public class BoundsController : MonoBehaviour
{
    public GameObject deathBounds;
    public GameObject rightBoundsNoFriction;
    public GameObject leftBoundsFriction;
    public GameObject rightBoundsFriction;
    public GameObject topBounds;

    void Update()
    {
        if (GameManager.GetInstance().GetState() == GameManager.GameState.Play)
        {
            deathBounds.SetActive(true);
            rightBoundsNoFriction.SetActive(true);
            leftBoundsFriction.SetActive(false);
            rightBoundsFriction.SetActive(false);
            topBounds.SetActive(false);
        }
        else
        {
            deathBounds.SetActive(false);
            rightBoundsNoFriction.SetActive(false);
            leftBoundsFriction.SetActive(true);
            rightBoundsFriction.SetActive(true);
            topBounds.SetActive(true); // Prevents player from infinitely climbing up on either wall
        }
    }
}
