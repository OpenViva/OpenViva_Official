using TMPro;
using UnityEngine;

public class MarkerController : MonoBehaviour
{
    [TextArea(2, 5)]
    public string text = "Marker Editor";
    [Min(0.1f)]
    public float fontSize = 2f;

    [Header("Target")]
    [Tooltip("Leave empty to automatically find the first TextMeshPro component in children")]
    public TMP_Text targetTMP;

    private void OnValidate()
    {
        Apply();
    }

    private void OnEnable()
    {
        Apply();
    }

    public void Apply()
    {
        if (targetTMP == null)
        {
            targetTMP = GetComponentInChildren<TMP_Text>(true);
        }

        if (targetTMP == null)
        {
            Debug.LogWarning($"[{name}] No TextMeshPro component found in children.", this);
            return;
        }

        if (!string.IsNullOrEmpty(text))
        {
            targetTMP.text = text;
        }

        targetTMP.fontSize = fontSize;
    }
}
