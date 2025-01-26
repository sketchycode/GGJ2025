using UnityEngine;
using UnityEngine.SceneManagement;

public class ScreenLoader : MonoBehaviour
{
    public string sceneToLoad; 
    private float startTime; 
    private bool isLoading = false;

    void Start()
    {
        startTime = Time.time; 
    }

    void Update()
    {
        // Check if the delay has passed, to avoid rage clicking through it
        if (Time.time - startTime >= 1.2f) 
        {
            // Check if any key is pressed
            if (Input.anyKey && !isLoading)
            {
                isLoading = true;
                // Load the specified scene
                SceneManager.LoadScene(sceneToLoad);
            }
        }
    }
}
