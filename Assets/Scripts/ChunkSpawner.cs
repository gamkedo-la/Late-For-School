﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ChunkSpawner : MonoBehaviour
{
    public List<GameObject> chunks;
    public Text keyInput;

    [Header("Debug Only (set to -1 for normal gameplay)")]
    [Tooltip("Only spawn this one chunk over and over:")]
    public int debugForceChunkIndex = -1;
    public int minimumChunksBetweenTutorials = 2;
    public int maximumChunksBetweenTutorials = 10;

    [HideInInspector] public LevelKeyHandler.LevelConfig levelConfig;

    private string levelKey = LevelKeyHandler.DefaultKey();

    private List<GameObject> activeChunks = new List<GameObject>();
    private int milestoneChunkCounter;
    private List<Player.Skill> knownSkills = new List<Player.Skill>();
    private List<GameObject> usableChunks = new List<GameObject>();
    private List<GameObject> unusableChunks = new List<GameObject>();
    private int chunksSinceLastTutorial;
    private string lastChosenChunk;

    private static ChunkSpawner instance;
    public static ChunkSpawner GetInstance()
    {
        return instance;
    }

    private void Awake()
    {
        instance = this;
    }

    private void Reset()
    {
        Debug.Log("Resetting level");
        DestroyChunks();
        knownSkills.Clear();
        usableChunks.Clear();
        unusableChunks.Clear();
        milestoneChunkCounter = 0;
    }

    public void InitialiseWithLevelKey(string levelKey)
    {
        Reset();

        levelConfig = LevelKeyHandler.ReadKey(levelKey);
        this.levelKey = LevelKeyHandler.GenerateKey(levelConfig);
        levelConfig.includedSkills.Add(Player.Skill.Jump);

        unusableChunks.AddRange(chunks);
        if (levelConfig.includeTutorialChunks)
        {
            // Only have jump tutorial as first chunk
            foreach (GameObject chunk in unusableChunks)
            {
                Chunk chunkDetails = chunk.GetComponent<Chunk>();
                if (chunkDetails.tutorialChunk &&
                    chunkDetails.includedSkills.Count == 1 &&
                    chunkDetails.includedSkills.Contains(Player.Skill.Jump) &&
                    chunkDetails.version <= levelConfig.version)
                {
                    usableChunks.Add(chunk);
                }
            }
            if (usableChunks.Count <= 0)
            {
                throw new Exception("No tutorial jump chunk provided");
            }
        }
        else
        {
            // Go straight to getting all the chunks that we can use, since we don't need to teach the player to jump first
            UpdateUsableChunks();
        }
    }

    private void UpdateUsableChunks()
    {
        // Get usable chunks from the list
        foreach (GameObject chunk in unusableChunks)
        {
            bool include = true;

            Chunk chunkDetails = chunk.GetComponent<Chunk>();

            // Don't add chunk if it already exists
            // or if the level key version is lower than the chunk version (this would mean the chunk came out after this level key was generated)
            if (usableChunks.Contains(chunk) || chunkDetails.version > levelConfig.version)
            {
                include = false;
                continue;
            }

            if (levelConfig.includeTutorialChunks)
            {
                if (!chunkDetails.tutorialChunk)
                {
                    // Don't add chunk if we don't yet know the skill
                    foreach (Player.Skill skill in chunkDetails.includedSkills)
                    {
                        if (!knownSkills.Contains(skill)) { include = false; }
                    }
                }
                else
                {
                    // Don't add a tutorial chunk if we already know all the included skills or skill isn't in level config
                    bool allSkillsKnown = true;
                    foreach (Player.Skill skill in chunkDetails.includedSkills)
                    {
                        if (!knownSkills.Contains(skill) && levelConfig.includedSkills.Contains(skill)) { allSkillsKnown = false; }
                    }
                    if (allSkillsKnown) { 
                        include = false;
                        continue;
                    }

                    // Don't add a tutorial chunk if we don't know any of the skills that aren't the main skill (index 0)
                    int index = 0;
                    foreach (Player.Skill skill in chunkDetails.includedSkills)
                    {
                        if (index != 0 && !knownSkills.Contains(skill)) { 
                            include = false;
                            continue;
                        }
                        index++;
                    }

                    // Don't add a tutorial chunk if we've had a tutorial chunk recently
                    if (chunksSinceLastTutorial < minimumChunksBetweenTutorials) {
                        include = false;
                    }
                }
            }
            else
            {
                // Don't include any tutorial chunks
                if (chunkDetails.tutorialChunk) { include = false; }

                // Don't include chunks that have skills that aren't included in the level config
                foreach (var skill in chunkDetails.includedSkills)
                {
                    if (!levelConfig.includedSkills.Contains(skill))
                    {
                        include = false;
                        continue;
                    }
                }
            }

            // Don't add chunks with a higher intensity than what is set to max
            if (chunkDetails.intensity > levelConfig.maxIntensity) { include = false; }

            if (include) { usableChunks.Add(chunk); }
        }

        // Remove chunks that are no longer usable
        List<GameObject> toRemove = new List<GameObject>();
        foreach (GameObject chunk in usableChunks)
        {
            bool remove = false;

            Chunk chunkDetails = chunk.GetComponent<Chunk>();

            // Remove tutorial chunk if we already know all the included skills
            if (levelConfig.includeTutorialChunks && chunkDetails.tutorialChunk)
            {
                bool allSkillsKnown = true;
                foreach (Player.Skill skill in chunkDetails.includedSkills)
                {
                    if (!knownSkills.Contains(skill)) { allSkillsKnown = false; }
                }
                if (allSkillsKnown) { remove = true; }
            }

            // Remove all tutorial chunks if we've had a tutorial chunk recently
            if (chunkDetails.tutorialChunk && chunksSinceLastTutorial < minimumChunksBetweenTutorials)
            {
                remove = true;
            }

            if (remove) { toRemove.Add(chunk); }
        }
        usableChunks.RemoveAll(chunk => toRemove.Contains(chunk));
    }

    private GameObject ChooseNextChunk()
    {
        UnityEngine.Random.InitState(levelConfig.randomSeed);
        levelConfig.randomSeed += UnityEngine.Random.Range(1, 10);

        // TODO: choose a chunk based on recent intensity & if we still have moves left to teach
        GameObject chosenChunk;
        if (debugForceChunkIndex > -1)
        {
            chosenChunk = chunks[debugForceChunkIndex];
        }
        else
        {
            if (levelConfig.includeTutorialChunks)
            {
                // don't have too many chunks between tutorials
                bool allSkillsKnown = true;
                foreach (Player.Skill skill in levelConfig.includedSkills)
                {
                    if (!knownSkills.Contains(skill)) { allSkillsKnown = false; }
                }

                if (chunksSinceLastTutorial >= maximumChunksBetweenTutorials &&
                    !allSkillsKnown)
                {
                    chosenChunk = GetTutorialChunk(usableChunks);
                }
                else
                {
                    int chunkIndex = UnityEngine.Random.Range(0, usableChunks.Count);
                    chosenChunk = usableChunks[chunkIndex];
                }
            }
            else
            {
                int chunkIndex = UnityEngine.Random.Range(0, usableChunks.Count);
                chosenChunk = usableChunks[chunkIndex];
            }


            if (lastChosenChunk != null && chosenChunk.name.Equals(lastChosenChunk))
            {
                chosenChunk = ChooseNextChunk();
            }

            lastChosenChunk = chosenChunk.name;
        }

        return chosenChunk;
    }

    private GameObject GetTutorialChunk(List<GameObject> chunks)
    {
        List<GameObject> tutorialChunks = new List<GameObject>();
        foreach (var chunk in chunks)
        {
            if (chunk.GetComponent<Chunk>().tutorialChunk)
            {
                tutorialChunks.Add(chunk);
            }
        }

        int chunkIndex = UnityEngine.Random.Range(0, tutorialChunks.Count);
        return tutorialChunks[chunkIndex];
    }

    private void ChunkSpawnerUpdate()
    {
        List<GameObject> toRemove = new List<GameObject>();

        bool spawnNewChunk = true;
        foreach (GameObject chunk in activeChunks)
        {
            // Move chunks
            Vector2 newPos = chunk.transform.position;
            newPos.x -= levelConfig.speed * Time.deltaTime;
            chunk.transform.position = newPos;

            // Prevent spawning new chunk if there is one in the way
            ChunkBounds[] chunkBounds = chunk.GetComponentsInChildren<ChunkBounds>();
            float chunkEndX = 0;
            foreach (ChunkBounds bounds in chunkBounds)
            {

                if (bounds.chunkEnd)
                {
                    chunkEndX = bounds.transform.position.x;
                }
            }

            if (chunkEndX > transform.position.x)
            {
                spawnNewChunk = false;
            }

            // Remove chunks that have gone off the screen
            if (chunkEndX < Camera.main.ViewportToWorldPoint(new Vector3(0, 1, Camera.main.nearClipPlane)).x)
            {
                toRemove.Add(chunk);
            }
        }

        if (spawnNewChunk)
        {
            GameObject chosenChunk = ChooseNextChunk();
            ChunkBounds[] chunkBounds = chosenChunk.GetComponentsInChildren<ChunkBounds>();
            float chunkOffset = 0f;
            foreach (ChunkBounds bounds in chunkBounds)
            {
                if (bounds.chunkStart)
                {
                    chunkOffset = -bounds.transform.localPosition.x;
                }
            }
            Vector2 spawnPos = new Vector2(transform.position.x + chunkOffset, transform.position.y);
            GameObject newChunk = Instantiate(chosenChunk, spawnPos, Quaternion.identity, transform);

            if (milestoneChunkCounter >= levelConfig.milestoneInterval)
            {
                ChunkBounds[] newChunkBounds = newChunk.GetComponentsInChildren<ChunkBounds>();
                foreach (ChunkBounds bounds in newChunkBounds)
                {
                    if (bounds.chunkStart)
                    {
                        bounds.isMilestone = true;
                    }
                }
                milestoneChunkCounter = 0;
            }
            else
            {
                milestoneChunkCounter++;
            }

            // Add learnt skill(s) to known skills if tutorial chunk was used
            Chunk chunkDetails = chosenChunk.GetComponent<Chunk>();
            if (chunkDetails.tutorialChunk)
            {
                foreach (Player.Skill skill in chunkDetails.includedSkills)
                {
                    if (!knownSkills.Contains(skill)) { knownSkills.Add(skill); }
                }
            }

            // Update chunks since last tutorial
            chunksSinceLastTutorial++;
            if (chunkDetails.tutorialChunk)
            {
                chunksSinceLastTutorial = 0;
            }

            activeChunks.Add(newChunk);
        }

        foreach (GameObject chunk in toRemove)
        {
            activeChunks.Remove(chunk);
            Destroy(chunk);
        }

        UpdateUsableChunks(); // Get all chunks we can now use based on learnt skills
    }

    private void ChunkSpawnerUpdateOld()
    {
        List<GameObject> toRemove = new List<GameObject>();

        bool spawnNewChunk = true;
        foreach (GameObject chunk in activeChunks)
        {
            // Move chunks
            Vector2 newPos = chunk.transform.position;
            newPos.x -= levelConfig.speed * Time.deltaTime;
            chunk.transform.position = newPos;

            // Prevent spawning new chunk if there is one in the way
            ChunkBounds[] chunkBounds = chunk.GetComponentsInChildren<ChunkBounds>();
            float chunkEndX = 0;
            foreach (ChunkBounds bounds in chunkBounds)
            {

                if (bounds.chunkEnd)
                {
                    chunkEndX = bounds.transform.position.x;
                }
            }

            if (chunkEndX > transform.position.x)
            {
                spawnNewChunk = false;
            }

            // Remove chunks that have gone off the screen
            if (chunkEndX < Camera.main.ViewportToWorldPoint(new Vector3(0, 1, Camera.main.nearClipPlane)).x)
            {
                toRemove.Add(chunk);
            }
        }

        if (spawnNewChunk)
        {
            UnityEngine.Random.InitState(levelConfig.randomSeed);
            int chunkIndex = UnityEngine.Random.Range(0, chunks.Count);

            if (debugForceChunkIndex > -1) chunkIndex = debugForceChunkIndex;

            ChunkBounds[] chunkBounds = chunks[chunkIndex].GetComponentsInChildren<ChunkBounds>();
            float chunkOffset = 0f;
            foreach (ChunkBounds bounds in chunkBounds)
            {
                if (bounds.chunkStart)
                {
                    chunkOffset = -bounds.transform.localPosition.x;
                }
            }
            Vector2 spawnPos = new Vector2(transform.position.x + chunkOffset, transform.position.y);
            GameObject newChunk = Instantiate(chunks[chunkIndex], spawnPos, Quaternion.identity, transform);

            if (milestoneChunkCounter >= levelConfig.milestoneInterval)
            {
                ChunkBounds[] newChunkBounds = newChunk.GetComponentsInChildren<ChunkBounds>();
                foreach (ChunkBounds bounds in newChunkBounds)
                {
                    if (bounds.chunkStart)
                    {
                        bounds.isMilestone = true;
                    }
                }
                milestoneChunkCounter = 0;
            }
            else
            {
                milestoneChunkCounter++;
            }

            activeChunks.Add(newChunk);
            levelConfig.randomSeed++;
        }

        foreach (GameObject chunk in toRemove)
        {
            activeChunks.Remove(chunk);
            Destroy(chunk);
        }
    }

    void FixedUpdate() // Atleast the move needs to be in FixedUpdate to work correctly, just keeping it all in here for now
    {
        if (levelConfig != null)
        {
            //ChunkSpawnerUpdateOld(); // TODO: Replace with ChunkSpawnerUpdate() when there are enough chunks for it to work correctly.
            ChunkSpawnerUpdate();
        }
    }

    public void DestroyChunks()
    {
        foreach(GameObject chunk in activeChunks)
        {
            Destroy(chunk);
        }
        activeChunks.Clear();
    }

    public void SetLevelKey(string levelKey)
    {
        this.levelKey = levelKey;
    }

    public string GetLevelKey()
    {
        return levelKey;
    }

    public GameObject LastChunk()
    {
        if (activeChunks.Count > 0)
        {
            return activeChunks[activeChunks.Count - 1];
        }
        return null;
    }

    public bool IsKnownSkill(Player.Skill skill)
    {
        return knownSkills.Contains(skill);
    }
}
