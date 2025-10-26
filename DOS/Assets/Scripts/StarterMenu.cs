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

    public void LoadLevel1()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Level_1");
    }

    public void LoadLevel2()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Level_2");
    }

    public void LoadLevel3()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Level_3");
    }

    public void LoadLevel4()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Level_4");
    }

    public void LoadLevelSelector()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Level_Select");
    }

    public void LoadLeaderboard()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Leaderboard");
    }

    // Loads Tutorial 1
    public void LoadTutorial1()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Tutorial_1");
    }

    // Loads Tutorial 2
    public void LoadTutorial2()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Tutorial_2");
    }

    // Loads Starter Page
    public void LoadStarterPage()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Starter");
    }

    public void LoadControls()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Controls");
    }

    // Generic Quit
    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
}
