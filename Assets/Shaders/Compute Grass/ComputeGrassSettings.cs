using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

[CreateAssetMenu(fileName = "Compute Grass")]
public class ComputeGrassSettings : ScriptableObject
{
    public ComputeGrassSettingsData SettingsData = default;

    void OnValidate()
    {
        SettingsData.maxCameraDist = SettingsData.maxCameraDist.Clamp(SettingsData.minCamDist, float.MaxValue);
        SettingsData.SetChecksum(SettingsData.Checksum);
    }
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
    [Min(0)] public float minAltitude;
    [Min(0)] public float maxAltitude;
    [Min(0)] public float altitudeFade;
    [Range(-1,1)] public float camDotCuttoff;
    [Min(0)] public float minCamDist;

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
            result += (minAltitude * 4593).RoundToInt();
            result += (maxAltitude * 8552).RoundToInt();
            result += (altitudeFade * 2844).RoundToInt();
            result += (camDotCuttoff * 2723).RoundToInt();
            result += (minCamDist * 3951).RoundToInt();
            return result;
        }
    }

    [SerializeField] int m_checksum;
    public void SetChecksum(int _value) => m_checksum = _value;
}
