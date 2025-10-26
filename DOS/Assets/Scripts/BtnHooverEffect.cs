using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BtnHooverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Image buttonImage;
    private Color originalColor;
    private Vector3 originalScale;
    
    [SerializeField] private float hoverColorMultiplier = 0.85f; 
    [SerializeField] private float hoverScale = 1.05f; 
    
    void Start()
    {
        buttonImage = GetComponent<Image>();
        originalColor = buttonImage.color;
        originalScale = transform.localScale;
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        buttonImage.color = originalColor * hoverColorMultiplier;
        transform.localScale = originalScale * hoverScale;
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        buttonImage.color = originalColor;
        transform.localScale = originalScale;
    }
}