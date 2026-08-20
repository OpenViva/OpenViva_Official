using UnityEngine;

public class SplashTextAnimator : MonoBehaviour
{
    [Header("LeanTween Settings")]
    [Tooltip("How much bigger the text gets at the peak of the bounce")]
    public float scaleMultiplier = 1.1f;
    [Tooltip("Duration of scling (seconds)")]
    public float pulseDuration = 0.5f;

    private void Start()
    {
        // Calculate the maximum scale size
        Vector3 targetScale = transform.localScale * scaleMultiplier;

        // Scale to the target, ease it smoothly, and loop back and forth forever
        LeanTween.scale(gameObject, targetScale, pulseDuration)
            .setEaseInOutSine()
            .setLoopPingPong()
            .setIgnoreTimeScale(true);
    }
}
