using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Portal : MonoBehaviour
{
    [Header("Teleport")]
    public Transform destinationPortal;
    [Tooltip("How far to push the object out of the portal on exit to prevent getting stuck.")]
    public float exitOffset = 0.1f;
    private bool isTeleporting = false;

    private AmplitudeAnalytics amplitude;

    public static event Action OnPlayerTeleport;
    public static event Action OnEnemyTeleport;

    // --- Anchoring fields ---
    [Header("Anchoring (optional)")]
    [Tooltip("If non-null, the portal will follow this transform using a stored local offset/rotation.")]
    [SerializeField] private Transform anchorTransform = null;
    private Vector3 anchoredLocalPosition;
    private Quaternion anchoredLocalRotation;
    private bool anchored = false;

    void Awake()
    {
        amplitude = FindFirstObjectByType<AmplitudeAnalytics>();
        if (amplitude == null)
        {
            amplitude = gameObject.AddComponent<AmplitudeAnalytics>();
        }
    }

    void LateUpdate()
    {
        // Update portal position/rotation if anchored (LateUpdate avoids jitter for kinematic movers)
        if (anchored && anchorTransform != null)
        {
            transform.position = anchorTransform.TransformPoint(anchoredLocalPosition);
            transform.rotation = anchorTransform.rotation * anchoredLocalRotation;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (destinationPortal != null && !isTeleporting && CanTeleport(other))
        {
            StartCoroutine(Teleport(other.transform));
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (CanTeleport(other))
        {
            isTeleporting = false;
        }
    }

    private bool CanTeleport(Collider2D other)
    {
        return other.CompareTag("Player") ||
               other.CompareTag("MovableObstacle") ||
               other.CompareTag("LaserEnemy") ||
               other.CompareTag("Enemy");
    }

    private IEnumerator Teleport(Transform obj)
    {
        var rb = obj.GetComponent<Rigidbody2D>();
        if (rb == null) yield break;

        // Prevent immediate back-teleport
        var destPortalComp = destinationPortal.GetComponent<Portal>();
        if (destPortalComp != null) destPortalComp.isTeleporting = true;

        // Current velocity and speed 
        Vector2 currentVelocity = rb.linearVelocity;
        float currentSpeed = currentVelocity.magnitude;

        // Use destinationPortal.up as exit normal (assuming portal 'face' uses up)
        Vector3 newPosition = destinationPortal.position + (destinationPortal.up * exitOffset);
        Vector2 exitNormal = destinationPortal.up;

        // Project velocity along exit normal while keeping momentum magnitude (min speed fallback)
        Vector2 newVelocity = exitNormal.normalized * Mathf.Max(currentSpeed, 5f);

        // Move object safely
        RigidbodyType2D originalType = rb.bodyType;
        rb.bodyType = RigidbodyType2D.Kinematic;
        obj.position = newPosition;
        rb.bodyType = originalType;

        // Apply new projected velocity
        rb.linearVelocity = newVelocity;

        if (obj.CompareTag("Player"))
        {
            var player = obj.GetComponent<PlayerController>();
            if (player != null) player.OnTeleport();
        }
        
        else if (obj.CompareTag("Enemy"))
        {
            var enemy = obj.GetComponent<Enemy>();
            if (enemy != null) enemy.OnTeleport();
        }
        


        // Teleport Event & analytics
        if (amplitude != null)
        {
            var eventProperties = new Dictionary<string, object>
            {
                { "object_type", obj.tag } // Check what Object was teleported
            };
            if (obj.tag == "Player")
            {
                OnPlayerTeleport?.Invoke();
            }
            else if (obj.tag == "Enemy")
            {
                OnEnemyTeleport?.Invoke();
            }
            amplitude.LogEvent("teleport", eventProperties);
        }

        // Wait one physics step to avoid immediate re-triggering (and to let moving objects settle)
        yield return new WaitForFixedUpdate();
    }

 
    public void AnchorTo(Transform anchor, Vector3 worldHitPoint, Quaternion worldRotation)
    {
        if (anchor == null)
        {
            DetachAnchor();
            return;
        }

        anchorTransform = anchor;
        anchoredLocalPosition = anchorTransform.InverseTransformPoint(worldHitPoint);
        anchoredLocalRotation = Quaternion.Inverse(anchorTransform.rotation) * worldRotation;
        anchored = true;

        
        transform.SetParent(anchorTransform, worldPositionStays: true);
    }


    public void DetachAnchor()
    {
        anchored = false;
        anchorTransform = null;
        transform.SetParent(null, worldPositionStays: true);
    }

    
    public bool IsAnchored() => anchored && anchorTransform != null;
}