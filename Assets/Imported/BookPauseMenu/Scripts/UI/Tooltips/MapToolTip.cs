using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Viva
{

    public class MapToolTip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        private TextMeshProUGUI tooltip;
        [Tooltip("Specify Enter Fade time.")]
        public float fadetime = 0f;
        [Tooltip("Specify Exit Fade time.")]
        public float Unfadetime = 0f;

        public void OnPointerEnter(PointerEventData eventData)
        {            
            StartCoroutine(FadeText(fadetime, tooltip));
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            StartCoroutine(UnFadeText(Unfadetime, tooltip));          
        }
        public IEnumerator FadeText(float time, TextMeshProUGUI text)
        {
            text.color = new Color(text.color.r, text.color.g, text.color.b, 0);
            while (text.color.a < 1.0f)
            {
                text.color = new Color(text.color.r, text.color.g, text.color.b, text.color.a + (Time.deltaTime / time));
                yield return null;
            }
        }

        public IEnumerator UnFadeText(float time, TextMeshProUGUI text)
        {
            text.color = new Color(text.color.r, text.color.g, text.color.b, 1);
            while (text.color.a > 0.0f)
            {
                text.color = new Color(text.color.r, text.color.g, text.color.b, text.color.a - (Time.deltaTime / time));
                yield return null;
            }
        }
    }
}