using System.Collections.Generic;
using UnityEngine;

public class ChunkSpawner : MonoBehaviour
{
    public int randomSeed;
    public float speed = 2.5f;
    public List<Chunk> chunks;

    private List<Chunk> activeChunks = new List<Chunk>();

    void Update()
    {
        bool spawnNewChunk = true;
        foreach (Chunk chunk in activeChunks)
        {
            // Move chunks
            Vector2 newPos = chunk.contents.transform.position;
            newPos.x -= speed * Time.deltaTime;
            chunk.contents.transform.position = newPos;

            // Prevent spawning new chunk if there is one in the way
            float diff = Mathf.Abs(transform.position.x - newPos.x);
            if (diff < chunk.width/2)
            {
                spawnNewChunk = false;
            }
        }

        if (spawnNewChunk)
        {
            Random.InitState(randomSeed);
            int chunkIndex = Random.Range(0, chunks.Count);

            Debug.Log(chunkIndex);

            Chunk newChunk = new Chunk();
            newChunk.width = chunks[chunkIndex].width;
            Vector2 spawnPos = new Vector2(transform.position.x + newChunk.width / 2, transform.position.y);
            newChunk.contents = Instantiate(chunks[chunkIndex].contents, spawnPos, Quaternion.identity, transform);

            activeChunks.Add(newChunk);
            randomSeed++;
        }

        // If a chunk's full width is offscreen, remove the chunk

    }
}
