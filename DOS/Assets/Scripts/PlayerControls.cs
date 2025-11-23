using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float maxFallSpeed = 17f;
    public float speed = 5f;
    private Coroutine activeInvincibilityCoroutine = null;

    [Header("Jump Physics")]
    public float jumpForce = 5f; 
    public float fallMultiplier = 2.5f; 
    public float lowJumpMultiplier = 2f; 
    public float postTeleportInvincibility = 0.1f;

    [Header("Portal Physics")]
    public float portalMomentumDampening = 50f;

    [Header("Audio")]
    [SerializeField] private AudioSource sfxSource;       // Drag Main AudioSource here (for Jump/Land)
    [SerializeField] private AudioSource footstepsSource; // Drag 2nd AudioSource here (for Walking)
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private AudioClip landClip;
    [SerializeField] private AudioClip walkClip;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private bool isGrounded = false;
    private bool wasGrounded = false; // To track landing
    private bool _controlsOverriddenByPortal = false;
    private int portalGraceFrames; 
    private bool facingRight = true;
    private Transform portalGunTransform;

    public GameObject controlsHUD;
    
    private bool wJumpPressedLastFrame = false; 

    public bool IsInvincible { get; private set; } = false;

    private Animator animator;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        animator = GetComponent<Animator>();

        // Safety check for AudioSources
        if (footstepsSource != null)
        {
            footstepsSource.clip = walkClip;
            footstepsSource.loop = true;
        }
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
        // Logic to detect Landing (Must happen BEFORE updating isGrounded)
        bool currentlyGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // If we weren't grounded last frame, but we are now -> We Landed
        if (!wasGrounded && currentlyGrounded)
        {
            if (sfxSource != null && landClip != null)
            {
                sfxSource.PlayOneShot(landClip);
            }
        }

        isGrounded = currentlyGrounded;
        wasGrounded = isGrounded; // Update for next frame

        animator.SetBool("isGrounded", isGrounded);

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
        animator.SetFloat("speed", Mathf.Abs(horizontalInput));

        // --- AUDIO: WALKING ---
        if (footstepsSource != null)
        {
            // Play if: Grounded AND moving AND not currently playing
            if (isGrounded && Mathf.Abs(horizontalInput) > 0.1f && !_controlsOverriddenByPortal)
            {
                if (!footstepsSource.isPlaying)
                {
                    footstepsSource.Play();
                }
            }
            else
            {
                // Stop if: In air OR stopped moving
                if (footstepsSource.isPlaying)
                {
                    footstepsSource.Stop();
                }
            }
        }

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
                    if (Mathf.Sign(horizontalInput) != Mathf.Sign(rb.linearVelocity.x))
                    {
                        float targetSpeed = horizontalInput * speed;
                        float newVelocityX = Mathf.MoveTowards(
                            rb.linearVelocity.x, 
                            targetSpeed, 
                            portalMomentumDampening * Time.deltaTime
                        );
                        rb.linearVelocity = new Vector2(newVelocityX, rb.linearVelocity.y);
                    }
                }
                
                if (horizontalInput > 0 && !facingRight) Flip();
                else if (horizontalInput < 0 && facingRight) Flip();
                
                return;
            }
        }
        
        rb.linearVelocity = new Vector2(horizontalInput * speed, rb.linearVelocity.y);

        if (horizontalInput > 0 && !facingRight) Flip();
        else if (horizontalInput < 0 && facingRight) Flip();

        // Jump Input Logic 
        bool wJumpHeld = Input.GetAxisRaw("Vertical") > 0.5f;
        bool wJumpPressed = wJumpHeld && !wJumpPressedLastFrame;
        wJumpPressedLastFrame = wJumpHeld; 

        if ((Input.GetButtonDown("Jump") || wJumpPressed) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            
            // Play Jump Sound
            if (sfxSource != null && jumpClip != null)
            {
                sfxSource.PlayOneShot(jumpClip);
            }
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