using UnityEngine;

public class ControlsHUD : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public GameObject controlsHUD;

    void Start()
    {
        
    }

    // Update is called once per frame
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
