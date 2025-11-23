using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerCollision : MonoBehaviour
{
    [SerializeField] private GameObject missionFailedPanel;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip deathSound;

    private bool isFailing = false;
    private PlayerController playerController;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        // Attempt to find audio source if not assigned
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        bool isHazardous = collision.collider.CompareTag("Enemy") || collision.collider.CompareTag("Hazard");
        bool isInvincible = playerController != null && playerController.IsInvincible;

        if (!isFailing && isHazardous && !isInvincible)
        {
            if (collision.collider.CompareTag("Enemy"))
            {
                Enemy enemy = collision.collider.GetComponent<Enemy>();
                if (enemy != null && enemy.IsHarmless)
                {
                    return; 
                }
            }

            HandleDeath(); // Refactored into a method
        }
    }

    public void LaserHit()
    {
        bool isInvincible = playerController != null && playerController.IsInvincible;

        if (!isFailing && !isInvincible)
        {
            HandleDeath();
        }
    }

    private void HandleDeath()
    {
        isFailing = true;

        // Play Death Sound
        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        missionFailedPanel.SetActive(true);
        
        SpriteRenderer panelSprite = missionFailedPanel.GetComponent<SpriteRenderer>();
        if (panelSprite != null)
        {
            panelSprite.color = new Color(0.6f, 0f, 0f, 0.8f);
        }
        
        Transform canvasTransform = missionFailedPanel.transform.Find("Canvas");
        if (canvasTransform != null)
        {
            Transform headingTransform = canvasTransform.Find("Heading");
            if (headingTransform != null)
            {
                TMP_Text headingText = headingTransform.GetComponent<TMP_Text>();
                if (headingText != null)
                {
                    headingText.text = "Mission Failed";
                    headingText.fontSize = 50; 
                }
            }
        }

        Time.timeScale = 0f;
    }
}