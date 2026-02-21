#if UNITY_EDITOR || UNITY_STANDALONE_WIN

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple crosshair that changes color/size when looking at an interactable object.
/// Attach to a UI Image centered on screen.
/// </summary>
[RequireComponent(typeof(Image))]
public class DesktopCrosshair : MonoBehaviour
{
    [Header("Settings")]
    public Color normalColor = new Color(1f, 1f, 1f, 0.5f);
    public Color interactColor = new Color(1f, 0.9f, 0.3f, 0.9f);
    public Color grabColor = new Color(0.3f, 1f, 0.5f, 0.9f);
    public float normalSize = 4f;
    public float activeSize = 6f;
    public float transitionSpeed = 10f;

    [Header("References")]
    public PlayerKB_GrabController grabController;

    Image image;
    RectTransform rectTransform;
    Color targetColor;
    float targetSize;

    void Start()
    {
        image = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        targetColor = normalColor;
        targetSize = normalSize;
    }

    void Update()
    {
        if (grabController != null)
        {
            var item = grabController.GetLookedAtItem();
            if (item != null)
            {
                targetColor = grabColor;
                targetSize = activeSize;
            }
            else
            {
                targetColor = normalColor;
                targetSize = normalSize;
            }
        }

        image.color = Color.Lerp(image.color, targetColor, transitionSpeed * Time.deltaTime);
        float currentSize = rectTransform.sizeDelta.x;
        float newSize = Mathf.Lerp(currentSize, targetSize, transitionSpeed * Time.deltaTime);
        rectTransform.sizeDelta = new Vector2(newSize, newSize);
    }
}

#endif
