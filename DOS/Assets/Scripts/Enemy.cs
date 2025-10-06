using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float moveSpeed = 2f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckDistance = 1f;
    public Transform wallCheckLeft;
    public Transform wallCheckRight;
    public float wallCheckDistance = 0.1f;
    public float flipCooldown = 1f;
    private Rigidbody2D rb;
    private bool movingRight = true;
    private float lastFlipTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2((movingRight ? 1 : -1) * moveSpeed, rb.linearVelocity.y);

        // Ground check
        Vector3 checkPos = groundCheck.position;
        checkPos.x += movingRight ? 1f : -1f; 
        RaycastHit2D groundInfo = Physics2D.BoxCast(
            checkPos,
            new Vector2(0.5f, 0.1f),
            0f,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );


        if (groundInfo.collider == null)
        {
            Flip();
        }

        // Left / Right Wall Checks
        RaycastHit2D leftHit = Physics2D.Raycast(wallCheckLeft.position, Vector2.left, wallCheckDistance, groundLayer);
        RaycastHit2D rightHit = Physics2D.Raycast(wallCheckRight.position, Vector2.right, wallCheckDistance, groundLayer);

        if (leftHit.collider != null)
        {
            Flip();
        }
        else if (rightHit.collider != null)
        {
            Flip();
        }
    }

    private void Flip()
    {

        if (Time.time - lastFlipTime < flipCooldown)
            return;

        movingRight = !movingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;

        lastFlipTime = Time.time;
    }
}
