using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public MenuBlock PlayBlock;
    public MenuBlock LevelManagerBlock;
    public MenuBlock CreditsBlock;
    public MenuBlock BackBlock;
    public ChunkSpawner ChunkSpawner;
    public ParticleSystem LeavesSlow;
    public ParticleSystem LeavesFast;
    public GameObject logo;

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

    public void TransitionToMainMenuState()
    {
        gameState = GameState.MainMenu;
        ChunkSpawner.DestroyChunks();
    }

    public void TransitionToPlayState()
    {
        gameState = GameState.Play;
        ChunkSpawner.DestroyChunks();
        ChunkSpawner.UpdateKeyFromUI();
        scoreManager.ResetScore();
    }

    public void TransitionToLevelManagerState()
    {
        gameState = GameState.LevelManager;
    }

    public void TransitionToCreditsState()
    {
        gameState = GameState.Credits;
    }

    public GameState GetState()
    {
        return gameState;
    }
}
