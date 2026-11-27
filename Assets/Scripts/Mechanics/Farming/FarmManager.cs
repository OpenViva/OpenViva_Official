using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class FarmManager : MonoBehaviour
{
    public static FarmManager Instance { private set; get; }

    public readonly string DIRECTORY_PATH = "Assets/Personal/Persistence";
    public readonly string FILE_NAME = "Temp_FarmingData.txt";

    public int CropsPerSeed { private set; get; } = 16;

    [Header("Inventory")]
    public int WheatSeeds = 0 ;
    public int WheatCrops = 0;

    [Header("UI")]
    [SerializeField] private GameObject _scrollviewContent;
    [SerializeField] private GameObject _plotPanelPrefab;

    private List<FarmPlot> _farmPlots;

    [Header("Development")]
    [SerializeField] private InputAction _temp_saveAction;


    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }

    private void OnEnable()
    {
        _temp_saveAction.Enable();
    }

    private void Start()
    {
        _farmPlots = FindObjectsByType<FarmPlot>(FindObjectsSortMode.None).ToList();

        LoadFarmingData();
        InitFarmCanvas();
    }

    private void Update()
    {
        if (_temp_saveAction.WasPressedThisFrame())
        {
            SaveFarmingData();
        }
    }

    public void SaveFarmingData()
    {
        string path = Path.Combine(DIRECTORY_PATH, FILE_NAME);
        File.WriteAllText(path, string.Empty);

        foreach (FarmPlot plot in _farmPlots)
        {
            StartCoroutine(plot.SavePlotData());
        }

        Debug.Log("Farming data saved");
    }

    private void LoadFarmingData()
    {
        foreach (FarmPlot plot in _farmPlots)
        {
            StartCoroutine(plot.LoadPlotData());
        }

        Debug.Log("Farming data loaded");
    }

    private void InitFarmCanvas()
    {
        int panelsCreated = 0;
        foreach (FarmPlot plot in _farmPlots)
        {
            GameObject newPanel = Instantiate(_plotPanelPrefab, _scrollviewContent.transform);
            FarmingUIPanel panelScript = newPanel.GetComponent<FarmingUIPanel>();

            panelsCreated++;
            int cropsPerSeed = plot.CropAmount / CropsPerSeed;
            panelScript.InitializePanel(panelsCreated, plot.Name, plot.CropAmount);
            plot.AssignUIPanel(panelScript);
        }
    }

    private void OnDisable()
    {
        _temp_saveAction.Disable();
    }
}
