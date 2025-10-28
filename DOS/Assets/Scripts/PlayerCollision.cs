using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerCollision : MonoBehaviour
{
    public TextMeshProUGUI missionFailedText;
    [SerializeField] private GameObject missionFailedPanel;

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
            missionFailedPanel.SetActive(true);

            // Freeze the game
            Time.timeScale = 0f;
        }
    }

    // Called by LaserGun when laser hits player
    public void LaserHit()
    {
        bool isInvincible = playerController != null && playerController.IsInvincible;

        if (!isFailing && !isInvincible)
        {
            isFailing = true;
            missionFailedText.text = "MISSION FAILED";
            missionFailedPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }
}