using System.Collections.Generic;
using UnityEngine;

public class ActiveDuringSomeGameStates : MonoBehaviour
{
    public List<GameManager.GameState> activeDuringTheseGameStates;
    public float onDelay; // adds delay to becoming visible
    public float offDelay; // adds delay to becoming invisible
    public bool onlyFirstChildren = false;

    private bool active;


    void Update()
    {
        active = false;

        foreach (GameManager.GameState state in activeDuringTheseGameStates)
        {
            if (GameManager.GetInstance().GetState() == state)
            {
                active = true;
            }
        }

        float delay;
        if (active) { delay = onDelay; }
        else { delay = offDelay; }

        Invoke("SetActive", delay);
    }

    private void SetActive()
    {
        Transform[] children = gameObject.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            if (child != transform && 
                (!onlyFirstChildren || (onlyFirstChildren && child.parent == transform)))
                child.gameObject.SetActive(active);
        }
    }
}
