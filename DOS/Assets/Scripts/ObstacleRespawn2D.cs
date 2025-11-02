using UnityEngine;

[DisallowMultipleComponent]
public class ObstacleRespawn2D : MonoBehaviour
{
    public KeyCode respawnKey = KeyCode.O;   // press O to reset
    public float maxSpeed = 17f;
    public float fallMultiplier = 2.5f;
    private Vector3 startPos;
    private Quaternion startRot;
    private Vector3 startScale;
    private Rigidbody2D rb;

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

    void FixedUpdate()
    {
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }

        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
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
