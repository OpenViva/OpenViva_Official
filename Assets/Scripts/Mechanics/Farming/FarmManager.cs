using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class FarmManager : MonoBehaviour
{
    public static FarmManager Instance { private set; get; }

    [Header("Inventory")]
    public int CropsPerSeed { private set; get; } = 16;

    public int WheatSeeds = 0;

    public int WheatCrops = 0;

    [Header("UI")]
    [SerializeField] private GameObject _scrollviewContent;
    [SerializeField] private GameObject _plotPanelPrefab;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }

    private void Start()
    {
        InitFarmCanvas();
    }

    private void InitFarmCanvas()
    {
        List<FarmPlot> farmPlots = FindObjectsByType<FarmPlot>(FindObjectsSortMode.None).ToList();

        int panelsCreated = 0;
        foreach (FarmPlot plot in farmPlots)
        {
            GameObject newPanel = Instantiate(_plotPanelPrefab, _scrollviewContent.transform);
            PlotUIPanel panelScript = newPanel.GetComponent<PlotUIPanel>();

            panelsCreated++;
            int cropsPerSeed = plot.CropAmount / CropsPerSeed;
            panelScript.InitializePanel(panelsCreated, plot.Name, plot.CropAmount);
            plot.AssignUIPanel(panelScript);
        }
    }
}
