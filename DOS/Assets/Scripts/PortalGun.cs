using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
    
/// <summary>
/// PortalGun - shoots blue/orange portals, validates placement, and anchors portals to moving surfaces.
/// Drop-in replacement that adds anchoring + clean detach on destroy.
/// </summary>
public class PortalGun : MonoBehaviour
{
    public Camera mainCamera;
    public GameObject bluePortalPrefab;
    public GameObject orangePortalPrefab;
    public Transform raycastOrigin;
    public float portalHalfWidth = 0.5f;
    public float portalDepth = 0.1f;
    public LayerMask portalableSurfaceLayer;
    
    [Header("Beam Effect")]
    public Color blueBeamColor;
    public Color orangeBeamColor;
    public float beamDuration = 0.1f;
    private LineRenderer beamLine;

    private GameObject activeBluePortal;
    private GameObject activeOrangePortal;
    private AmplitudeAnalytics amplitude;

    public static event Action OnBluePortalCreated;
    public static event Action OnOrangePortalCreated;

    void Awake()
    {
        amplitude = FindFirstObjectByType<AmplitudeAnalytics>();
        if (amplitude == null) { amplitude = gameObject.AddComponent<AmplitudeAnalytics>(); }
        
        beamLine = GetComponent<LineRenderer>();
        if (beamLine != null) { beamLine.enabled = false; }

        ColorUtility.TryParseHtmlString("#0D0D8C", out blueBeamColor);
        ColorUtility.TryParseHtmlString("#FF6E00", out orangeBeamColor);
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire1") || Input.GetKeyDown(KeyCode.C)) { ShootPortal(bluePortalPrefab, ref activeBluePortal); }
        if (Input.GetButtonDown("Fire2") || Input.GetKeyDown(KeyCode.V)) { ShootPortal(orangePortalPrefab, ref activeOrangePortal); }
        if (Input.GetKeyDown(KeyCode.R))   { DeleteAllPortals(); }
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
        Vector2 playerCenter = transform.parent.position;
        Vector2 gunTipPosition = raycastOrigin.position;
        Vector2 playerToGunDir = (gunTipPosition - playerCenter).normalized;
        float playerToGunDist = Vector2.Distance(playerCenter, gunTipPosition);

        // Prevent clipping when gun tip starts inside geometry
        RaycastHit2D clipCheckHit = Physics2D.Raycast(playerCenter, playerToGunDir, playerToGunDist, portalableSurfaceLayer);
        Vector2 effectiveRaycastOrigin = raycastOrigin.position;
        if (clipCheckHit.collider != null)
        {
            effectiveRaycastOrigin = clipCheckHit.point - (playerToGunDir * 0.01f);
        }

        Vector2 direction = raycastOrigin.right;

        int playerLayer = LayerMask.NameToLayer("Player");
        int portalLayer = LayerMask.NameToLayer("Portal");

        // Don't hit player or existing portals when shooting
        int shootLayerMask = ~((1 << playerLayer) | (1 << portalLayer));
        // For placement validation, ignore only player
        int obstructionCheckLayerMask = ~(1 << playerLayer);

        RaycastHit2D hit = Physics2D.Raycast(effectiveRaycastOrigin, direction, 100f, shootLayerMask);

        // Beam VFX
        Vector3 beamStartPoint = raycastOrigin.position;
        Vector3 beamEndPoint   = hit.collider != null ? (Vector3)hit.point : beamStartPoint + (Vector3)direction * 100f;
        Color beamColor = (portalPrefab == bluePortalPrefab) ? blueBeamColor : orangeBeamColor;
        StartCoroutine(ShootBeam(beamStartPoint, beamEndPoint, beamColor));

        if (hit.collider != null && hit.collider.CompareTag("PortalGround"))
        {
            // Temporarily disable the existing portal collider so it doesn't block placement checks
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
                // Cleanly detach and remove the previous portal of this color
                if (activePortal != null)
                {
                    var existingPortalComp = activePortal.GetComponent<Portal>();
                    if (existingPortalComp != null) existingPortalComp.DetachAnchor();
                    Destroy(activePortal);
                    activePortal = null;
                }

                // Instantiate and orient the portal (portal face uses transform.up)
                activePortal = Instantiate(portalPrefab, finalPosition, Quaternion.identity);
                activePortal.transform.up = hit.normal;

                // Anchor the new portal to the impacted surface (so it follows moving platforms)
                var portalComp = activePortal.GetComponent<Portal>();
                if (portalComp != null)
                {
                    // Use the portal's rotation we just set and the validated final position
                    portalComp.AnchorTo(hit.collider.transform, (Vector3)finalPosition, activePortal.transform.rotation);
                }

                LinkPortals();

                // ---- Analytics ----
                var eventProperties = new Dictionary<string, object>
                {
                    { "x_position", hit.point.x },
                    { "y_position", hit.point.y },
                    { "surface_tag", hit.collider.tag },
                    { "surface_name", hit.collider.name }
                };
                if (portalPrefab == bluePortalPrefab)
                {
                    amplitude.LogEvent("shot_blue_portal", eventProperties);
                    OnBluePortalCreated?.Invoke();
                }
                else
                {
                    amplitude.LogEvent("shot_orange_portal", eventProperties);
                    OnOrangePortalCreated?.Invoke();
                }
            }
        }
    }

    private bool IsPositionOnSurface(Vector2 point, Vector2 normal, int layerMask)
    {
        Vector2 portalRightDir = new Vector2(normal.y, -normal.x);
        float castOffset = 0.1f;

        Vector2 rightPos = point + portalRightDir * portalHalfWidth;
        Vector2 leftPos  = point - portalRightDir * portalHalfWidth;

        RaycastHit2D hitLeft  = Physics2D.Raycast(leftPos  + (normal * castOffset), -normal, castOffset * 2f, layerMask);
        RaycastHit2D hitRight = Physics2D.Raycast(rightPos + (normal * castOffset), -normal, castOffset * 2f, layerMask);

        return hitLeft.collider != null && hitRight.collider != null;
    }

    private bool AdjustAndValidatePlacement(Vector2 point, Vector2 normal, int layerMask, Collider2D targetSurface, out Vector2 adjustedPoint)
    {
        adjustedPoint = point;
        Vector2 portalRightDir = new Vector2(normal.y, -normal.x);

        // Handle hanging off an edge
        if (!IsPositionOnSurface(point, normal, layerMask))
        {
            Vector2 rightPos = point + portalRightDir * portalHalfWidth;
            Vector2 leftPos  = point - portalRightDir * portalHalfWidth;
            float castOffset = 0.1f;

            RaycastHit2D hitLeft  = Physics2D.Raycast(leftPos  + (normal * castOffset), -normal, castOffset * 2f, layerMask);
            RaycastHit2D hitRight = Physics2D.Raycast(rightPos + (normal * castOffset), -normal, castOffset * 2f, layerMask);

            if (hitLeft.collider == null && hitRight.collider != null)      { adjustedPoint = point + portalRightDir * portalHalfWidth; }
            else if (hitRight.collider == null && hitLeft.collider != null) { adjustedPoint = point - portalRightDir * portalHalfWidth; }
            else                                                            { return false; } // Surface too narrow

            // Re-validate nudged position
            if (!IsPositionOnSurface(adjustedPoint, normal, layerMask)) { return false; }
        }

        // Check for obstructions (inside corners, other colliders)
        float portalWidth = portalHalfWidth * 2f;
        Vector2 boxCenter = adjustedPoint - (normal * (portalDepth / 2f));
        Vector2 boxSize   = new Vector2(portalWidth, portalDepth);
        float angle       = Vector2.SignedAngle(Vector2.up, normal);

        Collider2D[] obstructions = Physics2D.OverlapBoxAll(boxCenter, boxSize, angle, layerMask);
        foreach (Collider2D col in obstructions)
        {
            if (col == targetSurface) continue;

            // Another portalable wall -> inside corner case
            if (col.CompareTag("PortalGround"))
            {
                Vector2 closest = col.ClosestPoint(adjustedPoint);
                Vector2 pushDir = (adjustedPoint - closest).normalized;
                float dot = Vector2.Dot(pushDir, portalRightDir);
                Vector2 finalPushDir = (dot > 0) ? portalRightDir : -portalRightDir;

                float overlap = portalHalfWidth - Vector2.Distance(adjustedPoint, closest);
                Vector2 finalNudgedPoint = adjustedPoint + finalPushDir * (overlap + 0.01f);

                if (IsPositionOnSurface(finalNudgedPoint, normal, layerMask))
                {
                    adjustedPoint = finalNudgedPoint;
                    Collider2D[] finalObs = Physics2D.OverlapBoxAll(finalNudgedPoint - (normal * (portalDepth / 2f)), boxSize, angle, layerMask);
                    foreach (var c in finalObs) { if (c != targetSurface) return false; }
                    return true;
                }
                else
                {
                    return false; // Nudging failed
                }
            }
            else
            {
                // Non-portalable obstruction in the way
                return false;
            }
        }

        return true;
    }

    void LinkPortals()
    {
        if (activeBluePortal != null && activeOrangePortal != null)
        {
            var blue = activeBluePortal.GetComponent<Portal>();
            var orange = activeOrangePortal.GetComponent<Portal>();
            if (blue != null && orange != null)
            {
                blue.destinationPortal   = activeOrangePortal.transform;
                orange.destinationPortal = activeBluePortal.transform;
            }
        }
    }

    public void DeleteAllPortals()
    {
        if (activeBluePortal != null)
        {
            var p = activeBluePortal.GetComponent<Portal>();
            if (p != null) p.DetachAnchor();
            Destroy(activeBluePortal);
            activeBluePortal = null;
        }
        if (activeOrangePortal != null)
        {
            var p = activeOrangePortal.GetComponent<Portal>();
            if (p != null) p.DetachAnchor();
            Destroy(activeOrangePortal);
            activeOrangePortal = null;
        }
    }
}
