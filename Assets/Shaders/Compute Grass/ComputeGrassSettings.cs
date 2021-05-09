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
    [Min(0)] public float maxCameraDist;

    public int Checksum
    {
        get
        {
            int result = 0;
            result += (grassHeight * 7550).RoundToInt();
            result += (grassWidth * 3589).RoundToInt();
            result += (grassHeightRandom * 2954).RoundToInt();
            result += (grassWidthRandom * 1232).RoundToInt();
            result += grassSegments * 242;
            result += grassPerVertex * 2942;
            result += (randomPosition * 846).RoundToInt();
            result += (maxCameraDist * 3253).RoundToInt();
            return result;
        }
    }
}
