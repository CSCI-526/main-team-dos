using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float maxFallSpeed = 15f;
    public float speed = 5f;
    public float jumpForce = 5f;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private bool isGrounded = false;
    private bool _controlsOverriddenByPortal = false;

    private bool facingRight = true;
    private Transform portalGunTransform;

    public void OnTeleport()
    {
        _controlsOverriddenByPortal = true;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;

        // Get the portal gun
        portalGunTransform = GetComponentInChildren<PortalGun>()?.transform;
    }

    void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // If portal has overridden controls AND the player is grounded
        if (_controlsOverriddenByPortal && isGrounded)
        {
            // Stop the horizontal sliding from the portals momentum.
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

            // Give control back to the player immediately.
            _controlsOverriddenByPortal = false;
        }

        // Clamp fall speed (existing logic).
        if (rb.linearVelocity.y < -maxFallSpeed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -maxFallSpeed);
        }
    }

    void Update()
    {
        // If controls are overridden, check if the player wants to take back control mid-air.
        if (_controlsOverriddenByPortal)
        {
            bool playerIsTryingToMove = Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1f || Input.GetButtonDown("Jump");
            if (playerIsTryingToMove)
            {
                _controlsOverriddenByPortal = false;
            }
        }

        // If controls are still overridden, don't process normal input.
        if (_controlsOverriddenByPortal) return;

        // Normal movement logic.
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(horizontalInput * speed, rb.linearVelocity.y);

        if (horizontalInput > 0 && !facingRight)
        {
            Flip();
        }
        else if (horizontalInput < 0 && facingRight)
        {
            Flip();
        }

        if ((Input.GetButtonDown("Jump") || Input.GetAxisRaw("Vertical") > 0.5f) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    void Flip()
    {
        facingRight = !facingRight;

        // Flip player sprite
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;

        // Flip the gun properly
        if (portalGunTransform != null)
        {
            Vector3 gunScale = portalGunTransform.localScale;

            // Mirror horizontally
            gunScale.x *= -1;

            // Mirror vertically too to keep top/bottom consistent
            gunScale.y *= -1;

            portalGunTransform.localScale = gunScale;

            // Flip Raycast origin position / Helps get the aim on the end of the gun.
            var gun = portalGunTransform.GetComponent<PortalGun>();
            if (gun != null && gun.raycastOrigin != null)
            {
                Vector3 originLocalPos = gun.raycastOrigin.localPosition;
                originLocalPos.x = -originLocalPos.x;
                gun.raycastOrigin.localPosition = originLocalPos;
            }
        }
    }

    public bool IsFacingRight()
    {
        return facingRight;
    }
}