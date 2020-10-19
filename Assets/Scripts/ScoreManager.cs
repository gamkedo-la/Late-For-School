using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public float timeForEachPoint = 1;
    public Text scoreDisplay;

    private float timeLeftForNextPoint;
    private int score;

    void Update()
    {
        scoreDisplay.text = "Score: " + score.ToString();

        timeLeftForNextPoint -= Time.deltaTime;
        if (timeLeftForNextPoint <= 0)
        {
            score++;
            timeLeftForNextPoint += timeForEachPoint;
        }
    }
}
