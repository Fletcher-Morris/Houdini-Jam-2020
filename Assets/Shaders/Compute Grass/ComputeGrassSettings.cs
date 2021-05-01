using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

[CreateAssetMenu(fileName = "Compute Grass")]
public class ComputeGrassSettings : ScriptableObject
{
    public ComputeGrassSettingsData SettingsData = default;
}

[System.Serializable]
public struct ComputeGrassSettingsData
{
    [Min(0)] public float grassHeight;
    [Min(0)] public float grassHeightRandom;
    [Min(0)] public float grassWidth;
    [Min(0)] public float grassWidthRandom;
    [Range(1, 4)] public int grassSegments;
    [Range(1, 8)] public int grassPerVertex;
    [Min(0)] public float randomPosition;

    public int Checksum
    {
        get
        {
            int result = 0;
            result += (grassHeight * 12345).RoundToInt();
            result += (grassWidth * 12345).RoundToInt();
            result += (grassHeightRandom * 12345).RoundToInt();
            result += (grassWidthRandom * 112345).RoundToInt();
            result += grassSegments;
            result += grassPerVertex * 123;
            result += (randomPosition * 1234).RoundToInt();
            return result;
        }
    }
}
