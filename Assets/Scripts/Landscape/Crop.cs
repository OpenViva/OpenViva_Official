using System;
using UnityEngine;

public class Crop : PlayerKB_GrabObject
{
    public event Action OnIsGrabbed;

    [SerializeField] private CropData _data;

    private float _growTimer; // Should this depend on Day/Night cycle speed?
    private float _maxScale;
    private Color _phase1Color;
    private Color _phase2Color;
    private Color _finalColor;

    private float _timer;
    private bool _isGrowing = true;
    private Color _currentColor;

    [SerializeField] public bool ShouldGrow = true;

    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    protected override void Start()
    {
        base.Start();

        if (!ShouldGrow)
        {
            _isGrowing = false;
            _timer = 0;
            CheckPhase();
            return;
        }

        _growTimer = _data.GrowTimer;
        _maxScale = _data.MaxScale;
        _phase1Color = _data.Phase1Color;
        _phase2Color = _data.Phase2Color;
        _finalColor = _data.FinalColor;

        _timer = _growTimer;
        _currentColor = _finalColor;
    }

    private void Update()
    {
        if (_timer > 0)
        {
            _timer -= Time.deltaTime;
        }
        else
        {
            _isGrowing = false;
        }

        if (_isGrowing)
        {
            float value = (1 - _timer / _growTimer) * _maxScale;
            Vector3 scale = new(value, value, value);
            transform.localScale = scale;
            CheckPhase();
        }
    }

    private void CheckPhase()
    {
        if (TryGetComponent(out Renderer r))
        {
            float growthPercent = 1 - _timer / _growTimer;
            switch (growthPercent)
            {
                case float n when (n >= 0 && n < 0.5):
                    if (_currentColor == _phase1Color) { return; }
                    r.material.color = _phase1Color;
                    _currentColor = _phase1Color;
                    break;

                case float n when (n >= 0.5f && n < 0.99):
                    if (_currentColor == _phase2Color) { return; }
                    r.material.color = _phase2Color;
                    _currentColor = _phase2Color;
                    break;

                default:
                    if (_currentColor == _finalColor) { return; }
                    r.material.color = _finalColor;
                    _currentColor = _finalColor;
                    break;
            }
        }
    }

    protected override void OnGrabbed()
    {
        base.OnGrabbed();
        OnIsGrabbed?.Invoke();
        if (_audioSource != null) { _audioSource.Play(); }
        else { Debug.Log($"An audio source component has not been given to the '{gameObject.name}' prefab"); }
    }

    protected override void GrabLeft()
    {
        if (!_isGrowing)
        {
            base.GrabLeft();
        }
    }

    protected override void GrabRight()
    {
        if (!_isGrowing)
        {
            base.GrabRight();
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (!_isGrowing)
        {
            TriggerEntered(collider, HintConstants.GrabHint);
        }
        else
        {
            TriggerEntered(collider, HintConstants.CropGrowingHint);
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (!_isGrowing)
        {
            TriggerExited(collider, HintConstants.GrabHint);
        }
        else
        {
            TriggerExited(collider, HintConstants.CropGrowingHint);
        }
    }
}
