using System.Collections;
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
        [SerializeField] private GameObject _controlsDuring;
        [SerializeField] private GameObject _controlsAfter;
        [SerializeField] private TextMeshProUGUI _containerFull;

        private float _transition = 1f;
        private bool _fading = false;

        private void Awake()
        {
            if (Instance == null) { Instance = this; }
            else { Destroy(this); }
        }

        private void Update()
        {
            if (_fading && _transition > 0)
            {
                _transition -= Time.deltaTime * 0.5f;
                _containerFull.alpha = _transition;
            }
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

        public void SetProgressBarSize(float ratio) {  _progressBar.size = ratio; }

        public void SetAccuracy(float cutAccuracy, float average)
        {
            _accuracyText.SetText($"{cutAccuracy:0}%");
            _averageText.SetText($"{average:0}%");

            switch (average)
            {
                case float n when n < 85: _averageText.color = Color.red; break;
                case float n when n >= 85 && n < 95: _averageText.color = Color.white; break;
                case float n when n >= 95 && n <= 100: _averageText.color = Color.green; break;

                case float n when n > 100:
                    _accuracyText.SetText("--%");
                    _averageText.SetText("--%");
                    _averageText.color = Color.white; 
                    break;
            }
        }

        public void SetControlHints(bool isComplete)
        {
            if (isComplete)
            {
                _controlsDuring.SetActive(false);
                _controlsAfter.SetActive(true);
            }
            else
            {
                _controlsDuring.SetActive(true);
                _controlsAfter.SetActive(false);
            }
        }

        public void DoneTextEnabled(bool enable, float accuracy)
        {
            if (!enable) { _doneText.enabled = false; }
            else
            {
                switch (accuracy)
                {
                    case float n when n < 80:
                        _doneText.SetText("Poor. Price -10%");
                        _doneText.color = Color.red;
                        break;

                    case float n when n >= 80 && n < 95:
                        _doneText.SetText("Decent.");
                        _doneText.color = Color.white;
                        break;

                    case float n when n >= 95 && n < 99:
                        _doneText.SetText("Excellent! Price +10%");
                        _doneText.color = Color.green;
                        break;

                    case float n when n >= 99:
                        _doneText.SetText("PERFECT! Price +15%");
                        _doneText.color = Color.green;
                        break;
                }

                _doneText.enabled = true;
            }
        }

        public IEnumerator ShowContainerFullText()
        {
            _fading = true;
            yield return new WaitForSeconds(2f);
            _fading = false;
            _transition = 1f;
        }
    }
}
