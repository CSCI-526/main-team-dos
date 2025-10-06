using System.Collections.Generic;
using UnityEngine;

public class GateTrigger : MonoBehaviour
{
    [SerializeField] private GameObject gate;

    private int objectsInside = 0;
    
    private AmplitudeAnalytics amplitude;

    void Awake()
    {
        amplitude = FindFirstObjectByType<AmplitudeAnalytics>();
        if (amplitude == null)
        {
            amplitude = gameObject.AddComponent<AmplitudeAnalytics>();
        }
    }

    private bool CanTriggerGate(Collider2D other)
    {
        return other.CompareTag("Player") ||
               other.CompareTag("MovableObstacle") ||
               other.CompareTag("Enemy");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (CanTriggerGate(other))
        {
            objectsInside++;
            if (gate != null)
            {
                gate.SetActive(false);

                // Gate Trigger Analytics
                var eventProperties = new Dictionary<string, object>
                {
                    { "object_type", other.tag } // Check what Object triggered the Gate
                };
                amplitude.LogEvent("gate_trigger", eventProperties);

            }
            else
            {
                Debug.Log("Gate not set");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (CanTriggerGate(other))
        {
            objectsInside = Mathf.Max(0, objectsInside - 1);
            if (objectsInside == 0 && gate != null)
                gate.SetActive(true);
        }
    }
}
