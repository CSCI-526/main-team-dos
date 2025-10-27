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
        bool isHazardous = collision.collider.CompareTag("Enemy") || collision.collider.CompareTag("Hazard");

        bool isInvincible = playerController != null && playerController.IsInvincible;

        if (!isFailing && isHazardous && !isInvincible)
        {
            isFailing = true;

            // Show the "MISSION FAILED" text
            missionFailedText.text = "MISSION FAILED";

            // Freeze the game
            Time.timeScale = 0f;

            // Start the coroutine to restart the scene
            StartCoroutine(RestartScene());
        }
    }
    public void LaserHit()
    {
        bool isInvincible = playerController != null && playerController.IsInvincible;

        if (!isFailing && !isInvincible)
        {
            isFailing = true;
            missionFailedText.text = "MISSION FAILED";
            Time.timeScale = 0f;
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