using System.Collections.Generic;
using UnityEngine;

namespace Viva.Interaction
{
    public class TargetOutlineManager : MonoBehaviour
    {
        [SerializeField] private SmartTargetDetector detector;
        [SerializeField] private Color outlineColor = Color.white;
        [SerializeField] private float outlineWidth = 5f;

        private readonly Dictionary<GameObject, Outline> outlineCache = new Dictionary<GameObject, Outline>();
        private GameObject currentOutlinedObject;

        private void OnEnable()
        {
            if (detector != null)
            {
                detector.OnTargetChanged += HandleTargetChanged;
            }
        }

        private void OnDisable()
        {
            if (detector != null) detector.OnTargetChanged -= HandleTargetChanged;
            ClearOutline();
        }

        private void HandleTargetChanged(GameObject newTarget)
        {
            if (currentOutlinedObject == newTarget)
            {
                return;
            }

            ClearOutline();

            if (newTarget != null)
            {
                ApplyOutline(newTarget);
            }
        }

        private void ApplyOutline(GameObject target)
        {
            if (!outlineCache.TryGetValue(target, out Outline outline) || outline == null)
            {
                if (!target.TryGetComponent(out outline))
                {
                    outline = target.AddComponent<Outline>();
                }
                outlineCache[target] = outline;
            }

            outline.OutlineColor = outlineColor;
            outline.OutlineWidth = outlineWidth;
            outline.enabled = true;
            currentOutlinedObject = target;
        }

        private void ClearOutline()
        {
            if (currentOutlinedObject != null)
            {
                if (outlineCache.TryGetValue(currentOutlinedObject, out Outline outline) && outline != null)
                {
                    outline.enabled = false;
                }
                currentOutlinedObject = null;
            }
        }
    }
}