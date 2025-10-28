using UnityEngine;

public class LaserGunEnemy : MonoBehaviour
{
    [Header("Movement Settings (Same as Enemy)")]
    public float moveSpeed = 2f;
    public LayerMask groundLayer;
    
    [Header("Collision Checks")]
    public Transform wallCheck;
    public float wallCheckDistance = 0.2f;
    public Transform groundCheck;
    public float groundCheckDistance = 0.5f;
    
    [Header("Flip Logic")]
    public float flipCooldown = 0.1f;
    
    [Header("Laser Gun Reference")]
    public LaserGun laserGun; // Reference to the LaserGun component
    
    private Rigidbody2D rb;
    private bool movingRight = true;
    private float lastFlipTime = -1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        
        // Auto-find LaserGun if not assigned
        if (laserGun == null)
        {
            laserGun = GetComponentInChildren<LaserGun>();
        }
    }

    void FixedUpdate()
    {
        // Movement logic (identical to Enemy.cs)
        rb.linearVelocity = new Vector2(movingRight ? moveSpeed : -moveSpeed, rb.linearVelocity.y);

        bool isHittingWall = IsHittingWall();
        bool isNearEdge = IsNearEdge();

        if (isHittingWall || isNearEdge)
        {
            Flip();
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // Enemy collision logic (same as Enemy.cs)
        if (other.CompareTag("Enemy") && Time.time - lastFlipTime >= flipCooldown)
        {
            if (other.transform.position.x > transform.position.x)
            {
                if (movingRight)
                {
                    Flip();
                }
            }
            else
            {
                if (!movingRight)
                {
                    Flip();
                }
            }
        }
    }

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
        
        // Flip the gun's Y scale so it always points in the movement direction
        if (laserGun != null)
        {
            Vector3 gunScale = laserGun.transform.localScale;
            gunScale.y *= -1;
            laserGun.transform.localScale = gunScale;
        }
    }

    void OnDrawGizmos()
    {
        // Debug visualization (same as Enemy.cs)
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