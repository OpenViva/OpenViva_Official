using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static PlotData;

public class FarmPlot : MonoBehaviour
{
    public enum PlotStates
    {
        None,
        Unpurchased,
        Purchased,
        Growing,
        Waiting
    }
    public PlotStates CurrentState { private set; get; } = PlotStates.None;

    // Initialization
    [SerializeField][Min(0.01f)] private float _plotArea = 7.5f;
    [SerializeField][Min(0.01f)] private float _cropSpacing = 1.5f;
    [SerializeField] private string _name = "Small Plot";
    [SerializeField] private int _price = 100;

    public string Name { private set; get; }
    public int Price { private set; get; } 
    public int CropAmount { private set; get; } = 0;

    private List<Vector3> _cropPositions;
    private FarmingUIPanel _plotUIPanel;
    private Player _player;

    // Actions
    public Action OnStateChanged;
    public Action OnPurchaseFailed;
    public Action<bool> OnInRangeChanged;

    // Persistense
    [SerializeField] private int _plotID;
    public bool IsPurchased { private set; get; } = false;
    public CropTypes CurrentCrop { private set; get; } = CropTypes.None;
    public float TimeLeft { private set; get; } = 0;
    public float Condition { private set; get; } = 100;
    public float Irrigation { private set; get; } = 0;
    private int _pestCount = 0;
    private int _weedCount = 0;

    private string _directory;
    private string _file;
    private bool _isLoading = false;

    // Runtime
    private bool _inRange;

    // Development
    private const int MAX_GIZMO_DRAWS = 1024;
    private const int MAX_CROPS = 65536;
    private int _temp_playerFunds = 999999999/*0*/;

    private void Awake()
    {
        Name = _name;
        Price = _price;
        GenerateGrid();
    }

    private void OnEnable()
    {
        if (CurrentState == PlotStates.None && _player != null) DetermineState();
    }

