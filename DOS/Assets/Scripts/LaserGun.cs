using UnityEngine;
using System.Collections;

public class LaserGun : MonoBehaviour
{
    [Header("Laser Settings")]
    public float minAngle = -30f; // Minimum rotation angle
    public float maxAngle = 30f;  // Maximum rotation angle
    public float laserRange = 50f; 
    
    [Header("Direction Settings")]
    public bool shootOnlyRight = true; // Set to true for enemies that should only shoot right
    
    [Header("Timing")]
    public float warningDuration = 3.0f; // INCREASED from 1.5f to 3.0f for better reaction time
    public float shootDuration = 0.3f;   
    public float cooldownDuration = 2f; 
    
    [Header("Visual Settings")]
    public Color warningColor = new Color(1f, 0f, 0f, 0.3f);
    public Color laserColor = new Color(1f, 0f, 0f, 1f);    
    public float warningWidth = 0.05f;
    public float laserWidth = 0.15f;
    
    [Header("References")]
    public Transform firePoint; 
    public LayerMask playerLayer; 
    
    private LineRenderer warningLine;
    private LineRenderer laserLine;

    void Start()
    {
        // Create warning line renderer
        GameObject warningObj = new GameObject("WarningLine");
        warningObj.transform.SetParent(transform);
        warningObj.transform.localPosition = Vector3.zero;
        warningLine = warningObj.AddComponent<LineRenderer>();
        SetupLineRenderer(warningLine, warningColor, warningWidth);
        warningLine.enabled = false;
        warningLine.useWorldSpace = true; // Use world space to prevent positioning issues
        
        GameObject laserObj = new GameObject("LaserLine");
        laserObj.transform.SetParent(transform);
        laserObj.transform.localPosition = Vector3.zero;
        laserLine = laserObj.AddComponent<LineRenderer>();
        SetupLineRenderer(laserLine, laserColor, laserWidth);
        laserLine.enabled = false;
        laserLine.useWorldSpace = true; // Use world space to prevent positioning issues
        
        StartCoroutine(ShootingCycle());
    }

    void SetupLineRenderer(LineRenderer line, Color color, float width)
    {
        line.positionCount = 2;
        line.startWidth = width;
        line.endWidth = width;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = color;
        line.endColor = color;
        line.sortingLayerName = "Default";
        line.sortingOrder = 10;
        line.useWorldSpace = true; // Ensure world space
        line.alignment = LineAlignment.TransformZ; // Use transform-based alignment
    }

    IEnumerator ShootingCycle()
    {
        while (true)
        {
            // Wait for cooldown
            yield return new WaitForSeconds(cooldownDuration);
            
            // Pick a random angle with proper constraints
            float targetAngle = GetRandomAngleInDirection();
            transform.rotation = Quaternion.Euler(0, 0, targetAngle);
            
            // Show warning
            yield return StartCoroutine(ShowWarning());
            
            // Fire laser
            yield return StartCoroutine(FireLaser());
        }
    }

    float GetRandomAngleInDirection()
    {
        if (shootOnlyRight)
        {
            // For shooting right, constrain angles between minAngle and maxAngle
            // This keeps the gun pointing generally to the right
            return Random.Range(minAngle, maxAngle);
        }
        else
        {
            // For shooting left, add 180 degrees to flip direction
            float angle = Random.Range(minAngle, maxAngle);
            return angle + 180f;
        }
    }

    IEnumerator ShowWarning()
    {
        warningLine.enabled = true;
        
        float elapsed = 0f;
        while (elapsed < warningDuration)
        {
            UpdateLaserLine(warningLine, false);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        warningLine.enabled = false;
    }

    IEnumerator FireLaser()
    {
        laserLine.enabled = true;
        
        float elapsed = 0f;
        while (elapsed < shootDuration)
        {
            UpdateLaserLine(laserLine, true);
            CheckPlayerHit();
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        laserLine.enabled = false;
    }

    void UpdateLaserLine(LineRenderer line, bool isDamaging)
    {
        Vector3 startPos = firePoint != null ? firePoint.position : transform.position;
        
        // Calculate direction based on gun's rotation
        // Use the gun's actual forward direction (transform.right for 2D)
        Vector3 direction = transform.right;
        
        // Ensure we're always shooting forward (positive X in world space when shootOnlyRight is true)
        if (shootOnlyRight && direction.x < 0)
        {
            // If somehow the direction is backwards, flip it
            direction = -direction;
        }
        
        Vector3 endPos = startPos + direction * laserRange;
        
        line.SetPosition(0, startPos);
        line.SetPosition(1, endPos);
    }

    void CheckPlayerHit()
    {
        Vector3 startPos = firePoint != null ? firePoint.position : transform.position;
        Vector3 direction = transform.right;
        
        // Ensure we're always shooting forward (positive X in world space when shootOnlyRight is true)
        if (shootOnlyRight && direction.x < 0)
        {
            // If somehow the direction is backwards, flip it
            direction = -direction;
        }
        
        RaycastHit2D hit = Physics2D.Raycast(startPos, direction, laserRange, playerLayer);
        
        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            PlayerCollision playerCollision = hit.collider.GetComponent<PlayerCollision>();
            if (playerCollision != null)
            {
                playerCollision.LaserHit();
            }
        }
    }

    void OnDrawGizmos()
    {
        if (firePoint == null) return;
        
        Gizmos.color = Color.red;
        Vector3 direction = transform.right;
        Gizmos.DrawRay(firePoint.position, direction * laserRange);
        
        // Draw shooting arc range
        Gizmos.color = Color.yellow;
        float angleRange = shootOnlyRight ? maxAngle - minAngle : maxAngle - minAngle;
        Vector3 center = firePoint.position;
        
        // Draw min and max angle lines
        Vector3 minDir = Quaternion.Euler(0, 0, minAngle) * Vector3.right;
        Vector3 maxDir = Quaternion.Euler(0, 0, maxAngle) * Vector3.right;
        Gizmos.DrawRay(center, minDir * laserRange * 0.5f);
        Gizmos.DrawRay(center, maxDir * laserRange * 0.5f);
    }
}