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
            if (collision.collider.CompareTag("Enemy"))
            {
                Enemy enemy = collision.collider.GetComponent<Enemy>();
                if (enemy != null && enemy.IsHarmless)
                {
                    return; 
                }
            }


            isFailing = true;

            // Disable the large background (just don't remove this comment please it helped me disable once in for all for all the levels, there's a mixup for our mission fail text let this be here for now)
            if (missionFailedText != null)
            {
                missionFailedText.gameObject.SetActive(false);
            }

            // Show and configure the mission failed panel
            missionFailedPanel.SetActive(true);
            
            // Change panel background to RED
            SpriteRenderer panelSprite = missionFailedPanel.GetComponent<SpriteRenderer>();
            if (panelSprite != null)
            {
                panelSprite.color = new Color(0.6f, 0f, 0f, 0.8f);
            }
            
            // Find and update the heading text
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
                        headingText.fontSize = 50; // Make title bigger
                    }
                }
            }

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
            
            // Disable the large background mission fail text (don't remove this comment please it helped me disable once in for all for all the levels, there's a mixup for our mission fail text let this be)
            if (missionFailedText != null)
            {
                missionFailedText.gameObject.SetActive(false);
            }
            
            // Show and configure the mission failed panel
            missionFailedPanel.SetActive(true);
            
            // Change panel background to RED
            SpriteRenderer panelSprite = missionFailedPanel.GetComponent<SpriteRenderer>();
            if (panelSprite != null)
            {
                panelSprite.color = new Color(1f, 0f, 0f, 1f); // Red color
            }
            
            // Find and update the heading text
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
}