using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Background : MonoBehaviour
{
    public float speedScale = 1f;
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
            float currentscroll = Mathf.Repeat (Time.time * backgroundSpeeds[i],1); // 0..1 loop
            
            // appears to work
            // Debug.Log(Time.time + " bg layer " + i + " currentscroll: "+currentscroll);
            
            // no error.. but this seems to do nothing.. only works on a 3d material?!? not sprite
            backgroundSprites[i].material.mainTextureOffset = new Vector2(currentscroll, 0);

            // HACK FIXME - move the actual sprites lol =(
            float newX = Mathf.Repeat (Time.time * -1 * backgroundSpeeds[i] * speedScale,8);
            backgroundSprites[i].transform.position = new Vector2(newX,backgroundSprites[i].transform.position.y);
        }
    }
}
