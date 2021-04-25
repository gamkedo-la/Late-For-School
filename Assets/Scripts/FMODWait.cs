using UnityEngine;
using UnityEngine.SceneManagement;

public class FMODWait : MonoBehaviour
{
    public string bankToLoad;
    public string sceneToLoad;

    void Update()
    {
        if (FMODUnity.RuntimeManager.HasBankLoaded(bankToLoad))
        {
            Debug.Log("Master Bank Loaded");
            SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Single);
        }
    }
}
