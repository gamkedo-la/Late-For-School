using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public float timeForEachPoint = 1;
    public Text scoreDisplay;

    private float timeLeftForNextPoint;
    private int score;

    public static ScoreManager instance;

    public static ScoreManager GetInstance()
    {
        return instance;
    }

    private void Awake()
    {
        instance = this;
    }

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

    public void ResetScore()
    {
        score = 0;
    }
     public int GetScore(){
         return score;
     }
}
