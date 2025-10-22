using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerCollision : MonoBehaviour
{
    public TextMeshProUGUI missionFailedText;
    private bool isFailing = false;
    
    
    private PlayerController playerController;

    
    void Start()
    {
        playerController = GetComponent<PlayerController>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the collided object has EITHER the "Enemy" tag OR the "Hazard" tag.
        bool isHazardous = collision.collider.CompareTag("Enemy") || collision.collider.CompareTag("Hazard");

        if (!isFailing && isHazardous)
        {
            isFailing = true;

            // Show Mission Failed
            missionFailedText.text = "MISSION FAILED";

            // Freeze everything in the scene
            Time.timeScale = 0f;

            // Restart after short delay 
            StartCoroutine(RestartScene());
        }
    }

    private System.Collections.IEnumerator RestartScene()
    {
        yield return new WaitForSecondsRealtime(2f);
        Time.timeScale = 1f;
        SceneManager.LoadScene("Starter");
    }
}