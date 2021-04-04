using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public ChunkSpawner ChunkSpawner;
    public ParticleSystem LeavesSlow;
    public ParticleSystem LeavesFast;
    public GameObject logo;
    public Text plusMinusLevelSettingText;

    private ScoreManager scoreManager;
    private PlusMinusLevelSetting chosenLevelSetting;
    private string chosenLevelKey = LevelKeyHandler.DefaultKey();

    private int chosenLevelSeed = 0;
    private int chosenLevelSeedIncrement = 1;
    private int chosenLevelSeedMin = 0;
    private int chosenLevelSeedMax = 99;

    private float chosenLevelSpeed = 5;
    private float chosenLevelSpeedIncrement = 0.5f;
    private float chosenLevelSpeedMin = 2.5f;
    private float chosenLevelSpeedMax = 10f;

    private float chosenLevelIntensity = 7.5f;
    private float chosenLevelIntensityIncrement = 0.5f;
    private float chosenLevelIntensityMin = 2.5f;
    private float chosenLevelIntensityMax = 10f;

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
        chosenLevelSetting = levelSetting;
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
        switch(chosenLevelSetting)
        {
            case PlusMinusLevelSetting.Seed:
                chosenLevelSeed += chosenLevelSeedIncrement;
                Mathf.Clamp(chosenLevelSeed, chosenLevelSeedMin, chosenLevelSeedMax);
                break;
            case PlusMinusLevelSetting.Speed:
                chosenLevelSpeed += chosenLevelSpeedIncrement;
                Mathf.Clamp(chosenLevelSpeed, chosenLevelSpeedMin, chosenLevelSpeedMax);
                break;
            case PlusMinusLevelSetting.Intensity:
                chosenLevelIntensity += chosenLevelIntensityIncrement;
                Mathf.Clamp(chosenLevelIntensity, chosenLevelIntensityMin, chosenLevelIntensityMax);
                break;
        }
        UpdatePlusMinusLevelSettingText();
    }

    public void MinusLevelSetting()
    {
        switch (chosenLevelSetting)
        {
            case PlusMinusLevelSetting.Seed:
                chosenLevelSeed -= chosenLevelSeedIncrement;
                Mathf.Clamp(chosenLevelSeed, chosenLevelSeedMin, chosenLevelSeedMax);
                break;
            case PlusMinusLevelSetting.Speed:
                chosenLevelSpeed -= chosenLevelSpeedIncrement;
                Mathf.Clamp(chosenLevelSpeed, chosenLevelSpeedMin, chosenLevelSpeedMax);
                break;
            case PlusMinusLevelSetting.Intensity:
                chosenLevelIntensity -= chosenLevelIntensityIncrement;
                Mathf.Clamp(chosenLevelIntensity, chosenLevelIntensityMin, chosenLevelIntensityMax);
                break;
        }
        UpdatePlusMinusLevelSettingText();
    }

    public void UpdatePlusMinusLevelSettingText()
    {
        switch (chosenLevelSetting)
        {
            case PlusMinusLevelSetting.Seed:
                plusMinusLevelSettingText.text = chosenLevelSeed.ToString();
                break;
            case PlusMinusLevelSetting.Speed:
                plusMinusLevelSettingText.text = chosenLevelSpeed.ToString();
                break;
            case PlusMinusLevelSetting.Intensity:
                plusMinusLevelSettingText.text = chosenLevelIntensity.ToString();
                break;
        }
    }
}
