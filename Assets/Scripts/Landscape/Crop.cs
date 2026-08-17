using System;
using UnityEngine;

public class Crop : PlayerKB_GrabObject
{
    public event Action OnIsGrabbed;

    [SerializeField] private CropData _data;
    [SerializeField] public bool ShouldGrow = true;

    private float _growTimer; // Should this depend on Day/Night cycle speed?
    private float _maxScale;
    private Color _phase1Color;
    private Color _phase2Color;
    private Color _finalColor;

    private float _timer;
    private bool _isGrowing = true;
    private int _currentPhase = -1; // -1 = none, 0 = phase1, 1 = phase2, 2 = final

    private AudioSource _audioSource;
    private Renderer _renderer;
    private MaterialPropertyBlock _materialPropertyBlock;

    private void Awake()
    {
        _audioSource = GetComponentInChildren<AudioSource>();
        _renderer = GetComponent<Renderer>();
        _materialPropertyBlock = new MaterialPropertyBlock();
    }

    protected override void Start()
    {
        base.Start();

        if (!ShouldGrow)
        {
            _isGrowing = false;
            _timer = 0;
            SetPhase(2); // Final phase
            enabled = false; // Stop Update completely
            return;
        }

        _growTimer = _data.GrowTimer;
        _maxScale = _data.MaxScale;
        _phase1Color = _data.Phase1Color;
        _phase2Color = _data.Phase2Color;
        _finalColor = _data.FinalColor;

        _timer = _growTimer;
        SetPhase(0);
    }

    private void Update()
    {
        if (!_isGrowing)
        {
            return;
        }

        _timer -= Time.deltaTime;

        if (_timer <= 0)
        {
            _timer = 0;
            _isGrowing = false;
            SetPhase(2);
            enabled = false;
            return;
        }

        float t = 1f - (_timer / _growTimer);
        float scale = t * _maxScale;
        transform.localScale = new Vector3(scale, scale, scale);

        if (t < 0.5f)
        {
            SetPhase(0);
        }
        else if(t < 0.99f)
        {
            SetPhase(1);
        }
        else
        {
            SetPhase(2);
        }
    }

    private void SetPhase(int phase)
    {
        if (phase == _currentPhase || _renderer == null)
        {
            return;
        }

        _currentPhase = phase;

        Color color = phase switch
        {
            0 => _phase1Color,
            1 => _phase2Color,
            _ => _finalColor,
        };

        _renderer.GetPropertyBlock(_materialPropertyBlock);
        _materialPropertyBlock.SetColor("_Color", color);
        _renderer.SetPropertyBlock(_materialPropertyBlock);
    }

    protected override void OnGrabbed()
    {
        base.OnGrabbed();
        OnIsGrabbed?.Invoke();
        if (_audioSource != null && ShouldGrow)
        {
            _audioSource.Play();
            ShouldGrow = false;
        }
        else if (_audioSource == null)
        { 
            Debug.Log($"An audio source component has not been given to the '{gameObject.name}' prefab");
        }
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
