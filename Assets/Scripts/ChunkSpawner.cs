using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ChunkSpawner : MonoBehaviour
{
    public int randomSeed;
    public float speed = 2.5f;
    public List<GameObject> chunks;
    public int milestoneInterval = 1;
    public Text seedInput;
    [Range(0.0f, 10.0f)]
    public float maxIntensity;
    public List<Player.Skill> includedSkills;
    public bool includeTutorialChunks;

    [Header("Debug Only (set to -1 for normal gameplay)")]
    [Tooltip("Only spawn this one chunk over and over:")]
    public int debugForceChunkIndex = -1;

    private List<GameObject> activeChunks = new List<GameObject>();
    private int milestoneChunkCounter;
    private List<Player.Skill> knownSkills = new List<Player.Skill>();
    private List<GameObject> usableChunks = new List<GameObject>();
    private List<GameObject> unusableChunks = new List<GameObject>();

    private static ChunkSpawner instance;
    public static ChunkSpawner GetInstance()
    {
        return instance;
    }

    private void Awake()
    {
        instance = this;

        // Setup starting chunk
        if (includeTutorialChunks)
        {
            // Only have jump tutorial as first chunk
            unusableChunks.AddRange(chunks);
            foreach (GameObject chunk in unusableChunks)
            {
                Chunk chunkDetails = chunk.GetComponent<Chunk>();
                if (chunkDetails.tutorialChunk &&
                    chunkDetails.includedSkills.Count == 1 &&
                    chunkDetails.includedSkills.Contains(Player.Skill.Jump))
                {
                    usableChunks.Add(chunk);
                }
            }
            if (unusableChunks.Count <= 0)
            {
                Debug.Log("No tutorial jump chunk provided");
                // TODO: Prevent going past this point - this should be an error.
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
            // TODO: Do logic for includeTutorialChunks = false

            Chunk chunkDetails = chunk.GetComponent<Chunk>();

            // Don't add chunk if it already exists
            if (usableChunks.Contains(chunk))
            {
                include = false;
            }

            if (includeTutorialChunks)
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
                    // Don't add a tutorial chunk if we already know all the included skills
                    bool allSkillsKnown = true;
                    foreach (Player.Skill skill in chunkDetails.includedSkills)
                    {
                        if (!knownSkills.Contains(skill)) { allSkillsKnown = false; }
                    }
                    if (allSkillsKnown) { include = false; }
                }
            }
            else
            {
                // Don't include any tutorial chunks
                if (chunkDetails.tutorialChunk) { include = false; }
            }

            // Don't add chunks with a higher intensity than what is set to max
            if (chunkDetails.intensity > maxIntensity) { include = false; }

            if (include) { usableChunks.Add(chunk); }
        }

        // Remove chunks that are no longer usable
        foreach (GameObject chunk in usableChunks)
        {
            bool remove = false;

            Chunk chunkDetails = chunk.GetComponent<Chunk>();

            if (includeTutorialChunks && !chunkDetails.tutorialChunk)
            {
                // Don't add a tutorial chunk if we already know all the included skills
                bool allSkillsKnown = true;
                foreach (Player.Skill skill in chunkDetails.includedSkills)
                {
                    if (!knownSkills.Contains(skill)) { allSkillsKnown = false; }
                }
                if (allSkillsKnown) { remove = true; }
            }

            if (remove) { usableChunks.Remove(chunk); }
        }
    }

    private GameObject ChooseNextChunk()
    {
        // TODO: choose a chunk based on recent intensity & if we still have moves left to teach
        return null;
    }

    private void ChunkSpawnerUpdate()
    {
        List<GameObject> toRemove = new List<GameObject>();

        bool spawnNewChunk = true;
        foreach (GameObject chunk in activeChunks)
        {
            // Move chunks
            Vector2 newPos = chunk.transform.position;
            newPos.x -= speed * Time.deltaTime;
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
            UnityEngine.Random.InitState(randomSeed);
            int chunkIndex = UnityEngine.Random.Range(0, usableChunks.Count);

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

            if (milestoneChunkCounter >= milestoneInterval)
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
            randomSeed++;
        }

        foreach (GameObject chunk in toRemove)
        {
            activeChunks.Remove(chunk);
            Destroy(chunk);
        }

        // TODO Add to known skills list

        UpdateUsableChunks();
    }

    private void ChunkSpawnerUpdateOld()
    {
        List<GameObject> toRemove = new List<GameObject>();

        bool spawnNewChunk = true;
        foreach (GameObject chunk in activeChunks)
        {
            // Move chunks
            Vector2 newPos = chunk.transform.position;
            newPos.x -= speed * Time.deltaTime;
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
            UnityEngine.Random.InitState(randomSeed);
            int chunkIndex = UnityEngine.Random.Range(0, chunks.Count);

            if (debugForceChunkIndex != -1) chunkIndex = debugForceChunkIndex;

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

            if (milestoneChunkCounter >= milestoneInterval)
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
            randomSeed++;
        }

        foreach (GameObject chunk in toRemove)
        {
            activeChunks.Remove(chunk);
            Destroy(chunk);
        }
    }

    void FixedUpdate() // Atleast the move needs to be in FixedUpdate to work correctly, just keeping it all in here for now
    {
        ChunkSpawnerUpdateOld(); // TODO: Replace with ChunkSpawnerUpdate() when there are enough chunks for it to work correctly.
    }

    public void UpdateSeedFromUI() {
        if (seedInput != null)
        {
            try
            {
                randomSeed = Int32.Parse(seedInput.text);
                Debug.Log("Starting with seed from UI:" + randomSeed);
            }
            catch (FormatException)
            {
                Debug.Log("Starting with seed from Inspector:" + randomSeed);
            }
        }
        else
        {
            Debug.Log("Starting with seed from Inspector:" + randomSeed);
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
}
