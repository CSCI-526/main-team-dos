using UnityEngine;
using UnityEngine.SceneManagement;

public class StarterMenu : MonoBehaviour
{
    public void StartGame()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene("Level_1"); 
    }
}
