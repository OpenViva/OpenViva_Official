
#if (UNITY_EDITOR)

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;


namespace nTools.PrefabPainter
{

    public enum OrientationMode
    {
        SurfaceNormal,
        X,
        Y,
        Z,
        SurfaceNormalNegative,
        XNegative,
        YNegative,
        ZNegative,
    }

    public enum TransformMode
    {
        Relative,
        Absolute,
    }

    public enum Placement
    {
        World,
        HitObject,
        CustomObject,
    }

    public enum AxisMode
    {
        Uniform,
        PerAxis,
    }

    public enum SurfaceCoords
    {
        AroundX,
        AroundY,
        AroundZ,
    }

    public enum ColorTag
    {
        None,
        Red,
        Orange,
        Yellow,
        Green,
        Blue,
        Violet
    }

    public enum SlopeVector
    {
        X,
        Y,
        Z,
        FirstNormal,
        View,
        Custom
    }

    public enum SelectMode
    {
        Replace,
        Add,
        Substract
    }

    public enum GridPlane
    {
        XY,
        XZ,
        YZ,
        Custom
    }

    public enum MultibrushMode
    {
        Random,
        Pattern
    }

    public enum OverlapCheckMode
    {
        None,
        Distance,
        Bounds
    }

    public enum OverlapCheckObjects
    {
        SameObjects,
        SamePlaceLayer,
        OtherLayers
    }

    public enum PivotMode
    {
        ObjectPivot,
        BoundsCenter,
        BoundsTopCenter,
        BoundsBottomCenter,
        WorldBoundsCenter,
        WorldBoundsTopCenter,
        WorldBoundsBottomCenter,
    }



    //
    // class BrushSettings
    //
    [System.Serializable]
    public class BrushSettings
    {
        // Brush Tool
        public float            brushRadius;
        public float            brushSpacing;
        public OverlapCheckMode brushOverlapCheckMode;
        public float            brushOverlapDistance;
        public OverlapCheckObjects brushOverlapCheckObjects;
        public LayerMask        brushOverlapCheckLayers;

        // Position
        public float            surfaceOffset;

        // Orientation
        public TransformMode    orientationTransformMode;
        public OrientationMode  orientationMode;
        public bool             alongBrushStroke;
        public Vector3          rotation;
        public float            randomizeOrientationX;
        public float            randomizeOrientationY;
        public float            randomizeOrientationZ;

        // Scale
        public TransformMode    scaleTransformMode;
        public AxisMode         scaleMode;
        public float            scaleUniformMin;
        public float            scaleUniformMax;
        public Vector3          scalePerAxisMin;
        public Vector3          scalePerAxisMax;

        // Pin Tool
        public bool             pinFixedRotation;
        public Vector3          pinFixedRotationValue;
        public bool             pinFixedScale;
        public Vector3          pinFixedScaleValue;

        // Place Tool
        public Vector3          placeEulerAngles;
        public float            placeScale;

        // Throw Tool
        public float            throwRadius;
        public float            throwSpacing;
        public float            throwHeight;
        public float            throwVelocity;
        public Vector3          throwRandomRotation;
        public Vector2          throwScaleMinMax;

        // Multibrush
        [System.Serializable]
        public struct MultibrushSlot
        {
            public bool             enabled;
            public float            raiting;
            public Vector3          position;
            public Vector3          rotation;
            public Vector3          scale;
            public PivotMode        pivotMode;
            public Vector3          pivotOffset;
        }

        public const int        kNumMultibrushSlots = 32;

        public bool             multibrushEnabled;
        public MultibrushSlot[] multibrushSlots = new MultibrushSlot[kNumMultibrushSlots];
        public bool             multibrushPaintSelectedSlot;
        public MultibrushMode   multibrushMode;
        public string           multibrushPattern;
        public bool             multibrushPatternContinue;

        // Slope Filter
        public bool             slopeEnabled;
        public float            slopeAngleMin;
        public float            slopeAngleMax;
        public SlopeVector      slopeVector;
        public Vector3          slopeVectorCustom;
        public bool             slopeVectorFlip;

        // Grid
        public bool             gridEnabled;
        public Vector2          gridOrigin;
        public Vector2          gridStep;
        public GridPlane        gridPlane;
        public Vector3          gridNormal;
        public float            gridAngle;



        public BrushSettings() {
            Reset();
        }

        public BrushSettings(BrushSettings other)	{
            CopyFrom(other);
        }

