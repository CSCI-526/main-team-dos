using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float moveSpeed = 2f;
    [Tooltip("Set this to your Ground/Platforms layer.")]
    public LayerMask groundLayer;

    [Header("Collision Checks")]
    public Transform wallCheck;
    public float wallCheckDistance = 0.2f;
    public Transform groundCheck;
    public float groundCheckDistance = 0.5f;

    [Header("Flip Logic")]
    public float flipCooldown = 0.1f; // A small cooldown is still good

    private Rigidbody2D rb;
    private bool movingRight = true;
    private float lastFlipTime = -1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(movingRight ? moveSpeed : -moveSpeed, rb.linearVelocity.y);

        bool isHittingWall = IsHittingWall();
        bool isNearEdge = IsNearEdge();

        if (isHittingWall || isNearEdge)
        {
            Flip();
        }
    }

    // --- MODIFICATION START ---
    // Swapped from OnTriggerEnter2D to OnTriggerStay2D
    private void OnTriggerStay2D(Collider2D other)
    {
        // Only react to other enemies, and only if the cooldown has passed
        if (other.CompareTag("Enemy") && Time.time - lastFlipTime >= flipCooldown)
        {
            // Determine the direction of the other enemy
            // Is the other enemy to my right?
            if (other.transform.position.x > transform.position.x)
            {
                // If the other enemy is on my right, I must move left.
                // If I am currently moving right, then I need to flip.
                if (movingRight)
                {
                    Flip();
                }
            }
            // Is the other enemy to my left?
            else
            {
                // If the other enemy is on my left, I must move right.
                // If I am currently moving left, then I need to flip.
                if (!movingRight)
                {
                    Flip();
                }
            }
        }
    }
    // --- MODIFICATION END ---

    private bool IsHittingWall()
    {
        Vector2 direction = movingRight ? Vector2.right : Vector2.left;
        RaycastHit2D hit = Physics2D.Raycast(wallCheck.position, direction, wallCheckDistance, groundLayer);
        return hit.collider != null;
    }

    private bool IsNearEdge()
    {
        RaycastHit2D hit = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, groundLayer);
        return hit.collider == null;
    }

    private void Flip()
    {
        if (Time.time - lastFlipTime < flipCooldown)
        {
            return;
        }

        lastFlipTime = Time.time;
        movingRight = !movingRight;
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (wallCheck != null)
        {
            Vector3 wallCheckDir = movingRight ? Vector3.right : Vector3.left;
            Gizmos.DrawLine(wallCheck.position, wallCheck.position + wallCheckDir * wallCheckDistance);
        }

        Gizmos.color = Color.green;
        if (groundCheck != null)
        {
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * groundCheckDistance);
        }
    }
}