using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkBounds : MonoBehaviour
{
    public bool chunkStart = true;
    public bool chunkEnd = false;
    public bool isMilestone = false;

    private bool chunkStartPrevValue;
    private bool chunkEndPrevValue;
    private bool hasPoppedMilestone = false;

    private void OnValidate()
    {
        // Only allow either chunk start or chunk end to be selected. Should create custom editor for this, but did it this way for now
        if (chunkStart != chunkStartPrevValue)
        {
            chunkStartPrevValue = chunkStart;
            chunkEnd = !chunkStart;
            chunkEndPrevValue = chunkEnd;
        }

        if (chunkEnd != chunkEndPrevValue)
        {
            chunkEndPrevValue = chunkEnd;
            chunkStart = !chunkEnd;
            chunkStartPrevValue = chunkStart;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.CompareTag("Player") &&
           chunkStart &&
           isMilestone &&
           !hasPoppedMilestone)
        {
            Debug.Log("Hooray! YOU ROCK! **crows fly in the distance**");
            hasPoppedMilestone = true; // Stops player from triggering milestone more than once
        }
    }
}