        public void Reset()
        {
            multibrushEnabled         = false;
            slopeEnabled              = false;
            gridEnabled               = false;

            ResetBrush();
            ResetPinOrientation();
            ResetPinScale();
            ResetPlaceOrientation();
            ResetPlaceScale();
            ResetThrowSettings();
            ResetThrowOrientation();
            ResetThrowScale();
            ResetPosition();
            ResetOrientation();
            ResetScale();
            ResetSlopeFilter();
            ResetMultibrush();
            ResetGrid();

        }

        public void ResetBrush()
        {
            brushRadius                 = 1.0f;
            brushSpacing                = 0.5f;
            brushOverlapCheckMode       = OverlapCheckMode.None;
            brushOverlapDistance        = 0.5f;
            brushOverlapCheckObjects    = OverlapCheckObjects.SamePlaceLayer;
            brushOverlapCheckLayers     = 0;
        }

        public void ResetPinOrientation()
        {
            pinFixedRotation = false;
            pinFixedRotationValue = new Vector3(0, 0, 0);
        }

        public void ResetPinScale()
        {
            pinFixedScale = false;
            pinFixedScaleValue = new Vector3(1, 1, 1);
        }

        public void ResetPlaceOrientation()
        {
            orientationTransformMode = TransformMode.Relative;
            orientationMode = OrientationMode.SurfaceNormal;
            placeEulerAngles = new Vector3(0, 0, 0);
        }

        public void ResetPlaceScale()
        {
            scaleTransformMode = TransformMode.Relative;
            placeScale = 1.0f;
        }

        public void ResetThrowSettings()
        {
            throwRadius = 1.0f;
            throwSpacing = 1.0f;
            throwVelocity = 1.0f;
            throwHeight = 3.0f;
        }

        public void ResetThrowOrientation()
        {
            orientationTransformMode = TransformMode.Relative;
            throwRandomRotation = new Vector3(100f, 100f, 100f);
        }

        public void ResetThrowScale()
        {
            scaleTransformMode = TransformMode.Relative;
            throwScaleMinMax = new Vector2(1f, 1f);
        }

        public void ResetPosition()
        {
            surfaceOffset              = 0f;
        }

        public void ResetOrientation()
        {
            orientationTransformMode    = TransformMode.Relative;
            orientationMode             = OrientationMode.SurfaceNormal;
            alongBrushStroke            = false;
            rotation                    = new Vector3(0, 0, 0);
            randomizeOrientationX       = 0.0f;
            randomizeOrientationY       = 0.0f;
            randomizeOrientationZ       = 0.0f;
        }

        public void ResetScale()
        {
            scaleTransformMode          = TransformMode.Relative;
            scaleMode                   = AxisMode.Uniform;
            scaleUniformMin             = 1.0f;
            scaleUniformMax             = 1.0f;
            scalePerAxisMin             = new Vector3(1, 1, 1);
            scalePerAxisMax             = new Vector3(1, 1, 1);
        }

        public void ResetMultibrushSlot(int slot)
        {
            multibrushSlots[slot].enabled     = true;
            multibrushSlots[slot].raiting     = 1.0f;
            multibrushSlots[slot].position    = new Vector3(0, 0, 0);
            multibrushSlots[slot].rotation    = new Vector3(0, 0, 0);
            multibrushSlots[slot].scale       = new Vector3(1, 1, 1);
            multibrushSlots[slot].pivotMode   = PivotMode.ObjectPivot;
            multibrushSlots[slot].pivotOffset = new Vector3(1, 1, 1);
        }

        public void ResetMultibrush()
        {
            multibrushPaintSelectedSlot = false;
            multibrushMode              = MultibrushMode.Random;
            multibrushPattern           = "0 1 2 3 5 6 7 8 9";
            multibrushPatternContinue   = false;

            for(int i = 0; i < multibrushSlots.Length; i++)
                ResetMultibrushSlot(i);
        }

        public void ResetPlaceScaleSettings()
        {
            placeScale = 1.0f;
        }

        public void ResetPlaceOrientationSettings()
        {
            placeEulerAngles = Vector3.zero;
        }

        public void ResetSlopeFilter()
        {
            slopeAngleMin               = 0.0f;
            slopeAngleMax               = 35.0f;
            slopeVector                 = SlopeVector.Y;
            slopeVectorCustom           = new Vector3(0, 1, 0);
            slopeVectorFlip             = false;
        }

        public void ResetGrid()
        {
            gridOrigin                  = new Vector3(0, 0, 0);
            gridStep                    = new Vector2(5, 5);
            gridPlane                   = GridPlane.XZ;
            gridNormal                  = new Vector3(0, 1, 0);
            gridAngle                   = 0.0f;
        }

