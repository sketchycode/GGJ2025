using UnityEngine;
using UnityEngine.SceneManagement;

public class ScreenLoader : MonoBehaviour
{
    public string sceneToLoad; 

    void Update()
    {
        // Check if any key is pressed
        if (Input.anyKey)
        {
            // Load the specified scene
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
