using UnityEngine;
using System.Collections;

public class LaserGun : MonoBehaviour
{
    [Header("Laser Settings")]
    public float minAngle = -45f; // Minimum rotation angle
    public float maxAngle = 45f;  // Maximum rotation angle
    public float laserRange = 50f; 
    
    [Header("Timing")]
    public float warningDuration = 2.0f; 
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
    private bool isShooting = false;

    void Start()
    {
        // Create warning line renderer
        GameObject warningObj = new GameObject("WarningLine");
        warningObj.transform.SetParent(transform);
        warningObj.transform.localPosition = Vector3.zero;
        warningLine = warningObj.AddComponent<LineRenderer>();
        SetupLineRenderer(warningLine, warningColor, warningWidth);
        warningLine.enabled = false;
        
        GameObject laserObj = new GameObject("LaserLine");
        laserObj.transform.SetParent(transform);
        laserObj.transform.localPosition = Vector3.zero;
        laserLine = laserObj.AddComponent<LineRenderer>();
        SetupLineRenderer(laserLine, laserColor, laserWidth);
        laserLine.enabled = false;
        
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
    }

    IEnumerator ShootingCycle()
    {
        while (true)
        {
            // Wait for cooldown
            yield return new WaitForSeconds(cooldownDuration);
            
            // Pick a random angle
            float targetAngle = Random.Range(minAngle, maxAngle);
            transform.rotation = Quaternion.Euler(0, 0, targetAngle);
            
            
            yield return StartCoroutine(ShowWarning());
            
            
            yield return StartCoroutine(FireLaser());
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
        isShooting = true;
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
        isShooting = false;
    }

    void UpdateLaserLine(LineRenderer line, bool isDamaging)
    {
        Vector3 startPos = firePoint != null ? firePoint.position : transform.position;
        Vector3 direction = transform.right; // Gun points right in local space
        Vector3 endPos = startPos + direction * laserRange;
        
        line.SetPosition(0, startPos);
        line.SetPosition(1, endPos);
    }

    void CheckPlayerHit()
    {
        Vector3 startPos = firePoint != null ? firePoint.position : transform.position;
        Vector3 direction = transform.right;
        
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
    }
}