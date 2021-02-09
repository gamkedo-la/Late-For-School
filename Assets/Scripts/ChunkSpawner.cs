using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ChunkSpawner : MonoBehaviour
{
    public int randomSeed;
    public float speed = 2.5f;
    public float offScreenX = -10f;
    public List<GameObject> chunks;
    public int milestoneInterval = 1;
    public Text seedInput;

    private List<GameObject> activeChunks = new List<GameObject>();
    private int milestoneChunkCounter;

    void FixedUpdate() // Atleast the move needs to be in FixedUpdate to work correctly, just keeping it all in here for now
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
            if (chunkEndX < offScreenX)
            {
                toRemove.Add(chunk);
            }
        }

        if (spawnNewChunk)
        {
            UnityEngine.Random.InitState(randomSeed);
            int chunkIndex = UnityEngine.Random.Range(0, chunks.Count);

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
            
            if(milestoneChunkCounter >= milestoneInterval)
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
            } else
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

    public void UpdateSeedFromUI() {
        try {
            randomSeed = Int32.Parse(seedInput.text);
        } catch (FormatException) {
            randomSeed = 42;
        }
        Debug.Log("starting with seed from UI:" + randomSeed);
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
