﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public float timeForEachPoint = 1;
    public List<Text> scoreDisplays;
    public List<Text> bestDisplays;

    private float timeLeftForNextPoint;
    private int score;
    private bool shouldCount = false;

    public static ScoreManager instance;

    public static ScoreManager GetInstance()
    {
        return instance;
    }

    private void Awake()
    {
        instance = this;
    }
    private void Start() 
    {
        //ChunkSpawner.GetInstance().UpdateKeyFromUI();
        string lookUp = GetBestSavedScoreLookUp();
        int best = PlayerPrefs.GetInt(lookUp, 0); 
        foreach (Text bestDisplay in bestDisplays)
        {
            bestDisplay.text = "Best: " + best.ToString();
        }
        Debug.Log(best + " Should be the HighScore WORK IN PROGRESS");
    }
    void Update()
    {
        foreach (Text scoreDisplay in scoreDisplays)
        {
            scoreDisplay.text = "Score: " + score.ToString();
        }

        if (shouldCount)
        {
            timeLeftForNextPoint -= Time.deltaTime;
            if (timeLeftForNextPoint <= 0)
            {
                score++;
                timeLeftForNextPoint += timeForEachPoint;
            }
        }
    }
    private string GetBestSavedScoreLookUp(){
        string levelKey = ChunkSpawner.GetInstance().GetLevelKey();
        string lookUp = levelKey;
        return lookUp;
    }
    public void SaveScore(){
        int score = GetScore();
        string lookUp = GetBestSavedScoreLookUp();
        int best = PlayerPrefs.GetInt(lookUp, 0); 
        if(score > best) {
            Debug.Log("New Best Score for Level " + lookUp + " was " + best + " now " + score);
            PlayerPrefs.SetInt(lookUp, score);
        } else {
            Debug.Log("Did not get Best Score for Level " + lookUp + " was " + best + " now " + score);
        }
    }
    public void ResetScore()
    {
        score = 0;
    }
     public int GetScore(){
         return score;
     }

    public void StartCounting()
    {
        shouldCount = true;
    }

    public void StopCounting()
    {
        shouldCount = false;
    }
}
