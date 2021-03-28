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
        logo.SetActive(true);

        BackBlock.SetPlayerCollisionAction(null);

        // Doing this prevents the block that appears in the 
        // main menu screen from being hit immediately
        Invoke("InitialiseMainMenuBlocks", 0.5f);
    }

    void TransitionToPlayState()
    {
        gameState = GameState.Play;
        ChunkSpawner.DestroyChunks();
        ChunkSpawner.UpdateKeyFromUI();
        scoreManager.ResetScore();
        logo.SetActive(false);

        PlayBlock.SetPlayerCollisionAction(null);
        LevelManagerBlock.SetPlayerCollisionAction(null);
        CreditsBlock.SetPlayerCollisionAction(null);
        BackBlock.SetPlayerCollisionAction(null);
    }

    void TransitionToLevelManagerState()
    {
        gameState = GameState.LevelManager;

        PlayBlock.SetPlayerCollisionAction(null);
        LevelManagerBlock.SetPlayerCollisionAction(null);
        CreditsBlock.SetPlayerCollisionAction(null);

        Invoke("InitialiseBackBlock", 0.5f);
    }

    void TransitionToCreditsState()
    {
        gameState = GameState.Credits;

        PlayBlock.SetPlayerCollisionAction(null);
        LevelManagerBlock.SetPlayerCollisionAction(null);
        CreditsBlock.SetPlayerCollisionAction(null);

        Invoke("InitialiseBackBlock", 0.5f);
    }

    public GameState GetState()
    {
        return gameState;
    }
}
