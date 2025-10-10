using UnityEngine;
using System.Collections; // Added for Coroutines
using System.Collections.Generic;

public class PortalGun : MonoBehaviour
{
    public Camera mainCamera;
    public GameObject bluePortalPrefab;
    public GameObject orangePortalPrefab;
    public Transform raycastOrigin;
    public float portalHalfWidth = 0.5f;
    public float portalDepth = 0.1f;
    public LayerMask portalableSurfaceLayer;
    
    // --- NEW: Beam Effect Variables ---
    [Header("Beam Effect")]
    public Color blueBeamColor = Color.cyan;
    public Color orangeBeamColor = Color.Lerp(Color.red, Color.yellow, 0.5f);
    public float beamDuration = 0.1f;
    private LineRenderer beamLine;
    // ---

    private GameObject activeBluePortal;
    private GameObject activeOrangePortal;
    private AmplitudeAnalytics amplitude;

    void Awake()
    {
        amplitude = FindFirstObjectByType<AmplitudeAnalytics>();
        if (amplitude == null)
        {
            amplitude = gameObject.AddComponent<AmplitudeAnalytics>();
        }
        
        // --- NEW: Get the Line Renderer component ---
        beamLine = GetComponent<LineRenderer>();
        if (beamLine != null)
        {
            beamLine.enabled = false;
        }
        // ---
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire1")) { ShootPortal(bluePortalPrefab, ref activeBluePortal); }
        if (Input.GetButtonDown("Fire2")) { ShootPortal(orangePortalPrefab, ref activeOrangePortal); }
        if (Input.GetKeyDown(KeyCode.R)) { DeleteAllPortals(); }
    }

    // --- NEW: Coroutine to manage the beam's lifecycle ---
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
    // ---

    void ShootPortal(GameObject portalPrefab, ref GameObject activePortal)
    {
        // ... (all the existing player/gun position and layer mask logic remains the same)
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
        // ---

        RaycastHit2D hit = Physics2D.Raycast(effectiveRaycastOrigin, direction, 100f, shootLayerMask);

        // --- NEW: Beam Firing Logic ---
        Vector3 beamStartPoint = raycastOrigin.position;
        Vector3 beamEndPoint = hit.collider != null ? (Vector3)hit.point : beamStartPoint + (Vector3)direction * 100f;
        Color beamColor = (portalPrefab == bluePortalPrefab) ? blueBeamColor : orangeBeamColor;
        StartCoroutine(ShootBeam(beamStartPoint, beamEndPoint, beamColor));
        // ---
        
        if (hit.collider != null && hit.collider.CompareTag("PortalGround"))
        {
            // ... (The rest of your IsPlacementValid and portal instantiation logic remains the same)
            Collider2D existingPortalCollider = null;
            if (activePortal != null)
            {
                existingPortalCollider = activePortal.GetComponent<Collider2D>();
                if (existingPortalCollider != null) { existingPortalCollider.enabled = false; }
            }
            bool is_valid = IsPlacementValid(hit.point, hit.normal, obstructionCheckLayerMask, hit.collider);
            if (existingPortalCollider != null) { existingPortalCollider.enabled = true; }

            if (is_valid)
            {
                if (activePortal != null) Destroy(activePortal);
                activePortal = Instantiate(portalPrefab, hit.point, Quaternion.identity);
                activePortal.transform.up = hit.normal;
                LinkPortals();

                var eventProperties = new Dictionary<string, object>
                {
                    { "x_position", hit.point.x }, { "y_position", hit.point.y },
                    { "surface_tag", hit.collider.tag }, { "surface_name", hit.collider.name }
                };

                if (portalPrefab == bluePortalPrefab) { amplitude.LogEvent("shot_blue_portal", eventProperties); }
                else { amplitude.LogEvent("shot_orange_portal", eventProperties); }
            }
            // ---
        }
    }
    
    private bool IsPlacementValid(Vector2 point, Vector2 normal, int layerMask, Collider2D targetSurface)
    {
        Vector2 portalRight = new Vector2(normal.y, -normal.x);
        Vector2 rightPos = point + (portalRight * portalHalfWidth);
        Vector2 leftPos = point - (portalRight * portalHalfWidth);
        float castOffset = 0.1f;
        
        RaycastHit2D hitLeft = Physics2D.Raycast(leftPos + (normal * castOffset), -normal, castOffset * 2f, layerMask);
        RaycastHit2D hitRight = Physics2D.Raycast(rightPos + (normal * castOffset), -normal, castOffset * 2f, layerMask);

        bool surfaceIsWideEnough = hitLeft.collider != null && hitLeft.collider.CompareTag("PortalGround") &&
                                   hitRight.collider != null && hitRight.collider.CompareTag("PortalGround");

        if (!surfaceIsWideEnough)
        {
            return false;
        }

        float portalWidth = portalHalfWidth * 2;
        Vector2 boxCenter = point - (normal * (portalDepth / 2));
        Vector2 boxSize = new Vector2(portalWidth, portalDepth);
        float angle = Vector2.SignedAngle(Vector2.up, normal);

        Collider2D[] obstructions = Physics2D.OverlapBoxAll(boxCenter, boxSize, angle, layerMask);

        foreach (Collider2D col in obstructions)
        {
            if (col != targetSurface)
            {
                Debug.Log("Placement failed: Obstruction detected: " + col.name);
                return false;
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