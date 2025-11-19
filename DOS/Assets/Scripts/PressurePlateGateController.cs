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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isPressed)
        {
            PressPlate();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (isPressed)
        {
            ReleasePlate();
        }
    }

    private void PressPlate()
    {
        isPressed = true;

        // Change to "pressed" plate sprite
        plateRenderer.sprite = plateDownSprite;

        // Change gate sprite to open
        gateRenderer.sprite = gateOpenSprite;
    }

    private void ReleasePlate()
    {
        isPressed = false;

        // Change back to "unpressed" plate sprite
        plateRenderer.sprite = plateUpSprite;

        // Change gate sprite to closed
        gateRenderer.sprite = gateClosedSprite;
    }
}
