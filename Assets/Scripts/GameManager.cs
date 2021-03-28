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
        LevelSelect,
        LevelManager,
        SeedInput,
        SpeedInput,
        IntensityInput,
        SkillsInput,
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

    public void TransitionToLevelSelectState()
    {
        gameState = GameState.LevelSelect;
    }

    public void TransitionToLevelManagerState()
    {
        gameState = GameState.LevelManager;
    }

    public void TransitionToSeedInputState()
    {
        gameState = GameState.SeedInput;
    }

    public void TransitionToSpeedInputState()
    {
        gameState = GameState.SpeedInput;
    }

    public void TransitionToIntensityInputState()
    {
        gameState = GameState.IntensityInput;
    }

    public void TransitionToSkillsInputState()
    {
        gameState = GameState.SkillsInput;
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
