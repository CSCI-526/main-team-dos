using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


public class Portal : MonoBehaviour
{
    public Transform destinationPortal;
    [Tooltip("How far to push the object out of the portal on exit to prevent getting stuck.")]
    public float exitOffset = 0.1f;
    private bool isTeleporting = false;

    private AmplitudeAnalytics amplitude;

    public static event Action OnPlayerTeleport;
    public static event Action OnEnemyTeleport;

    void Awake()
    {
        amplitude = FindFirstObjectByType<AmplitudeAnalytics>();
        if (amplitude == null)
        {
            amplitude = gameObject.AddComponent<AmplitudeAnalytics>();
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
            other.CompareTag("Enemy");
    }
    
    
    private IEnumerator Teleport(Transform obj)
    {
        var rb = obj.GetComponent<Rigidbody2D>();
        if (rb == null) yield break;

        // Prevent immediate back-teleport
        destinationPortal.GetComponent<Portal>().isTeleporting = true;

        // Current velocity and speed 
        Vector2 currentVelocity = rb.linearVelocity;
        float currentSpeed = currentVelocity.magnitude;

        Vector3 newPosition = destinationPortal.position + (destinationPortal.up * exitOffset);

        // Exit portal normal 
        Vector2 exitNormal = destinationPortal.up;

        // Project velocity along exit normal while keeping momentum magnitude 
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
            obj.GetComponent<PlayerController>().OnTeleport();
        }

        // Teleport Event
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

        yield return new WaitForFixedUpdate();
    }
}