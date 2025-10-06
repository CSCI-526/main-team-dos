using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerCollision : MonoBehaviour
{
    public TextMeshProUGUI missionFailedText;
    private bool isFailing = false;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isFailing && collision.collider.CompareTag("Enemy"))
        {
            isFailing = true;

            // Show Mission Failed
            missionFailedText.text = "MISSION FAILED";

            // Freeze everything in the scene
            Time.timeScale = 0f;

            // Restart after short delay (needs unscaled time)
            StartCoroutine(RestartScene());
        }
    }

    private System.Collections.IEnumerator RestartScene()
    {
        yield return new WaitForSecondsRealtime(2f); // wait 2s in real time
        Time.timeScale = 1f; // unfreeze
        SceneManager.LoadScene("Starter"); // reload starter scene
    }
}
