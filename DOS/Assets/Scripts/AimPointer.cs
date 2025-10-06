using UnityEngine;

public class AimPointer : MonoBehaviour
{
    public Camera mainCamera;

    void Start()
    {
        Cursor.visible = false;
    }

    void Update()
    {
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        transform.position = mousePos;
    }
}
