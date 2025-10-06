using UnityEngine;
using UnityEngine.SceneManagement;

public class EscapeToStarter : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("Starter"); 
        }
    }
}
