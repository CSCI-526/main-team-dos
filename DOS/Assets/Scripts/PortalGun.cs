using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PortalGun : MonoBehaviour
{
    // ... (All variables and Awake, Update, ShootBeam methods are unchanged) ...
    public Camera mainCamera;
    public GameObject bluePortalPrefab;
    public GameObject orangePortalPrefab;
    public Transform raycastOrigin;
    public float portalHalfWidth = 0.5f;
    public float portalDepth = 0.1f;
    public LayerMask portalableSurfaceLayer;
    
    [Header("Beam Effect")]
    public Color blueBeamColor = Color.cyan;
    public Color orangeBeamColor = Color.Lerp(Color.red, Color.yellow, 0.5f);
    public float beamDuration = 0.1f;
    private LineRenderer beamLine;

    private GameObject activeBluePortal;
    private GameObject activeOrangePortal;
    private AmplitudeAnalytics amplitude;

    void Awake()
    {
        amplitude = FindFirstObjectByType<AmplitudeAnalytics>();
        if (amplitude == null) { amplitude = gameObject.AddComponent<AmplitudeAnalytics>(); }
        
        beamLine = GetComponent<LineRenderer>();
        if (beamLine != null) { beamLine.enabled = false; }
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire1")) { ShootPortal(bluePortalPrefab, ref activeBluePortal); }
        if (Input.GetButtonDown("Fire2")) { ShootPortal(orangePortalPrefab, ref activeOrangePortal); }
        if (Input.GetKeyDown(KeyCode.R)) { DeleteAllPortals(); }
    }

    private IEnumerator ShootBeam(Vector3 startPoint, Vector3 endPoint, Color color)
    {
        if (beamLine == null) yield break;
        beamLine.enabled = true;
        beamLine.startColor = color;
        beamLine.endColor = color;
        beamLine.SetPosition(0, startPoint);
        beamLine.SetPosition(1, endPoint);
        yield return new WaitForSeconds(beamDuration);
        beamLine.enabled = false;
    }


    void ShootPortal(GameObject portalPrefab, ref GameObject activePortal)
    {
        // ... (This method is unchanged, it still calls AdjustAndValidatePlacement) ...
        Vector2 playerCenter = transform.parent.position;
        Vector2 gunTipPosition = raycastOrigin.position;
        Vector2 playerToGunDir = (gunTipPosition - playerCenter).normalized;
        float playerToGunDist = Vector2.Distance(playerCenter, gunTipPosition);
        RaycastHit2D clipCheckHit = Physics2D.Raycast(playerCenter, playerToGunDir, playerToGunDist, portalableSurfaceLayer);
        Vector2 effectiveRaycastOrigin = raycastOrigin.position;
        if (clipCheckHit.collider != null) { effectiveRaycastOrigin = clipCheckHit.point - (playerToGunDir * 0.01f); }
        Vector2 direction = raycastOrigin.right;
        int playerLayer = LayerMask.NameToLayer("Player");
        int portalLayer = LayerMask.NameToLayer("Portal");
        int shootLayerMask = ~((1 << playerLayer) | (1 << portalLayer));
        int obstructionCheckLayerMask = ~(1 << playerLayer);

        RaycastHit2D hit = Physics2D.Raycast(effectiveRaycastOrigin, direction, 100f, shootLayerMask);

        Vector3 beamStartPoint = raycastOrigin.position;
        Vector3 beamEndPoint = hit.collider != null ? (Vector3)hit.point : beamStartPoint + (Vector3)direction * 100f;
        Color beamColor = (portalPrefab == bluePortalPrefab) ? blueBeamColor : orangeBeamColor;
        StartCoroutine(ShootBeam(beamStartPoint, beamEndPoint, beamColor));
        
        if (hit.collider != null && hit.collider.CompareTag("PortalGround"))
        {
            Collider2D existingPortalCollider = null;
            if (activePortal != null)
            {
                existingPortalCollider = activePortal.GetComponent<Collider2D>();
                if (existingPortalCollider != null) { existingPortalCollider.enabled = false; }
            }
            
            Vector2 finalPosition;
            bool isValid = AdjustAndValidatePlacement(hit.point, hit.normal, obstructionCheckLayerMask, hit.collider, out finalPosition);
            
            if (existingPortalCollider != null) { existingPortalCollider.enabled = true; }

            if (isValid)
            {
                if (activePortal != null) Destroy(activePortal);
                activePortal = Instantiate(portalPrefab, finalPosition, Quaternion.identity);
                activePortal.transform.up = hit.normal;
                LinkPortals();
            }
        }
    }
    
    // --- NEW HELPER FUNCTION to check if a given point is fully on a surface ---
    private bool IsPositionOnSurface(Vector2 point, Vector2 normal, int layerMask)
    {
        Vector2 portalRightDir = new Vector2(normal.y, -normal.x);
        float castOffset = 0.1f;

        Vector2 rightPos = point + portalRightDir * portalHalfWidth;
        Vector2 leftPos = point - portalRightDir * portalHalfWidth;

        RaycastHit2D hitLeft = Physics2D.Raycast(leftPos + (normal * castOffset), -normal, castOffset * 2f, layerMask);
        RaycastHit2D hitRight = Physics2D.Raycast(rightPos + (normal * castOffset), -normal, castOffset * 2f, layerMask);

        return hitLeft.collider != null && hitRight.collider != null;
    }


    // --- MODIFIED AdjustAndValidatePlacement function ---
    private bool AdjustAndValidatePlacement(Vector2 point, Vector2 normal, int layerMask, Collider2D targetSurface, out Vector2 adjustedPoint)
    {
        adjustedPoint = point; 
        Vector2 portalRightDir = new Vector2(normal.y, -normal.x);

        // --- Step 1: Handle hanging off an edge (same as before) ---
        if (!IsPositionOnSurface(point, normal, layerMask))
        {
            // Propose a nudged position
            Vector2 rightPos = point + portalRightDir * portalHalfWidth;
            Vector2 leftPos = point - portalRightDir * portalHalfWidth;
            float castOffset = 0.1f;
            RaycastHit2D hitLeft = Physics2D.Raycast(leftPos + (normal * castOffset), -normal, castOffset * 2f, layerMask);
            RaycastHit2D hitRight = Physics2D.Raycast(rightPos + (normal * castOffset), -normal, castOffset * 2f, layerMask);

            if (hitLeft.collider == null && hitRight.collider != null) { adjustedPoint = point + portalRightDir * portalHalfWidth; }
            else if (hitRight.collider == null && hitLeft.collider != null) { adjustedPoint = point - portalRightDir * portalHalfWidth; }
            else { return false; } // Surface too narrow

            // Re-validate the nudged position
            if (!IsPositionOnSurface(adjustedPoint, normal, layerMask)) { return false; }
        }

        // --- Step 2: Check for obstructions, with new corner-handling logic ---
        float portalWidth = portalHalfWidth * 2;
        Vector2 boxCenter = adjustedPoint - (normal * (portalDepth / 2));
        Vector2 boxSize = new Vector2(portalWidth, portalDepth);
        float angle = Vector2.SignedAngle(Vector2.up, normal);
        Collider2D[] obstructions = Physics2D.OverlapBoxAll(boxCenter, boxSize, angle, layerMask);

        foreach (Collider2D col in obstructions)
        {
            if (col != targetSurface)
            {
                // Is the obstruction another portalable wall? If so, we're at an inside corner.
                if (col.CompareTag("PortalGround"))
                {
                    // Find the closest point on the obstructing wall to our portal's center.
                    Vector2 closestPointOnObstacle = col.ClosestPoint(adjustedPoint);
                    // Determine the direction to push the portal along its surface.
                    Vector2 pushDirection = (adjustedPoint - closestPointOnObstacle).normalized;
                    float dot = Vector2.Dot(pushDirection, portalRightDir);
                    Vector2 finalPushDirection = (dot > 0) ? portalRightDir : -portalRightDir;
                    
                    // Propose a new position by nudging it away from the corner.
                    // The distance is the amount of overlap plus a tiny buffer.
                    float overlap = portalHalfWidth - Vector2.Distance(adjustedPoint, closestPointOnObstacle);
                    Vector2 finalNudgedPoint = adjustedPoint + finalPushDirection * (overlap + 0.01f);

                    // Final check: Is this newly nudged position fully on the surface?
                    if (IsPositionOnSurface(finalNudgedPoint, normal, layerMask))
                    {
                        adjustedPoint = finalNudgedPoint;
                        // One last check for other obstructions at the new spot
                        Collider2D[] finalObstructions = Physics2D.OverlapBoxAll(finalNudgedPoint - (normal * (portalDepth / 2)), boxSize, angle, layerMask);
                        foreach (var finalCol in finalObstructions) { if (finalCol != targetSurface) return false; }
                        return true;
                    }
                    else
                    {
                        return false; // Nudging failed.
                    }
                }
                else
                {
                    // It's a non-portalable obstruction (like an enemy). Fail.
                    return false;
                }
            }
        }
        
        return true;
    }

    void LinkPortals()
    {
        if (activeBluePortal != null && activeOrangePortal != null)
        {
            activeBluePortal.GetComponent<Portal>().destinationPortal = activeOrangePortal.transform;
            activeOrangePortal.GetComponent<Portal>().destinationPortal = activeBluePortal.transform;
        }
    }

    void DeleteAllPortals()
    {
        if (activeBluePortal != null) Destroy(activeBluePortal);
        if (activeOrangePortal != null) Destroy(activeOrangePortal);
    }
}