using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject obstacle;
    public float startTimeBetweenSpawn;
    public float decreaseTime;
    public float minTime = 0.65f;
    public float maxY = 1;
    public float minY = -1;

    private float timeBetweenSpawn;
    private int randomSeed;

    void Update()
    {
        if (timeBetweenSpawn <= 0)
        {
            Random.InitState(randomSeed);
            float posY = Random.Range(minY, maxY);
            Instantiate(obstacle, new Vector2(transform.position.x, posY), Quaternion.identity);
            timeBetweenSpawn = startTimeBetweenSpawn;
            if (startTimeBetweenSpawn > minTime)
            {
                startTimeBetweenSpawn -= decreaseTime;
            }
            randomSeed++; // TODO: Have specific functions for altering the random seed for each level to create multiple levels
        }
        else
        {
            timeBetweenSpawn -= Time.deltaTime;
        }
    }
}
