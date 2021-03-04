using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    [Range(0.0f, 10.0f)]
    public float intensity;
    public bool tutorialChunk = false;
    public List<Player.Skill> includedSkills;
}
