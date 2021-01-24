﻿using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public MenuBlock PlayBlock;
    public MenuBlock LevelManagerBlock;
    public MenuBlock CreditsBlock;
    public MenuBlock BackBlock;
    public ChunkSpawner ChunkSpawner;

    private ScoreManager scoreManager;

    public enum GameState
    {
        MainMenu,
        Play,
        Pause,
        LevelManager,
        Credits
    }

    private GameState gameState = GameState.MainMenu;

    private static GameManager instance;
    
    public static GameManager GetInstance()
    {
        return instance;
    }

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        scoreManager = ScoreManager.GetInstance();

        TransitionToMainMenuState();
    }

    void Update()
    {
        switch(gameState)
        {
            case GameState.MainMenu:
                break;
            case GameState.Play:
                break;
            case GameState.Pause:
                break;
            case GameState.LevelManager:
                break;
            case GameState.Credits:
                break;
        }
    }

    void InitialiseMainMenuBlocks()
    {
        PlayBlock.SetPlayerCollisionAction(TransitionToPlayState);
        LevelManagerBlock.SetPlayerCollisionAction(TransitionToLevelManagerState);
        CreditsBlock.SetPlayerCollisionAction(TransitionToCreditsState);
    }

    void InitialiseBackBlock()
    {
        BackBlock.SetPlayerCollisionAction(TransitionToMainMenuState);
    }

    void TransitionToMainMenuState()
    {
        gameState = GameState.MainMenu;
        ChunkSpawner.DestroyChunks();
        ChunkSpawner.gameObject.SetActive(false);
        PlayBlock.gameObject.SetActive(true);
        LevelManagerBlock.gameObject.SetActive(true);
        CreditsBlock.gameObject.SetActive(true);
        scoreManager.scoreDisplay.gameObject.SetActive(false);
        BackBlock.gameObject.SetActive(false);

        BackBlock.SetPlayerCollisionAction(null);

        Invoke("InitialiseMainMenuBlocks", 0.5f);
    }

    void TransitionToPlayState()
    {
        gameState = GameState.Play;
        ChunkSpawner.DestroyChunks();
        ChunkSpawner.gameObject.SetActive(true);
        PlayBlock.gameObject.SetActive(false);
        LevelManagerBlock.gameObject.SetActive(false);
        CreditsBlock.gameObject.SetActive(false);
        scoreManager.scoreDisplay.gameObject.SetActive(true);
        scoreManager.ResetScore();
        BackBlock.gameObject.SetActive(false);

        PlayBlock.SetPlayerCollisionAction(null);
        LevelManagerBlock.SetPlayerCollisionAction(null);
        CreditsBlock.SetPlayerCollisionAction(null);
        BackBlock.SetPlayerCollisionAction(null);
    }

    void TransitionToLevelManagerState()
    {
        gameState = GameState.LevelManager;
        PlayBlock.gameObject.SetActive(false);
        LevelManagerBlock.gameObject.SetActive(false);
        CreditsBlock.gameObject.SetActive(false);
        BackBlock.gameObject.SetActive(true);

        PlayBlock.SetPlayerCollisionAction(null);
        LevelManagerBlock.SetPlayerCollisionAction(null);
        CreditsBlock.SetPlayerCollisionAction(null);

        Invoke("InitialiseBackBlock", 0.5f);
    }

    void TransitionToCreditsState()
    {
        gameState = GameState.LevelManager;
        PlayBlock.gameObject.SetActive(false);
        LevelManagerBlock.gameObject.SetActive(false);
        CreditsBlock.gameObject.SetActive(false);
        BackBlock.gameObject.SetActive(true);

        PlayBlock.SetPlayerCollisionAction(null);
        LevelManagerBlock.SetPlayerCollisionAction(null);
        CreditsBlock.SetPlayerCollisionAction(null);

        Invoke("InitialiseBackBlock", 0.5f);
    }
}