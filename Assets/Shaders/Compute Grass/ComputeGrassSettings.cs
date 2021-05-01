using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "Compute Grass")]
public class ComputeGrassSettings : ScriptableObject
{
    public ComputeGrassSettingsData SettingsData = default;
}

[System.Serializable]
[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
public struct ComputeGrassSettingsData
{
    float grassHeight;
    float grassHeightRandom;
    float grassWidth;
    float grassWidthRandom;
    [Range(1, 4)] int grassSegments;
}
