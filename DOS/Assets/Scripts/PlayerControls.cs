using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float maxFallSpeed = 15f;
    public float speed = 5f;

    [Header("Jump Physics")]
    [Tooltip("The initial velocity applied when jumping.")]
    public float jumpForce = 5f; 
    
    [Tooltip("Multiplier for gravity when falling.")]
    public float fallMultiplier = 2.5f; 
    [Tooltip("Multiplier for gravity when jump is released early.")]
    public float lowJumpMultiplier = 2f; 
    
    // Public variable for invincibility duration
    public float postTeleportInvincibility = 0.2f;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    // --- DECLARED HERE ---
    private Rigidbody2D rb;
    private bool isGrounded = false;
    private bool _controlsOverriddenByPortal = false;
    private int portalGraceFrames;
    private bool facingRight = true;
    private Transform portalGunTransform;

    public GameObject controlsHUD;
    
    private bool wJumpPressedLastFrame = false; 

    public bool IsInvincible { get; private set; } = false;

    // --- MOVED TO AWAKE() ---
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
    }

    public void OnTeleport()
    {
        _controlsOverriddenByPortal = true;
        portalGraceFrames = 2;
        StartCoroutine(InvincibilityCoroutine());
    }

    private IEnumerator InvincibilityCoroutine()
    {
        IsInvincible = true;
        yield return new WaitForSeconds(postTeleportInvincibility);
        IsInvincible = false;
    }

    void Start()
    {
        // Start is still fine for things that don't rely on physics components
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

        // --- Better Jump Physics ---
        bool wJumpHeld = Input.GetAxisRaw("Vertical") > 0.5f; 
        if (rb.linearVelocity.y < 0)
        {
            // Player is falling
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !(Input.GetButton("Jump") || wJumpHeld))
        {
            // Player is rising, but jump button is released
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
        }
        // ---
        
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
            if (portalGraceFrames > 0) { return; }
            
            bool playerIsTryingToMove = Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1f || Input.GetButtonDown("Jump");
            if (playerIsTryingToMove)
            {
                _controlsOverriddenByPortal = false;
            }
        }

        if (_controlsOverriddenByPortal) return;

        // Horizontal Movement
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

        // --- Jump Input Logic ---
        
        bool wJumpHeld = Input.GetAxisRaw("Vertical") > 0.5f;
        bool wJumpPressed = wJumpHeld && !wJumpPressedLastFrame;
        wJumpPressedLastFrame = wJumpHeld; 

        if ((Input.GetButtonDown("Jump") || wJumpPressed) && isGrounded)
        {
            // Set velocity directly for consistent height
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