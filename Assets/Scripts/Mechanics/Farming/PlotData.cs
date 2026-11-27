using System;
using System.IO;
using UnityEngine;

public static class PlotData
{
    public enum CropTypes
    {
        None,
        Wheat
    }

    [Serializable]
    public struct Plot
    {
        public int ID;
        public bool IsPurchased;
        public CropTypes CurrentCrop;
        public float TimeLeft;
        public float Condition;
        public float Irrigation;
        public int PestCount;
        public int WeedCount;
    }

    public static void WritePlotData(BinaryWriter writer, Plot plot)
    {
        string json = JsonUtility.ToJson(plot, true);
        writer.Write(json);
    }

    public static Plot ReadPlotData(BinaryReader reader)
    {
        string json = reader.ReadString();
        return JsonUtility.FromJson<Plot>(json);
    }
}
