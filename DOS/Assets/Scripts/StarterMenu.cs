using UnityEngine;
using UnityEngine.SceneManagement;

public class StarterMenu : MonoBehaviour
{
    // Loads Tutorial 1
    public void LoadTutorial1()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Tut1_PortalsBasic");  // Make sure the scene name matches exactly
    }

    // Loads Tutorial 2
    public void LoadTutorial2()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Tut1_Enemy_Gate");
    }

    // Loads Starter Page
    public void LoadStarterPage()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("StarterPage");
    }

    // Generic Quit (optional)
    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
}
