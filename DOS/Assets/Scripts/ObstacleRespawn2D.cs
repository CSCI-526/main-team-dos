using UnityEngine;

[DisallowMultipleComponent]
public class ObstacleRespawn2D : MonoBehaviour
{
    public KeyCode respawnKey = KeyCode.O;   // press O to reset

    Vector3 startPos;
    Quaternion startRot;
    Vector3 startScale;
    Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startPos   = transform.position;
        startRot   = transform.rotation;
        startScale = transform.localScale;
    }

    void Update()
    {
        if (Input.GetKeyDown(respawnKey))
            Respawn();
    }

    public void Respawn()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        transform.SetPositionAndRotation(startPos, startRot);
        transform.localScale = startScale;
    }
}