        public void CopyFrom(BrushSettings other)
        {
            brushRadius                 = other.brushRadius;
            brushSpacing                = other.brushSpacing;
            brushOverlapCheckMode       = other.brushOverlapCheckMode;
            brushOverlapDistance        = other.brushOverlapDistance;
            brushOverlapCheckObjects    = other.brushOverlapCheckObjects;
            brushOverlapCheckLayers     = other.brushOverlapCheckLayers;

            surfaceOffset               = other.surfaceOffset;

            orientationTransformMode    = other.orientationTransformMode;
            orientationMode             = other.orientationMode;
            alongBrushStroke            = other.alongBrushStroke;
            rotation                    = other.rotation;
            randomizeOrientationX       = other.randomizeOrientationX;
            randomizeOrientationY       = other.randomizeOrientationY;
            randomizeOrientationZ       = other.randomizeOrientationZ;

            scaleTransformMode          = other.scaleTransformMode;
            scaleMode                   = other.scaleMode;
            scaleUniformMin             = other.scaleUniformMin;
            scaleUniformMax             = other.scaleUniformMax;
            scalePerAxisMin             = other.scalePerAxisMin;
            scalePerAxisMax             = other.scalePerAxisMax;

            pinFixedRotation            = other.pinFixedRotation;
            pinFixedRotationValue       = other.pinFixedRotationValue;
            pinFixedScale               = other.pinFixedScale;
            pinFixedScaleValue          = other.pinFixedScaleValue;

            placeScale                  = other.placeScale;
            placeEulerAngles            = other.placeEulerAngles;

            multibrushEnabled           = other.multibrushEnabled;
            multibrushPaintSelectedSlot = other.multibrushPaintSelectedSlot;
            multibrushMode              = other.multibrushMode;
            multibrushPattern           = other.multibrushPattern;
            multibrushPatternContinue   = other.multibrushPatternContinue;

            for(int i = 0; i < Mathf.Min(multibrushSlots.Length, other.multibrushSlots.Length); i++)
            {
                multibrushSlots[i] = other.multibrushSlots[i];
            }

            slopeEnabled                = other.slopeEnabled;
            slopeAngleMin               = other.slopeAngleMin;
            slopeAngleMax               = other.slopeAngleMax;
            slopeVector                 = other.slopeVector;
            slopeVectorCustom           = other.slopeVectorCustom;
            slopeVectorFlip             = other.slopeVectorFlip;

            gridEnabled                 = other.gridEnabled;
            gridOrigin                  = other.gridOrigin;
            gridStep                    = other.gridStep;
            gridPlane                   = other.gridPlane;
            gridNormal                  = other.gridNormal;
            gridAngle                   = other.gridAngle;
        }
    }



    //
    // class Prefab
    //
    [System.Serializable]
    public struct Prefab
    {
        public GameObject       gameObject;
    }

    //
    // class Brush
    //
    [System.Serializable]
    public class Brush
    {
        public string           name            = "";
        public BrushSettings    settings        = new BrushSettings();
        public Prefab[]         prefabSlots     = new Prefab[BrushSettings.kNumMultibrushSlots];
        public int              selectedSlot    = 0;
        public bool             selected        = false;
        public ColorTag         colorTag        = ColorTag.None;
        public Texture2D        customPreviewImage = null;


        [NonSerialized] private string          lastPatternString = "";
        [NonSerialized] private int             patternCurrentPosition = 0;
        [NonSerialized] private List<int>       parsedPattern = null;
        [NonSerialized] private float           ratingSum = 0f;
        [NonSerialized] private int             nextPrefabForPlace = -1;

        [NonSerialized] public readonly int id = s_idCounter++;
        static int s_idCounter;




        public Brush()
        {
        }

        public Brush(GameObject gameObject)
        {
            name = gameObject.name;
            prefabSlots[0].gameObject = gameObject;
        }

        public Brush(Brush other)
        {
            name = other.name;
            other.prefabSlots.CopyTo(prefabSlots, 0);
            settings.CopyFrom(other.settings);
        }



        public void SelectPrefab(int index)
        {
            index = Mathf.Clamp(index, 0, prefabSlots.Length-1);
            if(index != selectedSlot)
            {
                Undo.RegisterCompleteObjectUndo(PrefabPainterSettings.current, "PP: Select Prefab");
                selectedSlot = index;
            }
        }


