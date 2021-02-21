using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveLevel : MonoBehaviour
{
    public float speed;
    private float startX;

    private void Start()
    {
        startX = transform.position.x;
    }

    void Update()
    {
        Vector3 position = transform.position;
        if (GameManager.GetInstance().GetState() == GameManager.GameState.Play)
        {
            position.x += speed * Time.deltaTime;
            transform.position = position;
        }
        else
        {
            if (position.x != startX)
            {
                position.x = startX;
            }
            transform.position = position;
        }
    }
}
