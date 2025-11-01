using UnityEngine;

public class GunRotation : MonoBehaviour
{
    public Camera mainCamera;
    public PlayerController player;

    void Start()
    {
        // Find player
        if (!player)
            player = GetComponentInParent<PlayerController>();
    }


    void Update()
    {
        if (!player) return;


        bool facingRight = player.IsFacingRight();
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        Vector3 direction = mousePos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Clamp the gun on the Semi-Circle in the facing direction (Degrees)
        // Facing Right: 270->0->90
        // Facing Left: 90->180->270

        // Uncomment below code for Clipped AIM.
        // if (facingRight)
        // {
        //     angle = Mathf.Clamp(angle, -90f, 90f);
        // }
        // else
        // {
        //     if (angle < 0) angle += 360f;
        //     if (angle < 90f) angle = 90f;
        //     if (angle > 270f) angle = 270f;
        // }

        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}
