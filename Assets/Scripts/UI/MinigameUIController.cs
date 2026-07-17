using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MinigameUIController
{
    public class CuttingMinigame : MonoBehaviour
    {
        public static CuttingMinigame Instance {  get; private set; }

        // Cutting Minigame
        [SerializeField] private GameObject _cuttingUIParent;
        [SerializeField] private GameObject _backgroundImage;
        [SerializeField] private GameObject _movingPoint;
        [SerializeField] private TextMeshProUGUI _accuracyText;
        [SerializeField] private TextMeshProUGUI _averageText;
        [SerializeField] private TextMeshProUGUI _doneText;
        [SerializeField] private Scrollbar _progressBar;
        private GameObject _progressBarHandle;

        private void Awake()
        {
            if (Instance == null) { Instance = this; }
            else { Destroy(this); }

            _progressBarHandle = _progressBar.handleRect.gameObject;
        }

        public void CuttingMinigameUIEnabled(bool enable) { _cuttingUIParent.SetActive(enable); }

        public void SetBackgroundPosition(float position, float range)
        {
            Vector3 currentPosition = _backgroundImage.transform.localPosition;
            _backgroundImage.transform.localPosition = new Vector3(-150f + (position * 300 / range), currentPosition.y, currentPosition.z);
        }

        public void SetMovingPointPosition(float position, float range)
        {
            Vector3 currentPosition = _movingPoint.transform.localPosition;
            _movingPoint.transform.localPosition = new Vector3(-375f + (position * 300 / range), currentPosition.y, currentPosition.z);
        }

        public void MovingPointEnabled(bool enable) { _movingPoint.SetActive(enable); }

        public void SetAccuracy(float cutAccuracy, float average)
        {
            _accuracyText.SetText($"{cutAccuracy:0}%");
            _averageText.SetText($"{average:0}%");

            switch (average)
            {
                case float n when n < 85: _averageText.color = Color.red; break;
                case float n when n >= 85 && n < 95: _averageText.color = Color.white; break;
                case float n when n >= 95: _averageText.color = Color.green; break;
            }
        }
    }
}
