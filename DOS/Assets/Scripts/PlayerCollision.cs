using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerCollision : MonoBehaviour
{
    public TextMeshProUGUI missionFailedText;
    private bool isFailing = false;
    
    // --- NEW: Add a reference to the PlayerController ---
    private PlayerController playerController;

    // --- NEW: Get the reference in the Start method ---
    void Start()
    {
        playerController = GetComponent<PlayerController>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // --- MODIFIED: Added a check for player invincibility ---
        if (!isFailing && collision.collider.CompareTag("Enemy") && (playerController == null || !playerController.IsInvincible))
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