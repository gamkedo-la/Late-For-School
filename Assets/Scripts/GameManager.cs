using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public ChunkSpawner ChunkSpawner;
    public ParticleSystem LeavesSlow;
    public ParticleSystem LeavesFast;
    public GameObject logo;
    public Text levelInputKeyText;
    public Text plusMinusLevelSettingText;
    public SpriteRenderer TutorialBlockRenderer;

    private ScoreManager scoreManager;
    private PlusMinusLevelSetting levelInputSetting;

    public LevelKeyHandler.LevelConfig levelInputConfig = new LevelKeyHandler.LevelConfig();

    private int levelInputSeedIncrement = 1;
    private int levelInputSeedMin = 0;
    private int levelInputSeedMax = 99;

    private float levelInputSpeedIncrement = 0.5f;
    private float levelInputSpeedMin = 2.5f;
    private float levelInputSpeedMax = 10f;

    private float levelInputIntensityIncrement = 0.5f;
    private float levelInputIntensityMin = 2.5f;
    private float levelInputIntensityMax = 10f;

    public enum GameState
    {
        MainMenu,
        Play,
        Pause,
        LevelSelect,
        LevelManager,
        PlusMinusInput,
        SkillsInput,
        Credits
    }

    public enum PlusMinusLevelSetting
    {
        Seed,
        Speed,
        Intensity
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

    public void TransitionToPlusMinusInputState()
    {
        gameState = GameState.PlusMinusInput;
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

    public void SetChosenLevelSetting(PlusMinusLevelSetting levelSetting)
    {
        levelInputSetting = levelSetting;
    }

    public void SetChosenLevelSettingToSeed()
    {
        SetChosenLevelSetting(PlusMinusLevelSetting.Seed);
    }

    public void SetChosenLevelSettingToSpeed()
    {
        SetChosenLevelSetting(PlusMinusLevelSetting.Speed);
    }

    public void SetChosenLevelSettingToIntensity()
    {
        SetChosenLevelSetting(PlusMinusLevelSetting.Intensity);
    }

    public void PlusLevelSetting()
    {
        switch(levelInputSetting)
        {
            case PlusMinusLevelSetting.Seed:
                levelInputConfig.randomSeed += levelInputSeedIncrement;
                levelInputConfig.randomSeed = Mathf.Clamp(levelInputConfig.randomSeed, levelInputSeedMin, levelInputSeedMax);
                break;
            case PlusMinusLevelSetting.Speed:
                levelInputConfig.speed += levelInputSpeedIncrement;
                levelInputConfig.speed = Mathf.Clamp(levelInputConfig.speed, levelInputSpeedMin, levelInputSpeedMax);
                break;
            case PlusMinusLevelSetting.Intensity:
                levelInputConfig.maxIntensity += levelInputIntensityIncrement;
                levelInputConfig.maxIntensity = Mathf.Clamp(levelInputConfig.maxIntensity, levelInputIntensityMin, levelInputIntensityMax);
                break;
        }
        LevelInputConfigChanged();
    }

    public void MinusLevelSetting()
    {
        switch (levelInputSetting)
        {
            case PlusMinusLevelSetting.Seed:
                levelInputConfig.randomSeed -= levelInputSeedIncrement;
                levelInputConfig.randomSeed = Mathf.Clamp(levelInputConfig.randomSeed, levelInputSeedMin, levelInputSeedMax);
                break;
            case PlusMinusLevelSetting.Speed:
                levelInputConfig.speed -= levelInputSpeedIncrement;
                levelInputConfig.speed = Mathf.Clamp(levelInputConfig.speed, levelInputSpeedMin, levelInputSpeedMax);
                break;
            case PlusMinusLevelSetting.Intensity:
                levelInputConfig.maxIntensity -= levelInputIntensityIncrement;
                levelInputConfig.maxIntensity = Mathf.Clamp(levelInputConfig.maxIntensity, levelInputIntensityMin, levelInputIntensityMax);
                break;
        }
        LevelInputConfigChanged();
    }

    public void LevelInputConfigChanged()
    {
        UpdatePlusMinusLevelSettingText();
        UpdateLevelInputKey();
    }

    public void UpdatePlusMinusLevelSettingText()
    {
        switch (levelInputSetting)
        {
            case PlusMinusLevelSetting.Seed:
                plusMinusLevelSettingText.text = levelInputConfig.randomSeed.ToString();
                break;
            case PlusMinusLevelSetting.Speed:
                plusMinusLevelSettingText.text = levelInputConfig.speed.ToString();
                break;
            case PlusMinusLevelSetting.Intensity:
                plusMinusLevelSettingText.text = levelInputConfig.maxIntensity.ToString();
                break;
        }
    }

    public void UpdateLevelInputKey()
    {
        levelInputKeyText.text = LevelKeyHandler.GenerateKey(levelInputConfig);
    }

    public void SwitchIncludeTutorialChunks()
    {
        levelInputConfig.includeTutorialChunks = !levelInputConfig.includeTutorialChunks;
        UpdateIncludeTutorialChunksButtonOpacity();
        LevelInputConfigChanged();
    }

    public void UpdateIncludeTutorialChunksButtonOpacity()
    {
        if (levelInputConfig.includeTutorialChunks)
        {
            Color color = TutorialBlockRenderer.color;
            color.a = 1f;
            TutorialBlockRenderer.color = color;
        }
        else
        {
            Color color = TutorialBlockRenderer.color;
            color.a = 0.5f;
            TutorialBlockRenderer.color = color;
        }
    }
}