        public void BeginStroke()
        {
            if(!settings.multibrushEnabled)
            {
                nextPrefabForPlace = -1;
            }
            else
            {
                switch(settings.multibrushMode)
                {
                case MultibrushMode.Random:
                    {
                        ratingSum = 0.0f;

                        for(int i = 0; i < prefabSlots.Length; i++)
                        {
                            if(prefabSlots[i].gameObject != null && settings.multibrushSlots[i].enabled)
                            {
                                ratingSum += settings.multibrushSlots[i].raiting;
                            }
                        }
                    }
                    break;
                case MultibrushMode.Pattern:
                    {
                        if(parsedPattern == null)
                            parsedPattern = new List<int>(16);

                        if(lastPatternString != settings.multibrushPattern)
                        {
                            parsedPattern.Clear();
                            lastPatternString = settings.multibrushPattern;

                            int value = 0;
                            char[] separator = { ' ',  ';', ',' };
                            string[] patternKeys = lastPatternString.Split(separator, StringSplitOptions.RemoveEmptyEntries);

                            for(int i = 0; i < patternKeys.Length; i++)
                            {
                                if(int.TryParse(patternKeys[i], out value))
                                    parsedPattern.Add(value);
                            }
                        }

                        if(!settings.multibrushPatternContinue)
                        {
                            nextPrefabForPlace = -1;
                            patternCurrentPosition = 0;
                        }
                        else {
                            patternCurrentPosition = Mathf.Clamp(patternCurrentPosition, 0, parsedPattern.Count-1);
                        }

                    }
                    break;
                }
            }
        }


        public void EndStroke()
        {
        }


        public void PrepareNextPrefabForPlace()
        {
            nextPrefabForPlace = GetNextPrefab();
        }

        public int GetPrefabSlotForPlace()
        {
            if(!settings.multibrushEnabled)
            {
                return GetFirstAssociatedPrefabSlot();
            }

            if(settings.multibrushPaintSelectedSlot)
            {
                return selectedSlot;
            }

            if(nextPrefabForPlace == -1)
            {
                PrepareNextPrefabForPlace();
            }

            return nextPrefabForPlace;
        }


        int GetNextPrefab()
        {
            switch(settings.multibrushMode)
            {
            case MultibrushMode.Random:
                {
                    if(ratingSum < 0.01f)
                        return GetFirstAssociatedPrefabSlot();

                    float v = UnityEngine.Random.value * ratingSum;

                    float sum = 0.0f;
                    for(int i = 0; i < prefabSlots.Length; i++)
                    {
                        if(prefabSlots[i].gameObject != null && settings.multibrushSlots[i].enabled)
                        {
                            if(v >= sum && v <= sum + settings.multibrushSlots[i].raiting)
                                return i;

                            sum += settings.multibrushSlots[i].raiting;
                        }
                    }

                    return GetFirstAssociatedPrefabSlot();
                }
            case MultibrushMode.Pattern:
                {
                    if(parsedPattern.Count == 0)
                        return -1;

                    int prefabIndex = parsedPattern[patternCurrentPosition];

                    patternCurrentPosition ++;

                    if(patternCurrentPosition >= parsedPattern.Count)
                        patternCurrentPosition = 0;

                    if(prefabIndex >= 0 && prefabIndex < prefabSlots.Length)
                    {
                        if(prefabSlots[prefabIndex].gameObject != null)
                            return prefabIndex;
                    }

                    return -1;
                }
            }

            return -1;
        }




        public int GetFirstAssociatedPrefabSlot()
        {
            for(int i = 0; i < prefabSlots.Length; i++)
            {
                if(prefabSlots[i].gameObject != null)
                    return i;
            }
            return -1;
        }



        public GameObject GetFirstAssociatedPrefab()
        {
            for(int i = 0; i < prefabSlots.Length; i++)
            {
                if(prefabSlots[i].gameObject != null)
                    return prefabSlots[i].gameObject;
            }
            return null;
        }



        public void AssignPrefab(GameObject gameObject, int prefabSlot)
        {
            if(prefabSlot < 0 || prefabSlot >= prefabSlots.Length)
                return;

            if(prefabSlots[prefabSlot].gameObject != gameObject)
            {
                Undo.RegisterCompleteObjectUndo(PrefabPainterSettings.current, "PP: Assign Prefab");

                if(!settings.multibrushEnabled)
                    name = gameObject.name;

                prefabSlots[prefabSlot].gameObject = gameObject;
            }
        }

        public void AssignPrefabToEmptySlot(GameObject gameObject)
        {
            for(int prefabSlot = 0; prefabSlot < prefabSlots.Length; prefabSlot++)
            {
                if(prefabSlots[prefabSlot].gameObject == null)
                {
                    Undo.RegisterCompleteObjectUndo(PrefabPainterSettings.current, "PP: Assign Prefab");
                    prefabSlots[prefabSlot].gameObject = gameObject;
                    return;
                }
            }
        }

