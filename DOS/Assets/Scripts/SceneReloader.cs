using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneReloader : MonoBehaviour
{
    [Tooltip("Key used to restart the current scene.")]
    public KeyCode restartKey = KeyCode.R;

    void Update()
    {
        if (Input.GetKeyDown(restartKey))
        {
            RestartScene();
        }
    }

    private void RestartScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}