    private void Start()
    {
        _player = FindFirstObjectByType<Player>();
        DetermineState();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _inRange = true;
            OnInRangeChanged?.Invoke(_inRange);
        }
    }

    private void GenerateGrid()
    {
        if (_plotArea <= 0 || _cropSpacing <= 0)
        {
            Debug.LogError("What are you trying to achieve?" +
                "\nSet Plot Area and Crop Spacing to positive values in the editor, then re-enter play mode." +
                "\nThis plot has been destroyed.");
            Destroy(this);
            return;
        }

        _cropPositions = new();
        CropAmount = 0;
        Vector3 current = new(-_cropSpacing, -_cropSpacing);

        while (current.x > -_plotArea)
        {
            while (current.y > -_plotArea)
            {
                if (CropAmount > MAX_CROPS)
                {
                    Debug.LogWarning("The amount of crops has exceeded a safe amount." +
                        "\nDecrease Plot Area or increase Crop Spacing." +
                        "\nAborting.");
                    Gizmos.color = Color.red;
                    return;
                }
                _cropPositions.Add(current);
                CropAmount++;
                current.y -= _cropSpacing;
            }
            current.y = -_cropSpacing;
            current.x -= _cropSpacing;
        }
    }

    private void DetermineState()
    {
        PlotStates state = PlotStates.None;
        if (!IsPurchased) state = PlotStates.Unpurchased;
        else if (IsPurchased) state = PlotStates.Purchased;

        if (state != CurrentState) SetCurrentState(state);
    }

    private void SetCurrentState(PlotStates state)
    {
        UnassignInputs();

        CurrentState = state;
        switch (CurrentState)
        {
            case PlotStates.Unpurchased:
                IsPurchased = false;
                CurrentCrop = 0;
                TimeLeft = 0;
                Condition = 100;
                Irrigation = 0;
                _pestCount = 0;
                _weedCount = 0;
                break;

            case PlotStates.Purchased:
                IsPurchased = true;
                CurrentCrop = 0;
                TimeLeft = 0;
                Condition = 100;
                Irrigation = 0;
                _pestCount = 0;
                _weedCount = 0;
                break;

            case PlotStates.None: break;
        }

        OnStateChanged?.Invoke();

        AssignInputs();
    }

    private void TryPurchase()
    {
        if (!_inRange) return;

        if (_temp_playerFunds < _price) OnPurchaseFailed?.Invoke();
        else SetCurrentState(PlotStates.Purchased);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _inRange = false;
            OnInRangeChanged?.Invoke(_inRange);
        }
    }

    private void OnDisable()
    {
        UnassignInputs();
        CurrentState = PlotStates.None;
    }

    #region Helpers
    public void AssignUIPanel(FarmingUIPanel panel) { _plotUIPanel = panel; }

    private void AssignInputs()
    {
        switch (CurrentState)
        {
            case PlotStates.Unpurchased:
                _player.Controls.Viva.UniversalInteractHold.performed += context => TryPurchase();
                break;

            case PlotStates.None: break;
        }
    }

    private void UnassignInputs()
    {
        switch (CurrentState)
        {
            case PlotStates.Unpurchased: 
                _player.Controls.Viva.UniversalInteractHold.performed -= context => TryPurchase(); 
                break;

            case PlotStates.None: break;
        }
    }
    #endregion

    #region Persistence
    private void GetDataDirectory()
    {
        if (Application.isEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
            string buildFolder = Directory.GetParent(Application.dataPath).FullName;
            _directory = Path.Combine(buildFolder, FarmManager.Instance.DIRECTORY_PATH);
        }
        else if (Application.platform == RuntimePlatform.Android)
        {
            _directory = Path.Combine(Application.persistentDataPath, FarmManager.Instance.DIRECTORY_PATH);
        }
    }

    public IEnumerator LoadPlotData()
    {
        // Fetch file
        if (_isLoading) yield break;

        _isLoading = true;

        if (_directory == null) GetDataDirectory();

        if (!Directory.Exists(_directory))
        {
            Debug.LogError("Directory not found: 'Persistense'");
            _isLoading = false;
            yield break;
        }

        if (_file == null) _file = Directory.GetFiles(_directory, FarmManager.Instance.FILE_NAME)[0];

        if (_file == null || !File.Exists(_file))
        {
            Debug.LogError("File not found: FarmingData");
            _isLoading = false;
            yield break;
        }

        // Read file
        using FileStream fs = new(_file, FileMode.Open, FileAccess.Read);
        using BinaryReader reader = new(fs);

        while (reader.BaseStream.Position < reader.BaseStream.Length)
        {
            Plot plot = PlotData.ReadPlotData(reader);

            if (plot.ID == _plotID)
            {
                IsPurchased = plot.IsPurchased;
                CurrentCrop = plot.CurrentCrop;
                TimeLeft = plot.TimeLeft;
                Condition = plot.Condition;
                Irrigation = plot.Irrigation;
                _pestCount = plot.PestCount;
                _weedCount = plot.WeedCount;
            }
        }

        _isLoading = false;
    }

    public IEnumerator SavePlotData()
    {
        if (_isLoading) yield break;

        _isLoading = true;

        Plot data = new()
        {
            ID = _plotID,
            IsPurchased = this.IsPurchased,
            CurrentCrop = this.CurrentCrop,
            TimeLeft = this.TimeLeft,
            Condition = this.Condition,
            Irrigation = this.Irrigation,
            PestCount = _pestCount,
            WeedCount = _weedCount,
        };

        if (_directory == null) GetDataDirectory();

        if (!Directory.Exists(_directory))
        {
            Debug.LogError("Directory not found: 'Persistense'");
            _isLoading = false;
            yield break;
        }

        if (_file == null) _file = Directory.GetFiles(_directory, FarmManager.Instance.FILE_NAME)[0];

        if (_file == null || !File.Exists(_file))
        {
            Debug.LogError("File not found: FarmingData");
            _isLoading = false;
            yield break;
        }

        // Write
        using FileStream fs = new(_file, FileMode.Append, FileAccess.Write);
        using BinaryWriter writer = new(fs);

        PlotData.WritePlotData(writer, data);

        _isLoading = false;
    }

    #endregion

    #region Development
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.purple;
        Gizmos.matrix = transform.localToWorldMatrix;

        if (_plotArea <= 0 || _cropSpacing <= 0)
        {
            Debug.LogError("Plot Area or Crop Spacing is <= 0." +
                "\nEnsure both are positive values." +
                "\n(In case you didn't know, you're probably in debug mode.");
            Gizmos.color = Color.red;
            Gizmos_DrawArea();
            return;
        }

        Gizmos_DrawArea();
        GenerateGrid();

        if (CropAmount > MAX_GIZMO_DRAWS)
        {
            Debug.LogWarning("You are attempting to draw too many gizmos in one frame." +
                "\nYou're going to crash your editor at this rate." +
                "\nDecrease Plot Size, increase Crop Spacing or disable gizmos.");
            Gizmos.color = Color.red;
            Gizmos_DrawArea();
            return;
        }

        Gizmos_DrawCrops();
    }

    private void Gizmos_DrawArea()
    {
        Vector3[] points =
        {
            new Vector3(0, 0),
            new Vector3(-_plotArea, 0),
            new Vector3(-_plotArea, -_plotArea),
            new Vector3(0, -_plotArea)
        };

        for (int i = 0; i < points.Length; i++)
            Gizmos.DrawLine(points[i], points[(i + 1) % points.Length]);
    }

    private void Gizmos_DrawCrops()
    {
        foreach (Vector3 position in _cropPositions)
            Gizmos.DrawLine(position + (Vector3.forward * -1), position + (Vector3.forward * 1));
    }
    #endregion
}
