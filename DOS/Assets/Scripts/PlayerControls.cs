using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float maxFallSpeed = 15f;
    public float speed = 5f;
    public float jumpForce = 5f;
    
    // Public variable for invincibility duration
    public float postTeleportInvincibility = 0.2f;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private bool isGrounded = false;
    private bool _controlsOverriddenByPortal = false;
    private int portalGraceFrames;
    private bool facingRight = true;
    private Transform portalGunTransform;

    public GameObject controlsHUD;

    // Public property to check invincibility state ---
    public bool IsInvincible { get; private set; } = false;

    public void OnTeleport()
    {
        _controlsOverriddenByPortal = true;
        portalGraceFrames = 2;
        // Start the invincibility coroutine ---
        StartCoroutine(InvincibilityCoroutine());
    }

    // Coroutine to manage invincibility frames ---
    private IEnumerator InvincibilityCoroutine()
    {
        IsInvincible = true;
        yield return new WaitForSeconds(postTeleportInvincibility);
        IsInvincible = false;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        portalGunTransform = GetComponentInChildren<PortalGun>()?.transform;
    }

    void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (portalGraceFrames > 0)
        {
            portalGraceFrames--;
        }
        else
        {
            if (_controlsOverriddenByPortal && isGrounded)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                _controlsOverriddenByPortal = false;
            }
        }

        if (rb.linearVelocity.y < -maxFallSpeed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -maxFallSpeed);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            bool isActive = controlsHUD.activeSelf;
            controlsHUD.SetActive(!isActive);
        }

        if (_controlsOverriddenByPortal)
        {
            if (portalGraceFrames > 0)
            {
                return;
            }
            
            bool playerIsTryingToMove = Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1f || Input.GetButtonDown("Jump");
            if (playerIsTryingToMove)
            {
                _controlsOverriddenByPortal = false;
            }
        }

        if (_controlsOverriddenByPortal) return;

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

        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;

        if (portalGunTransform != null)
        {
            Vector3 gunScale = portalGunTransform.localScale;
            gunScale.x *= -1;
            gunScale.y *= -1;
            portalGunTransform.localScale = gunScale;

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