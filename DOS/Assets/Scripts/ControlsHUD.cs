using UnityEngine;

public class ControlsHUD : MonoBehaviour
{

    public GameObject controlsHUD;

    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            bool isActive = controlsHUD.activeSelf;
            controlsHUD.SetActive(!isActive);
        }
    }

    public void HideHUD()
    {
        controlsHUD.SetActive(false);
    }
    
    public void ToggleHUD()
    {
        bool isActive = controlsHUD.activeSelf;
        controlsHUD.SetActive(!isActive);
    }
}