        public void ClearPrefab(int index)
        {
            if(index < 0 || index >= prefabSlots.Length)
                return;

            if(prefabSlots[index].gameObject != null)
            {
                Undo.RegisterCompleteObjectUndo (PrefabPainterSettings.current, "PP: Clear Prefab Slot");
                prefabSlots[index].gameObject = null;
            }
        }
    }





    //
    // class Tab
    //
    [System.Serializable]
    public class Tab
    {
        public string name;
        public List<Brush> brushes = new List<Brush>();

        public float levelsGamma = 1.0f;
        public float levelsInBlack = 0.0f;
        public float levelsInWhite = 255.0f;
        public float levelsOutBlack = 0.0f;
        public float levelsOutWhite = 255.0f;


        [NonSerialized] public readonly int id = s_idCounter++;
        static int s_idCounter;

        public Tab()
        {
            name = "";
        }

        public Tab(string newName)
        {
            name = newName;
        }

        public void ResetLevels()
        {
            levelsGamma = 1.0f;
            levelsInBlack = 0.0f;
            levelsInWhite = 255.0f;
            levelsOutBlack = 0.0f;
            levelsOutWhite = 255.0f;
        }

        public bool HasMultipleSelectedBrushes()
        {
            int selectedCount = 0;
            for (int i = 0; i < brushes.Count; i++)
            {
                if (brushes[i].selected)
                {
                    selectedCount++;

                    if (selectedCount > 1)
                        return true;
                }
            }
            return false;
        }

        public bool HasSelectedBrushes()
        {
            for (int i = 0; i < brushes.Count; i++)
            {
                if (brushes[i].selected)
                    return true;
            }
            return false;
        }

        public int GetFirstSelectedBrushIndex()
        {
            for (int i = 0; i < brushes.Count; i++)
            {
                if (brushes[i].selected)
                    return i;
            }
            return -1;
        }

        public Brush GetFirstSelectedBrush()
        {
            int index = GetFirstSelectedBrushIndex();
            if(index != -1)
                return brushes[index];

            return null;
        }

        public int GetBrushCount() {
            return brushes.Count;
        }

        public bool IsBrushSelected(int brushIndex)
        {
            if (brushIndex >= 0 && brushIndex < brushes.Count)
            {
                return brushes[brushIndex].selected;
            }
            return false;
        }


        public void SelectBrush(int brushIndex)
        {
            if (brushIndex >= 0 && brushIndex < brushes.Count)
            {
                if(brushes[brushIndex].selected != true || HasMultipleSelectedBrushes())
                {
                    Undo.RegisterCompleteObjectUndo (PrefabPainterSettings.current, "PP: Selection Change");

                    brushes.ForEach ((preset) => preset.selected = false);
                    brushes[brushIndex].selected = true;
                }
            }
        }

        public void SelectBrushAdditive(int brushIndex)
        {
            if (brushIndex >= 0 && brushIndex < brushes.Count)
            {
                Undo.RegisterCompleteObjectUndo (PrefabPainterSettings.current, "PP: Selection Change");
                brushes[brushIndex].selected = !brushes[brushIndex].selected;
            }
        }

        public void SelectBrushRange(int toBrushIndex)
        {
            if (toBrushIndex < 0 && toBrushIndex >= brushes.Count)
                return;

            int rangeMin = toBrushIndex;
            int rangeMax = toBrushIndex;

            for (int i = 0; i < brushes.Count; i++)
            {
                if (brushes[i].selected)
                {
                    rangeMin = Mathf.Min(rangeMin, i);
                    rangeMax = Mathf.Max(rangeMax, i);
                }
            }

            for (int i = rangeMin; i <= rangeMax; i++) {
                if (brushes[i].selected != true)
                {
                    Undo.RegisterCompleteObjectUndo (PrefabPainterSettings.current, "PP: Selection Change");
                    break;
                }
            }

            for (int i = rangeMin; i <= rangeMax; i++) {
                brushes[i].selected = true;
            }
        }

        public void SelectAllBrushes()
        {
            for (int i = 0; i < brushes.Count; i++)
            {
                if (!brushes[i].selected)
                {
                    Undo.RegisterCompleteObjectUndo (PrefabPainterSettings.current, "PP: Selection Change");
                    brushes.ForEach ((brush) => brush.selected = true);
                    break;
                }
            }
        }

        public void InvertSelection()
        {
            if(brushes.Count > 0)
            {
                Undo.RegisterCompleteObjectUndo (PrefabPainterSettings.current, "PP: Selection Change");
                brushes.ForEach ((brush) => brush.selected = !brush.selected);
            }
        }


