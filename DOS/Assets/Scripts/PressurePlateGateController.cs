using UnityEngine;

public class PressurePlateGateController : MonoBehaviour
{
    [Header("Gate Settings")]
    public SpriteRenderer gateRenderer;
    public Sprite gateClosedSprite;
    public Sprite gateOpenSprite;

    [Header("Pressure Plate Settings")]
    public SpriteRenderer plateRenderer;
    public Sprite plateUpSprite;
    public Sprite plateDownSprite;

    private bool isPressed = false;
    private int objectsOnPlate = 0; // like objectsInside from the previous script

    // Same logic as CanTriggerGate in GateTrigger
    private bool CanTriggerPlate(Collider2D other)
    {
        return other.CompareTag("Player") ||
               other.CompareTag("MovableObstacle") ||
               other.CompareTag("Enemy");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!CanTriggerPlate(collision))
            return;

        objectsOnPlate++;

        // Only press when this is the FIRST valid object
        if (!isPressed && objectsOnPlate > 0)
        {
            PressPlate();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!CanTriggerPlate(collision))
            return;

        objectsOnPlate = Mathf.Max(0, objectsOnPlate - 1);

        // Only release when there are NO valid objects left
        if (isPressed && objectsOnPlate == 0)
        {
            ReleasePlate();
        }
    }

    private void PressPlate()
    {
        isPressed = true;

        // Change to "pressed" plate sprite
        if (plateRenderer != null && plateDownSprite != null)
            plateRenderer.sprite = plateDownSprite;

        // Change gate sprite to open
        if (gateRenderer != null && gateOpenSprite != null)
            gateRenderer.sprite = gateOpenSprite;
    }

    private void ReleasePlate()
    {
        isPressed = false;

        // Change back to "unpressed" plate sprite
        if (plateRenderer != null && plateUpSprite != null)
            plateRenderer.sprite = plateUpSprite;

        // Change gate sprite to closed
        if (gateRenderer != null && gateClosedSprite != null)
            gateRenderer.sprite = gateClosedSprite;
    }
}
