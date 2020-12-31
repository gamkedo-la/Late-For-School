using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Background : MonoBehaviour
{
    public List<SpriteRenderer> backgroundSprites;
    public List<float> backgroundSpeeds;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < backgroundSprites.Count; i++)
        {      
            float currentscroll = Time.time * backgroundSpeeds[i];
            
            // appears to work
            // Debug.Log(Time.time + " bg layer " + i + " currentscroll: "+currentscroll);
            
            // no error.. but this seems to do nothing..
            backgroundSprites[i].material.mainTextureOffset = new Vector2(currentscroll, 0);
        }
    }
}
