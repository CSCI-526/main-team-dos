using UnityEngine;
using UnityEngine.SceneManagement;

public class StarterMenu : MonoBehaviour
{
    // Loads Level 1
    public void StartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Level_1");  // Ensure your Level_1 scene name matches exactly
    }

    // Loads Tutorial 1
    public void LoadTutorial1()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Tut1_PortalsBasic");
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

    // Generic Quit
    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
}
