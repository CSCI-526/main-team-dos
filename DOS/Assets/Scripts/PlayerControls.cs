using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float maxFallSpeed = 17f;
    public float speed = 5f;
    private Coroutine activeInvincibilityCoroutine = null;

    [Header("Jump Physics")]
    [Tooltip("The initial velocity applied when jumping.")]
    public float jumpForce = 5f; 
    
    [Tooltip("Multiplier for gravity when falling.")]
    public float fallMultiplier = 2.5f; 
    [Tooltip("Multiplier for gravity when jump is released early.")]
    public float lowJumpMultiplier = 2f; 
    
    public float postTeleportInvincibility = 0.2f;

    [Header("Portal Physics")]
    [Tooltip("How quickly the player can 'fight' or 'dampen' portal momentum.")]
    public float portalMomentumDampening = 50f;

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
    
    private bool wJumpPressedLastFrame = false; 

    public bool IsInvincible { get; private set; } = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
    }

    public void OnTeleport()
    {
        _controlsOverriddenByPortal = true;
        portalGraceFrames = 2; 
        
        if (activeInvincibilityCoroutine != null)
        {
            StopCoroutine(activeInvincibilityCoroutine);
        }
        
        activeInvincibilityCoroutine = StartCoroutine(InvincibilityCoroutine());
    }

    private IEnumerator InvincibilityCoroutine()
    {
        IsInvincible = true;
        yield return new WaitForSeconds(postTeleportInvincibility);
        IsInvincible = false;
        activeInvincibilityCoroutine = null;
    }

    void Start()
    {
        portalGunTransform = GetComponentInChildren<PortalGun>()?.transform;
    }

    void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

    
        if (portalGraceFrames > 0)
        {
            portalGraceFrames--;
        }
        else if (_controlsOverriddenByPortal && isGrounded && Mathf.Abs(rb.linearVelocity.x) > speed)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            _controlsOverriddenByPortal = false;
        }
        

        bool wJumpHeld = Input.GetAxisRaw("Vertical") > 0.5f; 
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !(Input.GetButton("Jump") || wJumpHeld))
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
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

        float horizontalInput = Input.GetAxisRaw("Horizontal");

        // --- MOMENTUM LOGIC ---
        if (_controlsOverriddenByPortal)
        {
            if (Mathf.Abs(rb.linearVelocity.x) <= speed)
            {
                _controlsOverriddenByPortal = false;
            }
            else
            {
                
                if (Mathf.Abs(horizontalInput) > 0.1f)
                {
                    // Check if player is pressing *against* their momentum
                    if (Mathf.Sign(horizontalInput) != Mathf.Sign(rb.linearVelocity.x))
                    {
                        // Apply dampening
                        float targetSpeed = horizontalInput * speed;
                        float newVelocityX = Mathf.MoveTowards(
                            rb.linearVelocity.x, 
                            targetSpeed, 
                            portalMomentumDampening * Time.deltaTime
                        );
                        rb.linearVelocity = new Vector2(newVelocityX, rb.linearVelocity.y);
                    }
                }
                
                // --- Flipping while in momentum state ---
                if (horizontalInput > 0 && !facingRight) Flip();
                else if (horizontalInput < 0 && facingRight) Flip();
                
                return;
            }
        }
        
        rb.linearVelocity = new Vector2(horizontalInput * speed, rb.linearVelocity.y);

        if (horizontalInput > 0 && !facingRight)
        {
            Flip();
        }
        else if (horizontalInput < 0 && facingRight)
        {
            Flip();
        }

        // Jump Input Logic 
        
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