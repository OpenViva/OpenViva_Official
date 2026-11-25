using System.Collections.Generic;
using UnityEngine;

public class FarmPlot : MonoBehaviour
{
    [SerializeField][Min(0.01f)] private float _plotArea = 7.5f;
    [SerializeField][Min(0.01f)] private float _cropSpacing = 1.5f;
    [SerializeField] private string _name = "Small Plot";

    public string Name { private set; get; }
    public int CropAmount { private set; get; } = 0;
    public bool IsOccupied { private set; get; } = false;
    public float TimeLeft { private set; get; } = 0;
    public float Irrigation { private set; get; } = 0;
    public float Condition { private set; get; } = 100;

    private List<Vector3> _cropPositions;
    private PlotUIPanel _plotUIPanel;

    private void Awake()
    {
        Name = _name;
        GenerateGrid();
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

    public void AssignUIPanel(PlotUIPanel panel) { _plotUIPanel = panel; }

    #region Development
    private float _gizmoPlotArea;
    private float _gizmoCropSpacing;
    private const int MAX_GIZMO_DRAWS = 1024;
    private const int MAX_CROPS = 65536;

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
