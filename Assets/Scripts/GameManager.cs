﻿using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;

public class GameManager : MonoBehaviour
{
    public ChunkSpawner ChunkSpawner;
    public ParticleSystem LeavesSlow;
    public ParticleSystem LeavesFast;
    public GameObject logo;
    public Text levelInputKeyText;
    public Text plusMinusLevelSettingText;
    public SpriteRenderer tutorialBlockRenderer;
    public SpriteRenderer dashSkillBlockRenderer;
    public SpriteRenderer slideSkillBlockRenderer;
    public SpriteRenderer wallClimbSkillBlockRenderer;
    public SpriteRenderer wallJumpSkillBlockRenderer;
    public PopupTooltip plusPopupTooltip;
    public PopupTooltip minusPopupTooltip;
    public PopupTooltip tutorialPopupTooltip;
    public float runSummaryScreenTime = 5;

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
        Credits,
        RunSummary
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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (gameState == GameState.Pause)
            {
                UnPauseGame();
            }
            else if (gameState == GameState.Play)
            {
                PauseGame();
            }
        }
    }

    public void TransitionToMainMenuState()
    {
        gameState = GameState.MainMenu;
        ChunkSpawner.DestroyChunks();
    }

    private void PostRunSummary()
    {
        TransitionToMainMenuState();
        Player.GetInstance().UnFreezePos();
        Player.GetInstance().ResetHealth();
    }

    public void TransitionToRunSummaryState()
    {
        gameState = GameState.RunSummary;
        scoreManager.StopCounting();
        Invoke("PostRunSummary", runSummaryScreenTime);
    }

    public void TransitionToPlayState()
    {
        gameState = GameState.Play;
        ChunkSpawner.GetInstance().InitialiseWithLevelKey(LevelKeyHandler.GenerateKey(levelInputConfig));
        scoreManager.ResetScore();
        scoreManager.StartCounting();
    }

    public void PauseGame()
    {
        gameState = GameState.Pause;
        Time.timeScale = 0;
    }

    public void UnPauseGame()
    {
        gameState = GameState.Play;
        Time.timeScale = 1;
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

    public void TriggerRunSummary()
    {

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
                plusPopupTooltip.text = "Random Seed Incremented";
                break;
            case PlusMinusLevelSetting.Speed:
                levelInputConfig.speed += levelInputSpeedIncrement;
                levelInputConfig.speed = Mathf.Clamp(levelInputConfig.speed, levelInputSpeedMin, levelInputSpeedMax);
                plusPopupTooltip.text = "Level Speed Increased";
                break;
            case PlusMinusLevelSetting.Intensity:
                levelInputConfig.maxIntensity += levelInputIntensityIncrement;
                levelInputConfig.maxIntensity = Mathf.Clamp(levelInputConfig.maxIntensity, levelInputIntensityMin, levelInputIntensityMax);
                plusPopupTooltip.text = "Level Difficulty Increased";
                break;
        }
        plusPopupTooltip.Activate();
        LevelInputConfigChanged();
    }

    public void MinusLevelSetting()
    {
        switch (levelInputSetting)
        {
            case PlusMinusLevelSetting.Seed:
                levelInputConfig.randomSeed -= levelInputSeedIncrement;
                levelInputConfig.randomSeed = Mathf.Clamp(levelInputConfig.randomSeed, levelInputSeedMin, levelInputSeedMax);
                minusPopupTooltip.text = "Random Seed Decremented";
                break;
            case PlusMinusLevelSetting.Speed:
                levelInputConfig.speed -= levelInputSpeedIncrement;
                levelInputConfig.speed = Mathf.Clamp(levelInputConfig.speed, levelInputSpeedMin, levelInputSpeedMax);
                minusPopupTooltip.text = "Level Speed Decreased";
                break;
            case PlusMinusLevelSetting.Intensity:
                levelInputConfig.maxIntensity -= levelInputIntensityIncrement;
                levelInputConfig.maxIntensity = Mathf.Clamp(levelInputConfig.maxIntensity, levelInputIntensityMin, levelInputIntensityMax);
                minusPopupTooltip.text = "Level Difficulty Decreased";
                break;
        }
        minusPopupTooltip.Activate();
        LevelInputConfigChanged();
    }

    public void LevelInputConfigChanged()
    {
        UpdatePlusMinusLevelSettingText();
        UpdateDashSkillBlockOpacity();
        UpdateSlideSkillBlockOpacity();
        UpdateWallClimbSkillBlockOpacity();
        UpdateWallJumpSkillBlockOpacity();
        UpdateIncludeTutorialChunksBlockOpacity();

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

        if (levelInputConfig.includeTutorialChunks)
        {
            tutorialPopupTooltip.text = "Level will include skill tutorials";
        }
        else
        {
            tutorialPopupTooltip.text = "Level will not include skill tutorials";
        }
        tutorialPopupTooltip.Activate();

        LevelInputConfigChanged();
    }

    private void SwitchSkill(List<Player.Skill> skills, Player.Skill skill)
    {
        if (!skills.Contains(skill))
        {
            skills.Add(skill);
        }
        else
        {
            skills.Remove(skill);
        }
    }

    public void SwitchDashSkill()
    {
        SwitchSkill(levelInputConfig.includedSkills, Player.Skill.Dash);
        LevelInputConfigChanged();
    }

    public void SwitchSlideSkill()
    {
        SwitchSkill(levelInputConfig.includedSkills, Player.Skill.Slide);
        LevelInputConfigChanged();
    }

    public void SwitchWallClimbSkill()
    {
        SwitchSkill(levelInputConfig.includedSkills, Player.Skill.WallClimb);
        LevelInputConfigChanged();
    }

    public void SwitchWallJumpSkill()
    {
        SwitchSkill(levelInputConfig.includedSkills, Player.Skill.WallJump);
        LevelInputConfigChanged();
    }

    private void UpdateSpriteRendererOpacity(SpriteRenderer spriteRenderer, float opacity)
    {
        Color color = spriteRenderer.color;
        color.a = opacity;
        spriteRenderer.color = color;
    }

    public void UpdateIncludeTutorialChunksBlockOpacity()
    {
        if (levelInputConfig.includeTutorialChunks)
        {
            UpdateSpriteRendererOpacity(tutorialBlockRenderer, 1);
        }
        else
        {
            UpdateSpriteRendererOpacity(tutorialBlockRenderer, 0.5f);
        }
    }

    public void UpdateDashSkillBlockOpacity()
    {
        if (levelInputConfig.includedSkills.Contains(Player.Skill.Dash))
        {
            UpdateSpriteRendererOpacity(dashSkillBlockRenderer, 1);
        }
        else
        {
            UpdateSpriteRendererOpacity(dashSkillBlockRenderer, 0.5f);
        }
    }

    public void UpdateSlideSkillBlockOpacity()
    {
        if (levelInputConfig.includedSkills.Contains(Player.Skill.Slide))
        {
            UpdateSpriteRendererOpacity(slideSkillBlockRenderer, 1);
        }
        else
        {
            UpdateSpriteRendererOpacity(slideSkillBlockRenderer, 0.5f);
        }
    }

    public void UpdateWallClimbSkillBlockOpacity()
    {
        if (levelInputConfig.includedSkills.Contains(Player.Skill.WallClimb))
        {
            UpdateSpriteRendererOpacity(wallClimbSkillBlockRenderer, 1);
        }
        else
        {
            UpdateSpriteRendererOpacity(wallClimbSkillBlockRenderer, 0.5f);
        }
    }

    public void UpdateWallJumpSkillBlockOpacity()
    {
        if (levelInputConfig.includedSkills.Contains(Player.Skill.WallJump))
        {
            UpdateSpriteRendererOpacity(wallJumpSkillBlockRenderer, 1);
        }
        else
        {
            UpdateSpriteRendererOpacity(wallJumpSkillBlockRenderer, 0.5f);
        }
    }

    public void PasteLevelKey()
    {
#if UNITY_WEBGL
        ReadTextFromClipboard(gameObject.name, "LoadLevelFromKey");
#else
        LoadLevelFromKey(EditorGUIUtility.systemCopyBuffer);
#endif
    }

    public void CopyLevelKey()
    {
#if UNITY_WEBGL
        WriteTextToClipboard(levelInputKeyText.text);
#else
        EditorGUIUtility.systemCopyBuffer = levelInputKeyText.text;
#endif
        Debug.Log("Level key copied to clipboard");
        PopupTooltipManager.GetInstance().copyLevelKeySuccessful.Activate();
    }

    public void LoadLevelFromKey(string key)
    {
        LevelKeyHandler.LevelConfig loadedLevelConfig = LevelKeyHandler.ReadKey(key);
        if (loadedLevelConfig != null)
        {
            levelInputConfig = loadedLevelConfig;
            LevelInputConfigChanged();
            Debug.Log("Level loaded from pasted key");
            PopupTooltipManager.GetInstance().pasteLevelKeySuccessful.Activate();
        }
        else
        {
            Debug.Log("Pasted level key is invalid, could not load level");
            PopupTooltipManager.GetInstance().pasteLevelKeyFailed.Activate();
        }
    }

    [DllImport("__Internal")]
    private static extern void WriteTextToClipboard(string text);

    [DllImport("__Internal")]
    private static extern void ReadTextFromClipboard(string gameObjectName, string functionName);
}
