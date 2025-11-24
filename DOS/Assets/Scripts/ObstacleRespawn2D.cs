using UnityEngine;

[DisallowMultipleComponent]
public class ObstacleRespawn2D : MonoBehaviour
{
    public KeyCode respawnKey = KeyCode.O;
    public float maxSpeed = 17f;
    public float fallMultiplier = 2.5f;
    public GameObject resetPopup;
    public float popupOffsetY = 1f;
    public float popupDisplayTime = 2.5f;  
    
    private Vector3 startPos;
    private Quaternion startRot;
    private Vector3 startScale;
    private Rigidbody2D rb;
    private bool isTouchingWall = false;
    private float popupTimer = 0f;  
    private bool popupShown = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startPos   = transform.position;
        startRot   = transform.rotation;
        startScale = transform.localScale;
        
        if (resetPopup != null)
        {
            resetPopup.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(respawnKey))
            Respawn();
        
        // NEW: Update popup position and timer
        if (resetPopup != null && resetPopup.activeSelf)
        {
            Vector3 popupPos = transform.position;
            popupPos.y += popupOffsetY;
            resetPopup.transform.position = popupPos;
            
            // Count down the timer
            popupTimer -= Time.deltaTime;
            if (popupTimer <= 0f)
            {
                resetPopup.SetActive(false);
            }
        }
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

    void OnCollisionEnter2D(Collision2D collision)
    {
        CheckWallCollision(collision);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        CheckWallCollision(collision);
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (IsWall(collision))
        {
            isTouchingWall = false;
            popupShown = false;  
            if (resetPopup != null)
            {
                resetPopup.SetActive(false);
            }
        }
    }

    void CheckWallCollision(Collision2D collision)
    {
        if (!IsWall(collision))
            return;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (Mathf.Abs(contact.normal.x) > 0.5f)
            {
                if (!isTouchingWall || !popupShown)
                {
                    isTouchingWall = true;
                    popupShown = true;
                    if (resetPopup != null)
                    {
                        resetPopup.SetActive(true);
                        popupTimer = popupDisplayTime;  
                    }
                }
                return;
            }
        }
    }

    bool IsWall(Collision2D collision)
    {
        string tag = collision.gameObject.tag;
        string layerName = LayerMask.LayerToName(collision.gameObject.layer);
        
        if (tag == "PortalGround" && layerName == "VerticalWall")
            return true;
        
        if (tag == "Side Walls")
            return true;
        
        if (tag == "Ground" && layerName == "Ground")
            return true;
        
        return false;
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
        
        if (resetPopup != null)
        {
            resetPopup.SetActive(false);
            isTouchingWall = false;
            popupShown = false; 
        }
    }
}