        public void DeselectAllBrushes()
        {
            if(HasSelectedBrushes())
            {
                Undo.RegisterCompleteObjectUndo (PrefabPainterSettings.current, "PP: Selection Change");
                brushes.ForEach ((brush) => brush.selected = false);
            }
        }


        public void InsertSelectedBrushes(int index, bool after)
        {
            if (!HasSelectedBrushes ())
                return;

            List<Brush> selectedBrushes = new List<Brush>();
            brushes.ForEach ((brush) => { if(brush.selected) selectedBrushes.Add(brush); });

            if(selectedBrushes.Count > 0)
            {
                Undo.RegisterCompleteObjectUndo (PrefabPainterSettings.current, "PP: Move Brush(es)");

                index += after ? 1 : 0;
                index = Mathf.Clamp(index, 0, brushes.Count);

                brushes.Insert(index, null);    // insert null marker
                brushes.RemoveAll (b => b != null && b.selected); // remove all selected
                brushes.InsertRange(brushes.IndexOf(null), selectedBrushes); // insert selected brushes after null marker
                brushes.RemoveAll ((b) => b == null); // remove null marter
            }
        }


        public void DuplicateSelectedBrushes()
        {
            if (!HasSelectedBrushes ())
                return;

            Undo.RegisterCompleteObjectUndo(PrefabPainterSettings.current, "PP: Duplicate Brush(es)");

            for (int brushIndex = 0; brushIndex < brushes.Count; brushIndex++)
            {
                if (brushes[brushIndex].selected)
                {
                    Brush duplicate = new Brush (brushes [brushIndex]);
                    duplicate.name += " copy";

                    brushes [brushIndex].selected = false;
                    duplicate.selected = true;

                    brushes.Insert(brushIndex+1, duplicate);

                    brushIndex++; // move over new inserted duplicate
                }
            }
        }

        public void DeleteSelectedBrushes()
        {
            if (!HasSelectedBrushes ())
                return;

            Undo.RegisterCompleteObjectUndo (PrefabPainterSettings.current, "PP: Delete Brush(es)");

            brushes.RemoveAll ((brush) => brush.selected);
        }

        public void ResetSelectedBrushes()
        {
            if (!HasSelectedBrushes ())
                return;

            Undo.RegisterCompleteObjectUndo (PrefabPainterSettings.current, "PP: Reset Brush(es)");

            brushes.ForEach ((brush) => { if(brush.selected) brush.settings.Reset(); } );
        }

        public void SetSelectedBrushesTag(ColorTag tag)
        {
            Undo.RegisterCompleteObjectUndo (PrefabPainterSettings.current, "PP: Set Brush(es) Tag");

            brushes.ForEach ((brush) => { if(brush.selected) brush.colorTag = tag; });
        }

        public void ArrangeBrushesByName()
        {
            Undo.RegisterCompleteObjectUndo (PrefabPainterSettings.current, "PP: Arrange Brushes By Name");

            brushes.Sort (delegate(Brush x, Brush y)
            {
                return EditorUtility.NaturalCompare(x.name, y.name);
            });
        }
    }











    //
    // class PrefabPainterSettings
    //
    public class PrefabPainterSettings : ScriptableObject
    {
        [SerializeField] private List<Tab>  tabs = new List<Tab>();
        [SerializeField] private int        activeTab = 0;


        // Common settings
        public bool             paintOnSelected;
        public LayerMask        paintLayers;
        public LayerMask        ignoreLayers;
        public Placement        placeUnder;
        public bool             overwritePrefabLayer;
        public int              prefabPlaceLayer;

        // Pin Tool
        public float            pinSnapRotationValue;
        public float            pinSnapScaleValue;
        public bool             pinSnapRotation;
        public bool             pinSnapScale;

        // Place Tool
        public float            placeAngleStep;
        public float            placeScaleStep;

        // Erase Tool
        public float            eraseBrushRadius;
        public bool             eraseByLayer;
        public LayerMask        eraseLayers;

        // Select Tool
        public float            selectBrushRadius;
        public bool             selectByLayer;
        public LayerMask        selectLayers;

        // Modify Tool
        public float            modifyBrushRadius;
        public float            modifyStrength;
        public bool             modifyByLayer;
        public LayerMask        modifyLayers;
        public PivotMode        modifyPivotMode;
        public bool             modifyRandomRotation;
        public Vector3          modifyRandomRotationValues;
        public Vector3          modifyRotationValues;
        public float            modifyScale;
        public float            modifyScaleRandomize;

        // Orient Tool
        public Vector3          orientRotation;
        public bool             orientLockUp;
        public PivotMode        orientPivotMode;
        public bool             orientSameDirection;
        public bool             orientFlipDirection;

