using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Background : MonoBehaviour
{
    public float speedScale = 1f;
    public List<MeshRenderer> backgroundQuads;
    public List<float> backgroundSpeeds;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < backgroundQuads.Count; i++)
        {      
            // sprites can't update UV but 3d meshes can
            if (backgroundQuads[i]) {
                float currentscroll = Time.time * backgroundSpeeds[i] * speedScale;
                backgroundQuads[i].material.mainTextureOffset = new Vector2(currentscroll, 0);
            }
        }
    }
}
