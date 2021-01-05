using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class parallaxBackground : MonoBehaviour
{
    public float currentSpeed = 1f;
    public List<MeshRenderer> backgroundQuads;
    public List<float> backgroundSpeeds;

    void Update()
    {
        for (int i = 0; i < backgroundQuads.Count; i++)
        {      
            Vector2 uv = new Vector2((Time.time+777) * backgroundSpeeds[i] * currentSpeed, 0);
            backgroundQuads[i].material.mainTextureOffset = uv;
        }
    }
}