        // Move Tool
        public float            moveSurfaceOffset;
        public PivotMode        movePivotMode;
        public bool             moveLockUp;
        public OrientationMode  moveOrientationMode;

        // Settings tab
        public float            maxBrushRadius;
        public float            maxBrushSpacing;
        public SurfaceCoords    surfaceCoords;
        public bool             hideSceneSettingsObject;
        public bool             groupPrefabs;
        public string           groupName;
        public float            gridRaycastHeight;
        public Color            handlesColor;
        public bool             useAdditionalVertexStreams;
        public bool             enableToolsShortcuts;
        public bool             disableUndo;
        public float            throwPhysicsTimeStep; // Throw Tool

          // Clipboard variables
        [NonSerialized] private BrushSettings   copiedSettings = null;
        [NonSerialized] private Brush[]         copiedBrushes = null;


        public static PrefabPainterSettings current = null;


        public PrefabPainterSettings()
        {
            ResetCommonSettings();
            ResetPinSnapSettings();
            ResetPlaceSettings();
            ResetEraseSettings();
            ResetSelectSettings();
            ResetModifySettings();
            ResetModifyPositionSettings();
            ResetModifyOrientationSettings();
            ResetModifyScaleSettings();
            ResetOrientSettings();
            ResetMoveSettings();
            ResetToolSettings ();
        }


        public void ResetCommonSettings()
        {
            paintOnSelected         = false;
            paintLayers             = -1;
            ignoreLayers            = 0;
            placeUnder              = Placement.World;
            overwritePrefabLayer    = false;
            prefabPlaceLayer        = 0;
            groupPrefabs            = true;
    }


        public void ResetPinSnapSettings()
        {
            pinSnapRotationValue     = 15.0f;
            pinSnapScaleValue        = 0.1f;
            pinSnapRotation          = false;
            pinSnapScale             = false;
        }

        public void ResetPlaceSettings()
        {
            placeAngleStep = 15f;
            placeScaleStep = 0.1f;
        }

        public void ResetEraseSettings()
        {
            eraseBrushRadius        = 5.0f;
            eraseByLayer            = false;
            eraseLayers             = -1;
        }

        public void ResetSelectSettings()
        {
            selectBrushRadius       = 5.0f;
            selectByLayer           = false;
            selectLayers            = -1;
        }

        public void ResetModifySettings()
        {
            modifyBrushRadius       = 5.0f;
            modifyStrength          = 1.0f;
            modifyByLayer           = false;
            modifyLayers            = -1;
        }

        public void ResetModifyPositionSettings()
        {
            modifyPivotMode = PivotMode.BoundsBottomCenter;
        }

        public void ResetModifyOrientationSettings()
        {
            modifyRandomRotation = false;
            modifyRandomRotationValues = Vector3.zero;
            modifyRotationValues = Vector3.zero;
        }

        public void ResetModifyScaleSettings()
        {
            modifyScaleRandomize = 0f;
            modifyScale = 0.0f;
        }

        public void ResetOrientSettings()
        {
            orientRotation      = new Vector3(0, 0, 0);
            orientLockUp        = true;
            orientPivotMode     = PivotMode.BoundsBottomCenter;
            orientSameDirection = false;
            orientFlipDirection = false;
        }

        public void ResetMoveSettings()
        {
            moveSurfaceOffset   = 0f;
            movePivotMode       = PivotMode.BoundsBottomCenter;
            moveLockUp          = false;
            moveOrientationMode = OrientationMode.SurfaceNormal;
        }

        public void ResetToolSettings()
        {
            maxBrushRadius          = 20.0f;
            maxBrushSpacing         = 5.0f;
            surfaceCoords           = SurfaceCoords.AroundX;
            hideSceneSettingsObject = true;
            groupName               = "_group";
            gridRaycastHeight       = 5.0f;
            handlesColor            = new Color(1, 0, 0, 1);
            useAdditionalVertexStreams = false;
            enableToolsShortcuts    = false;
            disableUndo             = false;
            throwPhysicsTimeStep    = 0.02f;
        }


        public void SetActiveTabIndex(int index)
        {
            if(index != activeTab)
            {
                activeTab = Mathf.Clamp(index, 0, tabs.Count-1);
            }
        }

        public int GetActiveTabIndex()
        {
            // if tabs list is empty, add default tab
            if(tabs.Count == 0)
            {
                tabs.Add(new Tab("New Tab"));
                activeTab = 0;
            }

            activeTab = Mathf.Clamp(activeTab, 0, tabs.Count-1);
            return activeTab;
        }

