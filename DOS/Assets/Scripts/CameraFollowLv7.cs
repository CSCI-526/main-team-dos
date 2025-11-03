using UnityEngine;

public class CameraFollowLv7 : MonoBehaviour
{
    public Transform player; 
    public float offsetY = 2f; 
    public float thresholdY = 45f; 

    private Vector3 originalPosition;

    void Start()
    {
        originalPosition = transform.position;
    }

    void LateUpdate()
    {
        if (player == null) return;

        Vector3 newPos = originalPosition;

        if (player.position.y < thresholdY)
        {
            newPos.y = originalPosition.y - offsetY;
            Debug.Log("Player is below threshold. New target Y: " + newPos.y);
        }

        transform.position = Vector3.Lerp(transform.position, newPos, Time.deltaTime * 3f);
    }
}