        public Tab GetActiveTab()
        {
            return tabs[GetActiveTabIndex()];
        }

        public Tab GetTab(int index)
        {
            return tabs[index];
        }

        public int GetTabCount()
        {
            return tabs.Count;
        }

        public int AddNewTab(string name, int index = -1)
        {
            Undo.RegisterCompleteObjectUndo(this, "PP: Add Tab");


            if(index != -1)
            {
                tabs.Insert(index, new Tab(name));
                return index;
            }

            tabs.Add(new Tab(name));
            return tabs.Count-1;
        }

        public void DeleteTab(int tabIndex)
        {
            if(tabIndex >= 0 && tabIndex < tabs.Count && tabs.Count > 1)
            {
                Undo.RegisterCompleteObjectUndo(this, "PP: Delete Tab");
                tabs.RemoveAt(tabIndex);
            }
        }

        public void DuplicateTab(int tabIndex)
        {
            if(tabIndex >= 0 && tabIndex < tabs.Count)
            {
                Undo.RegisterCompleteObjectUndo(this, "PP: Duplicate Tab");

                Tab newTab = new Tab(tabs[tabIndex].name);

                tabs[tabIndex].brushes.ForEach((b) => { newTab.brushes.Add(new Brush(b)); } );

                tabs.Insert(tabIndex+1, newTab);

                activeTab = tabs.IndexOf(newTab);
            }
        }

        public void InsertSelectedTab(int index, bool after)
        {
            Undo.RegisterCompleteObjectUndo (PrefabPainterSettings.current, "PP: Move Tab");

            index += after ? 1 : 0;
            index = Mathf.Clamp(index, 0, tabs.Count);

            Tab tab = tabs[activeTab];
            tabs[activeTab] = null;
            tabs.Insert(index, tab);
            tabs.RemoveAll (t => t == null);
            activeTab = tabs.IndexOf(tab);
        }

        public void MoveBrushes(Brush[] brushes, Tab tab)
        {
            Undo.RegisterCompleteObjectUndo(this, "PP: Move brush(es)");

            foreach(Brush brush in brushes)
            {
                Tab brushTab = null;
                foreach(Tab searchTab in tabs)
                {
                    if (searchTab.brushes.Contains(brush))
                        brushTab = searchTab;
                }

                if (brushTab != null)
                {
                    tab.brushes.Add(brush);
                    brushTab.brushes.Remove(brush);
                }
            }
        }


        public void ClipboardCopySettings()
        {
            Brush selectedBrush = GetActiveTab().GetFirstSelectedBrush();

            if(selectedBrush != null)
                copiedSettings = new BrushSettings(selectedBrush.settings);
            else
                copiedSettings = null;
        }

        public void ClipboardPasteSettings()
        {
            if (!GetActiveTab().HasSelectedBrushes() || copiedSettings == null)
                return;

            Undo.RegisterCompleteObjectUndo(this, "PP: Paste Brush Settings");

            foreach(Brush brush in GetActiveTab().brushes) {
                if (brush.selected)
                    brush.settings.CopyFrom(copiedSettings);
            }
        }

        public bool ClipboardIsCanPasteSettings() {
            return copiedSettings != null;
        }



        public void ClipboardCutBrushes()
        {
            if(GetActiveTab().HasSelectedBrushes())
            {
                Undo.RegisterCompleteObjectUndo(this, "PP: Cut Brush(es)");

                copiedBrushes =  GetActiveTab().brushes.FindAll((b) => b.selected).ToArray();
                GetActiveTab().brushes.RemoveAll((b) => b.selected);
            }
            else
                copiedBrushes = null;
        }

        public void ClipboardCopyBrushes()
        {
            if(GetActiveTab().HasSelectedBrushes())
            {
                List<Brush> copied = new List<Brush>();
                GetActiveTab().brushes.ForEach((b) => { if(b.selected) copied.Add(new Brush(b)); } );
                copiedBrushes = copied.ToArray();
            }
            else
                copiedBrushes = null;
        }

        public void ClipboardPasteBrushes()
        {
            if(copiedBrushes != null)
            {
                Undo.RegisterCompleteObjectUndo(this, "PP: Paste Brush(es)");

                GetActiveTab().brushes.ForEach((b) => b.selected = false);
                foreach(Brush brush in copiedBrushes)
                {
                    Brush newBrush = new Brush(brush);
                    newBrush.selected = true;
                    GetActiveTab().brushes.Add(newBrush);
                }
            }
        }

        public bool ClipboardIsCanPasteBrushes() {
            return copiedBrushes != null;
        }
    }

} // namespace

#endif //(UNITY_EDITOR)
